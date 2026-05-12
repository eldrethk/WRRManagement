namespace WRRManagement.Application.Rooms.Dtos
{
    public class ViewRoomDto
    {
        public int RoomTypeId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? BedType { get; init; }
        public string? MainImageUrl { get; init; }
        public IReadOnlyList<RoomImageDto> Images { get; init; } = [];
        public IReadOnlyList<RoomFeatureDto> Features { get; init; } = [];
        public int MaxGuests { get; init; }
    }

    public class RoomImageDto
    {
        public int Id { get; init; }
        public string BlobUrl { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int SortOrder { get; init; }
    }

    public class RoomFeatureDto
    {
        public int Id { get; init; }
        public string? Icon { get; init; }
        public string Feature { get; init; } = string.Empty;
    }
}
