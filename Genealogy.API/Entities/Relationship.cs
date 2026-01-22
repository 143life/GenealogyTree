using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genealogy.API.Entities
{
    public enum RelationshipType
    {
        Spouse,
        ParentChild,
        Sibling
    }

    public class Relationship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Person")]
        public int PersonId { get; set; }

        [Required]
        [ForeignKey("RelatedPerson")]
        public int RelatedPersonId { get; set; }

        [Required]
        public RelationshipType RelationshipType { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        // Навигационные свойства
        public virtual Person Person { get; set; } = null!;
        public virtual Person RelatedPerson { get; set; } = null!;

        public override string ToString() => 
            $"Relationship {PersonId} -> {RelatedPersonId} ({RelationshipType})";
    }
}