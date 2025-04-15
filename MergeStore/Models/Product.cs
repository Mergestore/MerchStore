using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MergeStore.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Produktnamn krävs")]
        [StringLength(100, ErrorMessage = "Produktnamnet får inte överstiga 100 tecken")]
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pris krävs")]
        [Range(0.01, 100000, ErrorMessage = "Priset måste vara större än 0")]
        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Lagersaldo kan inte vara mindre än 0")]
        [BsonElement("stockQuantity")]
        public int StockQuantity { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}