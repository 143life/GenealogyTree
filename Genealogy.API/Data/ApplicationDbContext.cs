using Microsoft.EntityFrameworkCore;
using Genealogy.API.Entities;

namespace Genealogy.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Relationship> Relationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Конфигурация User остается без изменений
			modelBuilder.Entity<User>(entity =>
			{
				entity.HasIndex(u => u.Username).IsUnique();
				entity.HasIndex(u => u.Email).IsUnique();
				entity.Property(u => u.Username).HasMaxLength(50);
				entity.Property(u => u.Email).HasMaxLength(100);
				entity.Property(u => u.PasswordHash).HasMaxLength(100);
				entity.Property(u => u.Role).HasConversion<string>();
			});

			// Конфигурация Person - ИСПРАВЛЕННЫЙ CHECK CONSTRAINT
			modelBuilder.Entity<Person>(entity =>
			{
				entity.Property(p => p.FirstName).HasMaxLength(100);
				entity.Property(p => p.LastName).HasMaxLength(100);
				entity.Property(p => p.MiddleName).HasMaxLength(100);
				entity.Property(p => p.Biography).HasMaxLength(2000);
				entity.Property(p => p.Gender).HasConversion<string>();

				// ИСПРАВЛЕННАЯ ВАЛИДАЦИЯ: используем имена в нижнем регистре
				entity.ToTable(tb => tb.HasCheckConstraint(
					"CK_Person_DeathDateAfterBirthDate",
					@"""DeathDate"" IS NULL OR ""DeathDate"" > ""BirthDate"""));
			});

			// Конфигурация Relationship - ИСПРАВЛЕННЫЙ CHECK CONSTRAINT
			modelBuilder.Entity<Relationship>(entity =>
			{
				entity.Property(r => r.Note).HasMaxLength(500);
				entity.Property(r => r.RelationshipType).HasConversion<string>();

				// ИСПРАВЛЕННЫЙ CHECK: используем имена в нижнем регистре
				entity.ToTable(tb => tb.HasCheckConstraint(
					"CK_Relationship_NotSelf",
					@"""PersonId"" != ""RelatedPersonId"""));

				// Внешние ключи
				entity.HasOne(r => r.Person)
					.WithMany(p => p.RelationshipsFrom)
					.HasForeignKey(r => r.PersonId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(r => r.RelatedPerson)
					.WithMany(p => p.RelationshipsTo)
					.HasForeignKey(r => r.RelatedPersonId)
					.OnDelete(DeleteBehavior.Restrict);

				// Индекс для быстрого поиска связей
				entity.HasIndex(r => new { r.PersonId, r.RelationshipType });
				entity.HasIndex(r => new { r.RelatedPersonId, r.RelationshipType });
			});

			// Связь User -> Person
			modelBuilder.Entity<Person>()
				.HasOne(p => p.User)
				.WithMany(u => u.Persons)
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Автоматическое обновление UpdatedAt
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is User || e.Entity is Person)
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                    user.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is Person person)
                    person.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}