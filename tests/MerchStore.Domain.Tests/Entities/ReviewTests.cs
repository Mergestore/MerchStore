using System;
using FluentAssertions;
using MerchStore.Domain.Entities;
using MerchStore.Domain.Enums;
using Xunit;

namespace MerchStore.Domain.Tests.Entities;

public class ReviewTests
{
    // 🔧 Vanliga giltiga testdata att återanvända i flera tester
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Guid _validProductId = Guid.NewGuid();
    private const string _validCustomerName = "Alice";
    private const string _validTitle = "Toppenprodukt!";
    private const string _validContent = "Jag är mycket nöjd med denna produkt.";
    private const int _validRating = 5;
    private readonly DateTime _validCreatedAt = DateTime.UtcNow;
    private const ReviewStatus _validStatus = ReviewStatus.Approved;

    [Fact]
    public void Constructor_WithValidValues_ShouldCreateReview()
    {
        // Arrange & Act
        var review = new Review(
            _validId,
            _validProductId,
            _validCustomerName,
            _validTitle,
            _validContent,
            _validRating,
            _validCreatedAt,
            _validStatus);

        // Assert
        review.Id.Should().Be(_validId);
        review.ProductId.Should().Be(_validProductId);
        review.CustomerName.Should().Be(_validCustomerName);
        review.Title.Should().Be(_validTitle);
        review.Content.Should().Be(_validContent);
        review.Rating.Should().Be(_validRating);
        review.CreatedAt.Should().Be(_validCreatedAt);
        review.Status.Should().Be(_validStatus);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCustomerName_ShouldThrow(string invalidName)
    {
        // Act
        Action act = () => new Review(
            _validId,
            _validProductId,
            invalidName!,
            _validTitle,
            _validContent,
            _validRating,
            _validCreatedAt,
            _validStatus);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Kundnamn får inte vara tomt*")
        .And.ParamName.Should().Be("customerName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTitle_ShouldThrow(string invalidTitle)
    {
        Action act = () => new Review(
            _validId,
            _validProductId,
            _validCustomerName,
            invalidTitle!,
            _validContent,
            _validRating,
            _validCreatedAt,
            _validStatus);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Titel får inte vara tom*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidContent_ShouldThrow(string invalidContent)
    {
        Action act = () => new Review(
            _validId,
            _validProductId,
            _validCustomerName,
            _validTitle,
            invalidContent!,
            _validRating,
            _validCreatedAt,
            _validStatus);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Innehåll får inte vara tomt*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Constructor_WithInvalidRating_ShouldThrow(int invalidRating)
    {
        Action act = () => new Review(
            _validId,
            _validProductId,
            _validCustomerName,
            _validTitle,
            _validContent,
            invalidRating,
            _validCreatedAt,
            _validStatus);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Betyget måste vara mellan 1 och 5*");
    }

    [Fact]
    public void Equals_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var review1 = new Review(
            _validId,
            _validProductId,
            _validCustomerName,
            _validTitle,
            _validContent,
            _validRating,
            _validCreatedAt,
            _validStatus);

        var review2 = new Review(
            _validId, // Samma ID
            Guid.NewGuid(), "Bob", "Annat", "Text", 4, DateTime.UtcNow.AddDays(-1), ReviewStatus.Pending);

        // Act & Assert
        review1.Should().Be(review2);
        (review1 == review2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var review1 = new Review(
            _validId,
            _validProductId,
            _validCustomerName,
            _validTitle,
            _validContent,
            _validRating,
            _validCreatedAt,
            _validStatus);

        var review2 = new Review(
            Guid.NewGuid(), // Olika ID
            _validProductId,
            _validCustomerName,
            _validTitle,
            _validContent,
            _validRating,
            _validCreatedAt,
            _validStatus);

        // Assert
        review1.Should().NotBe(review2);
        (review1 == review2).Should().BeFalse();
    }
}

// kör testet med dotnet test --filter "FullyQualifiedName~ReviewTests"
