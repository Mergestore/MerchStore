using System;
using FluentAssertions;
using MerchStore.Domain.Enums;
using Xunit;

namespace MerchStore.Domain.Tests.Enums;

public class ReviewStatusTests
{
    [Fact]
    public void ReviewStatus_ShouldHaveExpectedValues()
    {
        // Arrange & Act - for enums, these steps are typically combined
        var values = Enum.GetValues<ReviewStatus>();

        // Assert
        values.Should().Contain(ReviewStatus.Pending);
        values.Should().Contain(ReviewStatus.Approved);
        values.Should().Contain(ReviewStatus.Rejected);
        values.Should().HaveCount(3); // Ensures no unexpected values are added
    }

    [Theory]
    [InlineData(ReviewStatus.Pending, "Pending")]
    [InlineData(ReviewStatus.Approved, "Approved")]
    [InlineData(ReviewStatus.Rejected, "Rejected")]
    public void ReviewStatus_ToString_ShouldReturnExpectedString(ReviewStatus status, string expected)
    {
        // Act
        string result = status.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ReviewStatus_DefaultValue_ShouldBePending()
    {
        // Arrange
        ReviewStatus defaultValue = default;

        // Act & Assert
        defaultValue.Should().Be(ReviewStatus.Pending);
    }
}

// kör test med dotnet test --filter "FullyQualifiedName~ReviewStatusTests"
