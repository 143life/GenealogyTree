using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genealogy.API.Entities
{
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public DateOnly? BirthDate { get; set; }
        public DateOnly? DeathDate { get; set; }

        [MaxLength(2000)]
        public string? Biography { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Relationship> RelationshipsFrom { get; set; } = new List<Relationship>();
        public virtual ICollection<Relationship> RelationshipsTo { get; set; } = new List<Relationship>();

        // Вычисляемые свойства
        [NotMapped]
        public int? Age
        {
            get
            {
                if (!BirthDate.HasValue) return null;
                var endDate = DeathDate ?? DateOnly.FromDateTime(DateTime.Today);
                return (endDate.DayNumber - BirthDate.Value.DayNumber) / 365;
            }
        }

        [NotMapped]
        public bool IsAlive => !DeathDate.HasValue;
    }
}