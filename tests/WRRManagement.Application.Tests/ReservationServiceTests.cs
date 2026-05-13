using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using WRRManagement.Application.Reservations;
using WRRManagement.Application.Reservations.Dtos;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Tests;

public class ReservationServiceTests
{
    private readonly IReservationRepository _reservationRepo = Substitute.For<IReservationRepository>();
    private readonly IReservationAmenityRepository _amenityRepo = Substitute.For<IReservationAmenityRepository>();
    private readonly ReservationService _sut;

    public ReservationServiceTests()
    {
        _sut = new ReservationService(_reservationRepo, _amenityRepo, NullLogger<ReservationService>.Instance);
    }

    private static CreateReservationDto MakeDto(
        IReadOnlyList<CreateReservationAmenityDto>? amenities = null,
        IReadOnlyList<(DateTime Date, decimal Rate)>? dailyRates = null) =>
        new()
        {
            HotelId = 1,
            RoomTypeId = 10,
            PaymentTypeId = 1,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 3),
            TotalNights = 2,
            Adults = 2,
            Children = 0,
            AvgDailyRate = 200m,
            SubTotal = 400m,
            TierLevel = 'A',
            TotalCharge = 450m,
            Deposit = 200m,
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
            Amenities = amenities ?? [],
            DailyRates = dailyRates ?? []
        };

    [Fact]
    public async Task CreateAsync_ReturnsReservationId_OnSuccess()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(42);

        var id = await _sut.CreateAsync(MakeDto());

        id.Should().Be(42);
    }

    [Fact]
    public async Task CreateAsync_SetsUserInitialsToWeb()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);

        await _sut.CreateAsync(MakeDto());

        await _reservationRepo.Received(1).CreateAsync(Arg.Is<Reservation>(r => r.UserInitials == "WEB"));
    }

    [Fact]
    public async Task CreateAsync_SetsBookedAmenityTrue_WhenAmenitiesProvided()
    {
        var amenity = new CreateReservationAmenityDto
        {
            AmenityId = 5, ChargeAmount = 15m, TaxIncluded = false,
            Mandatory = false, TaxRate = 0.08m, NumPeople = 1,
            NumDate = new DateTime(2026, 6, 1), TotalCharge = 15m
        };
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);
        _amenityRepo.AddAsync(Arg.Any<ReservationAmenity>()).Returns(1);

        await _sut.CreateAsync(MakeDto(amenities: [amenity]));

        await _reservationRepo.Received(1).CreateAsync(Arg.Is<Reservation>(r => r.BookedAmenity == true));
    }

    [Fact]
    public async Task CreateAsync_SetsBookedAmenityFalse_WhenNoAmenities()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);

        await _sut.CreateAsync(MakeDto(amenities: []));

        await _reservationRepo.Received(1).CreateAsync(Arg.Is<Reservation>(r => r.BookedAmenity == false));
    }

    [Fact]
    public async Task CreateAsync_AddsEachAmenity()
    {
        var amenities = new[]
        {
            new CreateReservationAmenityDto { AmenityId = 1, NumDate = new DateTime(2026, 6, 1) },
            new CreateReservationAmenityDto { AmenityId = 2, NumDate = new DateTime(2026, 6, 1) }
        };
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(10);
        _amenityRepo.AddAsync(Arg.Any<ReservationAmenity>()).Returns(1);

        await _sut.CreateAsync(MakeDto(amenities: amenities));

        await _amenityRepo.Received(2).AddAsync(Arg.Any<ReservationAmenity>());
    }

    [Fact]
    public async Task CreateAsync_AddsDailyRates_WhenProvided()
    {
        var rates = new (DateTime, decimal)[]
        {
            (new DateTime(2026, 6, 1), 200m),
            (new DateTime(2026, 6, 2), 200m)
        };
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(5);

        await _sut.CreateAsync(MakeDto(dailyRates: rates));

        await _reservationRepo.Received(1).AddDailyRatesAsync(5, Arg.Any<IEnumerable<(DateTime, decimal)>>());
    }

    [Fact]
    public async Task CreateAsync_DoesNotAddDailyRates_WhenNoneProvided()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Returns(1);

        await _sut.CreateAsync(MakeDto(dailyRates: []));

        await _reservationRepo.DidNotReceive().AddDailyRatesAsync(Arg.Any<int>(), Arg.Any<IEnumerable<(DateTime, decimal)>>());
    }

    [Fact]
    public async Task CreateAsync_PropagatesException_WhenRepoThrows()
    {
        _reservationRepo.CreateAsync(Arg.Any<Reservation>()).Throws(new InvalidOperationException("DB error"));

        var act = () => _sut.CreateAsync(MakeDto());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
    }
}
