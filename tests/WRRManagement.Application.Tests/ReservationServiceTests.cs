using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using WRRManagement.Application.Pricing;
using WRRManagement.Application.Pricing.Dtos;
using WRRManagement.Application.Reservations;
using WRRManagement.Application.Reservations.Dtos;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Tests;

public class ReservationServiceTests
{
    private readonly IReservationRepository _reservationRepo = Substitute.For<IReservationRepository>();
    private readonly IReservationAmenityRepository _amenityRepo = Substitute.For<IReservationAmenityRepository>();
    private readonly IRoomAllocationRepository _roomAllocationRepo = Substitute.For<IRoomAllocationRepository>();
    private readonly IPackageAllocationRepository _packageAllocationRepo = Substitute.For<IPackageAllocationRepository>();
    private readonly IQuoteService _quoteService = Substitute.For<IQuoteService>();
    private readonly ReservationService _sut;

    public ReservationServiceTests()
    {
        _sut = new ReservationService(
            _reservationRepo, _amenityRepo, _roomAllocationRepo, _packageAllocationRepo, _quoteService,
            NullLogger<ReservationService>.Instance);

        _roomAllocationRepo.AllocationIsValidAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(true);
        _quoteService.GetQuoteAsync(Arg.Any<QuoteRequestDto>(), Arg.Any<CancellationToken>()).Returns(MakeQuote());
    }

    private static QuoteResponseDto MakeQuote(IReadOnlyList<AmenityQuoteLineDto>? amenities = null) => new()
    {
        RoomTypeId = 10,
        CheckIn = new DateTime(2026, 6, 1),
        CheckOut = new DateTime(2026, 6, 3),
        TotalNights = 2,
        TierLevel = 'A',
        RateDates = [new DateTime(2026, 6, 1), new DateTime(2026, 6, 2)],
        DailyRates = [200m, 200m],
        SubTotal = 400m,
        Tax = 32m,
        Total = 450m,
        Deposit = 200m,
        Amenities = amenities ?? []
    };

    private static CreateReservationDto MakeDto(IReadOnlyList<AmenitySelectionDto>? amenities = null) => new()
    {
        HotelId = 1,
        RoomTypeId = 10,
        PaymentTypeId = 1,
        ArrivalDate = new DateTime(2026, 6, 1),
        DepartureDate = new DateTime(2026, 6, 3),
        Adults = 2,
        Children = 0,
        CusFirstName = "Jane",
        CusLastName = "Smith",
        CusAddress1 = "1 Main St",
        CusCity = "Springfield",
        CusState = "IL",
        CusZip = "62701",
        CusDayPhone = "555-0000",
        CusEmail = "jane@example.com",
        CardHolderName = "Jane Smith",
        CardExpirationDate = "12/28",
        CardNumber = "4111111111111111",
        CardSecureCode = "123",
        Amenities = amenities ?? []
    };

    [Fact]
    public async Task CreateAsync_ReturnsReservationId_OnSuccess()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(42);

        var id = await _sut.CreateAsync(MakeDto(), null);

        id.Should().Be(42);
    }

    [Fact]
    public async Task CreateAsync_SetsUserInitialsToWeb()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);

        await _sut.CreateAsync(MakeDto(), null);

        await _reservationRepo.Received(1).CreateAsync(Arg.Is<Reservation>(r => r.UserInitials == "WEB"));
    }

    [Fact]
    public async Task CreateAsync_SetsBookedAmenityTrue_WhenQuoteHasAmenities()
    {
        var amenityLine = new AmenityQuoteLineDto { AmenityId = 5, ChargeAmount = 15m, TotalCharge = 15m };
        _quoteService.GetQuoteAsync(Arg.Any<QuoteRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(MakeQuote(amenities: [amenityLine]));
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);
        _amenityRepo.AddAsync(Arg.Any<ReservationAmenity>()).Returns(1);

        await _sut.CreateAsync(MakeDto(amenities: [new AmenitySelectionDto { AmenityId = 5, NumPeople = 1 }]), null);

        await _reservationRepo.Received(1).CreateAsync(Arg.Is<Reservation>(r => r.BookedAmenity == true));
    }

    [Fact]
    public async Task CreateAsync_SetsBookedAmenityFalse_WhenNoAmenities()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);

        await _sut.CreateAsync(MakeDto(), null);

        await _reservationRepo.Received(1).CreateAsync(Arg.Is<Reservation>(r => r.BookedAmenity == false));
    }

    [Fact]
    public async Task CreateAsync_AddsEachAmenityFromTheQuote()
    {
        var amenities = new[]
        {
            new AmenityQuoteLineDto { AmenityId = 1, ChargeAmount = 10m, TotalCharge = 10m },
            new AmenityQuoteLineDto { AmenityId = 2, ChargeAmount = 20m, TotalCharge = 20m }
        };
        _quoteService.GetQuoteAsync(Arg.Any<QuoteRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(MakeQuote(amenities: amenities));
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(10);
        _amenityRepo.AddAsync(Arg.Any<ReservationAmenity>()).Returns(1);

        await _sut.CreateAsync(MakeDto(), null);

        await _amenityRepo.Received(2).AddAsync(Arg.Any<ReservationAmenity>());
    }

    [Fact]
    public async Task CreateAsync_AddsDailyRatesFromTheQuote()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(5);

        await _sut.CreateAsync(MakeDto(), null);

        await _reservationRepo.Received(1).AddDailyRatesAsync(5, Arg.Any<IEnumerable<(DateTime, decimal)>>());
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenRoomNoLongerAvailable()
    {
        _roomAllocationRepo.AllocationIsValidAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(false);

        var act = () => _sut.CreateAsync(MakeDto(), null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_ReturnsExistingReservation_OnIdempotentReplay()
    {
        var key = Guid.NewGuid();
        _reservationRepo.GetIdByIdempotencyKeyAsync(key).Returns(99);

        var id = await _sut.CreateAsync(MakeDto(), key);

        id.Should().Be(99);
        await _reservationRepo.DidNotReceive().CreateAsync(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task CreateAsync_PropagatesException_WhenRepoThrows()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Throws(new InvalidOperationException("DB error"));

        var act = () => _sut.CreateAsync(MakeDto(), null);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
    }
}
