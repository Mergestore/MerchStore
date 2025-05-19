using System;
using FluentAssertions;
using MerchStore.Domain.ValueObjects;
using Xunit;

namespace MerchStore.Domain.Tests.ValueObjects;

public class ReviewStatsTests
{
    private readonly Guid _validProductId = Guid.NewGuid();
    private const double _validAverageRating = 4.5;
    private const int _validReviewCount = 10;

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var stats = new ReviewStats(_validProductId, _validAverageRating, _validReviewCount);

        // Assert
        stats.ProductId.Should().Be(_validProductId);
        stats.AverageRating.Should().Be(_validAverageRating);
        stats.ReviewCount.Should().Be(_validReviewCount);
    }

    [Fact]
    public void Constructor_WithEmptyProductId_ShouldThrowArgumentException()
    {
        // Arrange
        Guid emptyProductId = Guid.Empty;

        // Act
        Action act = () => new ReviewStats(emptyProductId, _validAverageRating, _validReviewCount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Product ID kan inte vara tomt*")
            .And.ParamName.Should().Be("productId");
    }

    [Theory]
    [InlineData(-0.1)]  // Slightly below minimum
    [InlineData(5.1)]   // Slightly above maximum
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_WithInvalidAverageRating_ShouldThrowArgumentOutOfRangeException(double invalidRating)
    {
        // Arrange & Act
        Action act = () => new ReviewStats(_validProductId, invalidRating, _validReviewCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Snittbetyget måste vara mellan 0 och 5*")
            .And.ParamName.Should().Be("averageRating");
    }

    [Theory]
    [InlineData(0)]     // Edge case - valid
    [InlineData(5)]     // Edge case - valid
    [InlineData(2.5)]   // Middle value - valid
    public void Constructor_WithValidAverageRating_ShouldNotThrow(double validRating)
    {
        // Arrange & Act
        Action act = () => new ReviewStats(_validProductId, validRating, _validReviewCount);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_WithNegativeReviewCount_ShouldThrowArgumentOutOfRangeException(int invalidCount)
    {
        // Arrange & Act
        Action act = () => new ReviewStats(_validProductId, _validAverageRating, invalidCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Antal recensioner får inte vara negativt*")
            .And.ParamName.Should().Be("reviewCount");
    }

    [Theory]
    [InlineData(0)]     // Edge case - valid
    [InlineData(1)]     // Minimum positive value - valid
    [InlineData(1000)]  // Large value - valid
    public void Constructor_WithValidReviewCount_ShouldNotThrow(int validCount)
    {
        // Arrange & Act
        Action act = () => new ReviewStats(_validProductId, _validAverageRating, validCount);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EqualityOperator_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var stats1 = new ReviewStats(_validProductId, _validAverageRating, _validReviewCount);
        var stats2 = new ReviewStats(_validProductId, _validAverageRating, _validReviewCount);

        // Act & Assert
        stats1.Should().Be(stats2);
        (stats1 == stats2).Should().BeTrue();
        (stats1 != stats2).Should().BeFalse();
        stats1.GetHashCode().Should().Be(stats2.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var stats1 = new ReviewStats(_validProductId, _validAverageRating, _validReviewCount);
        var stats2 = new ReviewStats(_validProductId, _validAverageRating, _validReviewCount + 1);

        // Act & Assert
        stats1.Should().NotBe(stats2);
        (stats1 == stats2).Should().BeFalse();
        (stats1 != stats2).Should().BeTrue();
    }
}

// kör test med dotnet test --filter "FullyQualifiedName~ReviewStatsTests"
