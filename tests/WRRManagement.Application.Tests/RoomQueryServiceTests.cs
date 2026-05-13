using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using WRRManagement.Application.Rooms;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Tests;

public class RoomQueryServiceTests
{
    private readonly IRoomTypeRepository _roomTypeRepo = Substitute.For<IRoomTypeRepository>();
    private readonly IRoomImageRepository _roomImageRepo = Substitute.For<IRoomImageRepository>();
    private readonly IRoomFeaturesRepository _roomFeaturesRepo = Substitute.For<IRoomFeaturesRepository>();
    private readonly IAdultBaseRepository _adultBaseRepo = Substitute.For<IAdultBaseRepository>();
    private readonly IMaxBaseRepository _maxBaseRepo = Substitute.For<IMaxBaseRepository>();
    private readonly IRoomAllocation _roomAllocationRepo = Substitute.For<IRoomAllocation>();
    private readonly IRackRateRepository _rackRateRepo = Substitute.For<IRackRateRepository>();
    private readonly ITierLevelRepository _tierLevelRepo = Substitute.For<ITierLevelRepository>();
    private readonly IMinStayRepository _minStayRepo = Substitute.For<IMinStayRepository>();
    private readonly IHotelSystemRepository _hotelSystemRepo = Substitute.For<IHotelSystemRepository>();
    private readonly RoomQueryService _sut;

    public RoomQueryServiceTests()
    {
        _sut = new RoomQueryService(
            _roomTypeRepo, _roomImageRepo, _roomFeaturesRepo,
            _adultBaseRepo, _maxBaseRepo, _roomAllocationRepo,
            _rackRateRepo, _tierLevelRepo, _minStayRepo,
            _hotelSystemRepo, NullLogger<RoomQueryService>.Instance);
    }

    private static void SetupEmptyImagesAndFeatures(
        IRoomImageRepository imageRepo, IRoomFeaturesRepository featuresRepo)
    {
        imageRepo.GetAllImageForRoomAsync(Arg.Any<int>()).Returns([]);
        imageRepo.GetMainForRoomAsync(Arg.Any<int>()).Returns((RoomImage?)null);
        featuresRepo.GetRoomFeaturesAsync(Arg.Any<int>()).Returns([]);
    }

    [Fact]
    public async Task GetBookableRoomsAsync_UsesAdultBaseMaxRoomTotal_WhenRoomHasAdultBase()
    {
        var room = RoomType.Create(1, "Suite", "A nice suite", adultBase: true, maxBase: false, bedType: "King");
        _roomTypeRepo.GetAllForHotelAsync(1).Returns([room]);
        SetupEmptyImagesAndFeatures(_roomImageRepo, _roomFeaturesRepo);

        // AdultBase: maxAdult=4, maxChild=2, maxRoomTotal=4 (4 <= 4+2)
        var ab = AdultBase.Create(roomTypeID: 1, adultBaseCount: 2, maxAdult: 4, childBaseCount: 1, maxChild: 2, maxRoomTotal: 4);
        _adultBaseRepo.GetByRoomIDAsync(Arg.Any<int>()).Returns(ab);

        var result = await _sut.GetBookableRoomsAsync(1);

        result.Should().HaveCount(1);
        result[0].MaxGuests.Should().Be(4);
    }

    [Fact]
    public async Task GetBookableRoomsAsync_UsesMaxBaseCount_WhenRoomHasMaxBase()
    {
        var room = RoomType.Create(1, "Deluxe", "Deluxe room", adultBase: false, maxBase: true, bedType: "Queen");
        _roomTypeRepo.GetAllForHotelAsync(1).Returns([room]);
        SetupEmptyImagesAndFeatures(_roomImageRepo, _roomFeaturesRepo);

        var mb = MaxBase.Create(roomTyeID: 1, maxBaseCount: 3, baseCount: 2);
        _maxBaseRepo.GetByRoomID(Arg.Any<int>()).Returns(mb);

        var result = await _sut.GetBookableRoomsAsync(1);

        result.Should().HaveCount(1);
        result[0].MaxGuests.Should().Be(3);
    }

    [Fact]
    public async Task GetBookableRoomsAsync_DefaultsMaxGuestsToTwo_WhenNeitherBaseSet()
    {
        var room = RoomType.Create(1, "Standard", "Standard room", adultBase: false, maxBase: false, bedType: null);
        _roomTypeRepo.GetAllForHotelAsync(1).Returns([room]);
        SetupEmptyImagesAndFeatures(_roomImageRepo, _roomFeaturesRepo);

        var result = await _sut.GetBookableRoomsAsync(1);

        result.Should().HaveCount(1);
        result[0].MaxGuests.Should().Be(2);
    }

    [Fact]
    public async Task GetBookableRoomsAsync_MapsImageFields_WhenImagesPresent()
    {
        var room = RoomType.Create(1, "Ocean View", "Ocean view room", adultBase: false, maxBase: false, bedType: "King");
        _roomTypeRepo.GetAllForHotelAsync(1).Returns([room]);

        var img = RoomImage.Create(1, "https://cdn.example.com/img1.jpg", "img1.jpg", "image/jpeg", 50000, 1, "Lobby");
        _roomImageRepo.GetAllImageForRoomAsync(Arg.Any<int>()).Returns([img]);
        _roomImageRepo.GetMainForRoomAsync(Arg.Any<int>()).Returns(img);
        _roomFeaturesRepo.GetRoomFeaturesAsync(Arg.Any<int>()).Returns([]);

        var result = await _sut.GetBookableRoomsAsync(1);

        result[0].MainImageUrl.Should().Be("https://cdn.example.com/img1.jpg");
        result[0].Images.Should().HaveCount(1);
        result[0].Images[0].BlobUrl.Should().Be("https://cdn.example.com/img1.jpg");
    }

    [Fact]
    public async Task GetBookableRoomsAsync_SetsNullMainImageUrl_WhenNoMainImage()
    {
        var room = RoomType.Create(1, "Standard", "Standard room", adultBase: false, maxBase: false, bedType: null);
        _roomTypeRepo.GetAllForHotelAsync(1).Returns([room]);
        SetupEmptyImagesAndFeatures(_roomImageRepo, _roomFeaturesRepo);

        var result = await _sut.GetBookableRoomsAsync(1);

        result[0].MainImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetBookableRoomsAsync_MapsFeatures()
    {
        var room = RoomType.Create(1, "Standard", "Standard room", adultBase: false, maxBase: false, bedType: null);
        _roomTypeRepo.GetAllForHotelAsync(1).Returns([room]);
        _roomImageRepo.GetAllImageForRoomAsync(Arg.Any<int>()).Returns([]);
        _roomImageRepo.GetMainForRoomAsync(Arg.Any<int>()).Returns((RoomImage?)null);

        var feature = RoomFeatures.Create(1, null, "Free WiFi");
        _roomFeaturesRepo.GetRoomFeaturesAsync(Arg.Any<int>()).Returns([feature]);

        var result = await _sut.GetBookableRoomsAsync(1);

        result[0].Features.Should().HaveCount(1);
        result[0].Features[0].Feature.Should().Be("Free WiFi");
    }

    [Fact]
    public async Task IsRoomAvailableAsync_ReturnsTrue_WhenAllocationIsValid()
    {
        _roomAllocationRepo.AllocationIsValidAsync(5, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(true);

        var result = await _sut.IsRoomAvailableAsync(5, DateTime.Today, DateTime.Today.AddDays(2));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_ReturnsFalse_WhenRepoThrows()
    {
        _roomAllocationRepo.AllocationIsValidAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Throws(new InvalidOperationException("Connection failed"));

        var result = await _sut.IsRoomAvailableAsync(5, DateTime.Today, DateTime.Today.AddDays(2));

        result.Should().BeFalse();
    }
}
