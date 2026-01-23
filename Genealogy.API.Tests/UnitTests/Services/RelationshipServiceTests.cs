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
            
            // Добавляем тестовые данные
            var user = new User 
            { 
                Id = 1, 
                Username = "testuser", 
                Email = "test@test.com", 
                PasswordHash = "123"
            };
            _context.Users.Add(user);
            
            var persons = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", UserId = 1 },
                new Person { Id = 2, FirstName = "Jane", LastName = "Doe", UserId = 1 },
                new Person { Id = 3, FirstName = "Child", LastName = "Doe", UserId = 1 }
            };
            _context.Persons.AddRange(persons);
            
            // Добавляем существующую связь для теста дублирования
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
        
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}