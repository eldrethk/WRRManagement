using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using WRRManagement.Application.Hotels;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Tests;

public class HotelQueryServiceTests
{
    private readonly IHotelRepository _hotelRepo = Substitute.For<IHotelRepository>();
    private readonly IDisclaimerRepository _disclaimerRepo = Substitute.For<IDisclaimerRepository>();
    private readonly HotelQueryService _sut;

    public HotelQueryServiceTests()
    {
        _sut = new HotelQueryService(_hotelRepo, _disclaimerRepo, NullLogger<HotelQueryService>.Instance);
    }

    [Fact]
    public async Task GetHotelAsync_ReturnsNull_WhenHotelNotFound()
    {
        _hotelRepo.GetByIdAsync(1).Returns((Hotel?)null);

        var result = await _sut.GetHotelAsync(1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetHotelAsync_ReturnsMappedDto_WhenHotelFound()
    {
        var hotel = Hotel.Create(
            name: "Grand Hotel",
            email: "info@grand.com",
            address: "123 Main St",
            city: "Springfield",
            state: "IL",
            zipCode: "62701",
            localPhone: "555-1234",
            tollFreePhone: "800-555-0000",
            checkInTime: "3:00 PM",
            checkOutTime: "11:00 AM");
        _hotelRepo.GetByIdAsync(1).Returns(hotel);

        var result = await _sut.GetHotelAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Grand Hotel");
        result.Email.Should().Be("info@grand.com");
        result.City.Should().Be("Springfield");
        result.CheckInTime.Should().Be("3:00 PM");
        result.CheckOutTime.Should().Be("11:00 AM");
    }

    [Fact]
    public async Task GetDisclaimerAsync_ReturnsNull_WhenDisclaimerNotFound()
    {
        _disclaimerRepo.GetDisclaimerForHotel(1).Returns((Disclaimer?)null);

        var result = await _sut.GetDisclaimerAsync(1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDisclaimerAsync_ReturnsMappedDto_WhenDisclaimerFound()
    {
        var disclaimer = Disclaimer.Create(1, "Please read before booking.", "You may unsubscribe at any time.");
        _disclaimerRepo.GetDisclaimerForHotel(1).Returns(disclaimer);

        var result = await _sut.GetDisclaimerAsync(1);

        result.Should().NotBeNull();
        result!.ReservationDisclaimer.Should().Be("Please read before booking.");
        result.EmailDisclaimer.Should().Be("You may unsubscribe at any time.");
    }
}
