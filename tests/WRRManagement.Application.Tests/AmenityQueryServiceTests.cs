using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using WRRManagement.Application.Amenities;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Tests;

public class AmenityQueryServiceTests
{
    private readonly IExtraAmenityRepository _amenityRepo = Substitute.For<IExtraAmenityRepository>();
    private readonly AmenityQueryService _sut;

    public AmenityQueryServiceTests()
    {
        _sut = new AmenityQueryService(_amenityRepo, NullLogger<AmenityQueryService>.Instance);
    }

    [Fact]
    public async Task HotelHasAmenitiesAsync_ReturnsFalse_WhenRepoReturnsEmpty()
    {
        _amenityRepo.GetAllForHotelAsync(1).Returns([]);

        var result = await _sut.HotelHasAmenitiesAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HotelHasAmenitiesAsync_ReturnsFalse_WhenAllAmenitiesAreNotVisible()
    {
        _amenityRepo.GetAllForHotelAsync(1).Returns(
        [
            new ExtraAmenity { AmenityID = 1, HotelID = 1, Name = "Spa", Visible = false }
        ]);

        var result = await _sut.HotelHasAmenitiesAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HotelHasAmenitiesAsync_ReturnsTrue_WhenHasVisibleAmenity()
    {
        _amenityRepo.GetAllForHotelAsync(1).Returns(
        [
            new ExtraAmenity { AmenityID = 1, HotelID = 1, Name = "Breakfast", Visible = true }
        ]);

        var result = await _sut.HotelHasAmenitiesAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAmenitiesForHotelAsync_FiltersOutInvisibleAmenities()
    {
        _amenityRepo.GetAllForHotelAsync(1).Returns(
        [
            new ExtraAmenity { AmenityID = 1, HotelID = 1, Name = "Breakfast", Visible = true },
            new ExtraAmenity { AmenityID = 2, HotelID = 1, Name = "Hidden Perk", Visible = false },
            new ExtraAmenity { AmenityID = 3, HotelID = 1, Name = "Spa", Visible = true }
        ]);

        var result = await _sut.GetAmenitiesForHotelAsync(1);

        result.Should().HaveCount(2);
        result.Should().NotContain(a => a.Name == "Hidden Perk");
    }

    [Fact]
    public async Task GetAmenitiesForHotelAsync_MapsDtoFieldsCorrectly()
    {
        _amenityRepo.GetAllForHotelAsync(1).Returns(
        [
            new ExtraAmenity
            {
                AmenityID = 7,
                HotelID = 1,
                Name = "Breakfast",
                ShortDescription = "Buffet",
                AmenityRate = 15.00m,
                Tax = 1.50m,
                Visible = true,
                Mandatory = false,
                PerDay = true
            }
        ]);

        var result = await _sut.GetAmenitiesForHotelAsync(1);

        var dto = result.Single();
        dto.AmenityId.Should().Be(7);
        dto.Name.Should().Be("Breakfast");
        dto.AmenityRate.Should().Be(15.00m);
        dto.Tax.Should().Be(1.50m);
        dto.PerDay.Should().BeTrue();
    }
}
