using Genealogy.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Genealogy.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Проверяем, есть ли уже пользователи
            if (await context.Users.AnyAsync())
                return;

            // Создаем тестового пользователя (пароль: admin123)
            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = UserRole.Admin,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Создаем тестовые персоны
            var person1 = new Person
            {
                UserId = user.Id,
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                Gender = Gender.Male,
                BirthDate = new DateOnly(1980, 1, 15),
                Biography = "Основатель семьи Ивановых"
            };

            var person2 = new Person
            {
                UserId = user.Id,
                FirstName = "Мария",
                LastName = "Иванова",
                MiddleName = "Петровна",
                Gender = Gender.Female,
                BirthDate = new DateOnly(1982, 5, 20)
            };

            context.Persons.AddRange(person1, person2);
            await context.SaveChangesAsync();

            // Создаем связь между персонами
            var relationship = new Relationship
            {
                PersonId = person1.Id,
                RelatedPersonId = person2.Id,
                RelationshipType = RelationshipType.Spouse,
                Note = "Супружеская пара"
            };

            context.Relationships.Add(relationship);
            await context.SaveChangesAsync();
        }
    }
}