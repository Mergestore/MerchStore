using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MergeStore.Models;

public class CustomerCartInfo
{
    public CustomerCartInfo(string? id, string? name, string? email, string? address, string? postalCode, string? city, string? state, string? phoneNumber)
    {
        Id = id;
        Name = name;
        Email = email;
        Address = address;
        PostalCode = postalCode;
        City = city;
        State = state;
        PhoneNumber = phoneNumber;
    }  
    public CustomerCartInfo(string? name, string? email, string? address, string? postalCode, string? city, string? state, string? phoneNumber)
    {
        Name = name;
        Email = email;
        Address = address;
        PostalCode = postalCode;
        City = city;
        State = state;
        PhoneNumber = phoneNumber;
    }
    
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Namn krävs")]
    [StringLength(20, ErrorMessage = "Namnet får inte överstiga 20 tecken")]
    [BsonElement("name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "E-post krävs")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
    [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Ogiltig e-postadress")]
    [BsonElement("email")]
    public string? Email { get; set; } 

    [Required(ErrorMessage = "Adress krävs")]
    [StringLength(100, ErrorMessage = "Adressen får inte överstiga 100 tecken")]
    [BsonElement("address")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Postnummer krävs")]
    [RegularExpression(@"^\d{3}\s?\d{2}$", ErrorMessage = "Ogiltigt postnummerformat")]
    [BsonElement("postalCode")]
    public string? PostalCode { get; set; }

    [Required(ErrorMessage = "Stad krävs")]
    [RegularExpression(@"^[A-ZÅÄÖa-zåäö]+([-\s][A-ZÅÄÖa-zåäö]+)*$", ErrorMessage = "Ogiltigt stadsnamn")]
    [BsonElement("city")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Län krävs")]
    [RegularExpression(@"^[A-ZÅÄÖa-zåäö]+(\s[A-ZÅÄÖa-zåäö]+)*(\slän)?$", ErrorMessage = "Ogiltigt län")]
    [BsonElement("state")]
    public string? State { get; set; }

    [Required(ErrorMessage = "Telefonnummer krävs")]
    [RegularExpression(@"^[0-9\-\+\s\(\)]{6,20}$", ErrorMessage = "Ogiltigt telefonnummerformat")]
    [BsonElement("phoneNumber")]
    public string? PhoneNumber { get; set; }
}