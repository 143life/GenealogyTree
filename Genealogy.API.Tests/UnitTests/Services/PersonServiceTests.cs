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
    public class PersonServiceTests
    {
        private ApplicationDbContext _context;
        private PersonService _service;

        [SetUp]
        public void SetUp()
        {
            // Используем InMemory базу данных (версия для .NET 9)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Person_" + Guid.NewGuid())
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
                new Person { Id = 1, FirstName = "Parent", LastName = "Test", UserId = 1 },
                new Person { Id = 2, FirstName = "Child1", LastName = "Test", UserId = 1 },
                new Person { Id = 3, FirstName = "Child2", LastName = "Test", UserId = 1 },
                new Person { Id = 4, FirstName = "Child3", LastName = "Test", UserId = 1 }
            };
            _context.Persons.AddRange(persons);
            
            var relationships = new List<Relationship>
            {
                new Relationship { PersonId = 2, RelatedPersonId = 1, RelationshipType = RelationshipType.ParentChild },
                new Relationship { PersonId = 3, RelatedPersonId = 1, RelationshipType = RelationshipType.ParentChild },
                new Relationship { PersonId = 4, RelatedPersonId = 1, RelationshipType = RelationshipType.ParentChild }
            };
            _context.Relationships.AddRange(relationships);
            
            _context.SaveChanges();
            
            _service = new PersonService(_context);
        }

        [Test]
        public async Task GetChildrenAsync_ValidPersonId_ReturnsChildren()
        {
            // Act
            var result = await _service.GetChildrenAsync(1);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            
            var childrenIds = result.Select(p => p.Id).ToList();
            Assert.That(childrenIds, Does.Contain(2));
            Assert.That(childrenIds, Does.Contain(3));
            Assert.That(childrenIds, Does.Contain(4));
        }
        
        [Test]
        public async Task GetParentsAsync_ValidPersonId_ReturnsParents()
        {
            // Act
            var result = await _service.GetParentsAsync(2);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        }
        
        [Test]
        public async Task GetSiblingsAsync_ValidPersonId_ReturnsSiblings()
        {
            // Act
            var result = await _service.GetSiblingsAsync(2);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            
            var siblingIds = result.Select(p => p.Id).ToList();
            Assert.That(siblingIds, Does.Contain(3));
            Assert.That(siblingIds, Does.Contain(4));
            Assert.That(siblingIds, Does.Not.Contain(2));
        }
        
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}