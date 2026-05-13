using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class RoomImage
    {
        public int ImageID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public string BlobUrl { get; internal set; } = string.Empty;
        public string? Description { get; internal set; }
        public string FileName { get; internal set; } = string.Empty;
        public string ContentType { get; internal set; } = string.Empty;
        public long ContentLength { get; internal set; }
        public int SortOrder { get; internal set; }
        public bool IsVisible { get; internal set; }
        public bool MainImage { get; internal set; }

        public RoomImage() { }
        public static RoomImage Create(
        int roomTypeID,
        string blobUrl,
        string fileName,
        string contentType,
        long contentLength,
        int sortOrder,
        string? description = null)
        {
            if (roomTypeID <= 0)
                throw new ArgumentException("Room Type must be a valid room", nameof(roomTypeID));

            if (string.IsNullOrWhiteSpace(blobUrl))
                throw new ArgumentException("Blob Url is required", nameof(blobUrl));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File Name is required", nameof(fileName));

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Conent type is required", nameof(contentType));

            return new RoomImage
            {
                RoomTypeID = roomTypeID,
                BlobUrl = blobUrl,
                FileName = fileName.Trim(),
                ContentType = contentType,
                ContentLength = contentLength,
                SortOrder = sortOrder,
                Description = description?.Trim(),
                IsVisible = true
            };
        }

        public void UpdateDetails(string? description, int sortOrder)
        {
            Description = description?.Trim();
            SortOrder = sortOrder;
        }
        public void SetVisibility(bool isVisible) => IsVisible = isVisible;
    }
}
