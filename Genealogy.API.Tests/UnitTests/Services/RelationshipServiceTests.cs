using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Genealogy.API.Data;
using Genealogy.API.Services;
using Genealogy.API.Entities;
using Genealogy.API.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.API.Tests.UnitTests.Services
{
    [TestFixture]
    public class RelationshipServiceTests
    {
        private ApplicationDbContext _context;
        private RelationshipService _service;

        [SetUp]
		public void SetUp()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDb_Relationship_" + Guid.NewGuid())
				.Options;

			_context = new ApplicationDbContext(options);

			// Добавляем тестового пользователя
			var user = new User 
			{ 
				Id = 1, 
				Username = "testuser", 
				Email = "test@test.com", 
				PasswordHash = "123",
				Role = UserRole.User
			};
			_context.Users.Add(user);

			// Добавляем тестовых персон
			var persons = new List<Person>
			{
				new Person { Id = 1, FirstName = "John", LastName = "Doe", UserId = 1 },
				new Person { Id = 2, FirstName = "Jane", LastName = "Doe", UserId = 1 },
				new Person { Id = 3, FirstName = "Child", LastName = "Doe", UserId = 1 },
				new Person { Id = 4, FirstName = "Alice", LastName = "Smith", UserId = 1 } // Новая персона для тестов
			};
			_context.Persons.AddRange(persons);

			// Добавляем существующую связь для теста дублирования и супружества
			var existingRelationship = new Relationship
			{
				PersonId = 1,
				RelatedPersonId = 2,
				RelationshipType = RelationshipType.Spouse
			};
			_context.Relationships.Add(existingRelationship);

			_context.SaveChanges();

			_service = new RelationshipService(_context);
		}

        [Test]
        public void CreateRelationshipAsync_SelfRelationship_ThrowsArgumentException()
        {
            // Arrange
            var dto = new RelationshipCreateDto
            {
                PersonId = 1,
                RelatedPersonId = 1,
                RelationshipType = RelationshipType.Spouse
            };
            
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _service.CreateRelationshipAsync(dto));
        }
        
        [Test]
        public void CreateRelationshipAsync_Duplicate_ThrowsArgumentException()
        {
            // Arrange
            var dto = new RelationshipCreateDto
            {
                PersonId = 1,
                RelatedPersonId = 2,
                RelationshipType = RelationshipType.Spouse
            };
            
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _service.CreateRelationshipAsync(dto));
        }
        
        [Test]
        public async Task CreateRelationshipAsync_ValidData_ReturnsRelationshipDto()
        {
            // Arrange
            var dto = new RelationshipCreateDto
            {
                PersonId = 1,
                RelatedPersonId = 3,
                RelationshipType = RelationshipType.ParentChild,
                Note = "Father and child"
            };
            
            // Act
            var result = await _service.CreateRelationshipAsync(dto);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PersonId, Is.EqualTo(1));
            Assert.That(result.RelatedPersonId, Is.EqualTo(3));
            Assert.That(result.RelationshipType, Is.EqualTo(RelationshipType.ParentChild));
            Assert.That(result.Note, Is.EqualTo("Father and child"));
        }

		[Test]
		public void CreateRelationshipAsync_Spouse_WhenPersonAlreadyHasSpouse_ThrowsArgumentException()
		{
			// Arrange: У персоны с ID=1 уже есть супруг (ID=2) из настроек SetUp.
			var dto = new RelationshipCreateDto
			{
				PersonId = 1,
				RelatedPersonId = 3, // Новая персона
				RelationshipType = RelationshipType.Spouse
			};

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
				await _service.CreateRelationshipAsync(dto));
			Assert.That(ex.Message, Does.Contain("already has a spouse"));
		}

		[Test]
		public void CreateRelationshipAsync_Spouse_WhenRelatedPersonAlreadyHasSpouse_ThrowsArgumentException()
		{
			// Arrange: У персоны с ID=2 уже есть супруг (ID=1) из настроек SetUp.
			// Создаем связь между ID=3 (новая) и ID=2 (уже есть супруг).
			var dto = new RelationshipCreateDto
			{
				PersonId = 3,
				RelatedPersonId = 2, // У этого человека уже есть супруг
				RelationshipType = RelationshipType.Spouse
			};

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
				await _service.CreateRelationshipAsync(dto));
			Assert.That(ex.Message, Does.Contain("already has a spouse"));
		}

		[Test]
		public async Task CreateRelationshipAsync_Sibling_WithoutCommonParent_ThrowsArgumentException()
		{
			// Arrange: Персона ID=1 и ID=2 не имеют общих родителей в тестовой базе.
			var dto = new RelationshipCreateDto
			{
				PersonId = 1,
				RelatedPersonId = 2,
				RelationshipType = RelationshipType.Sibling
			};

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
				await _service.CreateRelationshipAsync(dto));
			Assert.That(ex.Message, Does.Contain("Siblings must have at least one common parent"));
		}

		[Test]
		public async Task CreateRelationshipAsync_ParentChild_CreatesCycle_ThrowsArgumentException()
		{
			// Arrange: Сначала создадим связь ParentChild: 1 -> 2 (1 родитель 2)
			var parentChildDto = new RelationshipCreateDto
			{
				PersonId = 2,    // Ребенок
				RelatedPersonId = 1, // Родитель
				RelationshipType = RelationshipType.ParentChild
			};
			// Имитируем, что связь уже существует в базе, создав ее.
			await _service.CreateRelationshipAsync(parentChildDto); // 1 - родитель 2

			// Теперь попытаемся сделать 2 родителем 1 (цикл!)
			var cycleDto = new RelationshipCreateDto
			{
				PersonId = 1,
				RelatedPersonId = 2,
				RelationshipType = RelationshipType.ParentChild
			};

			// Act & Assert: Должна сработать валидация на цикл (IsAncestorAsync)
			var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
				await _service.CreateRelationshipAsync(cycleDto));
			// Проверяем, что в сообщении об ошибке есть ключевые слова
			Assert.That(ex.Message, Does.Contain("cycle").Or.Contains("ancestor").Or.Contains("descendant"));
		}
        
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}