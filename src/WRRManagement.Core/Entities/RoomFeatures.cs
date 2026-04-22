using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class RoomFeatures
    {
        public int ID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public string? Icon { get; internal set; }
        public string Features { get; internal set; }

        public RoomFeatures() { }

        public static RoomFeatures Create(
            int roomTypeID,
            string? icon,
            string features) 
        {
            if (roomTypeID < 0)
                throw new ArgumentException("Room Type must be a valid room", nameof(roomTypeID));

            if (icon != null)
                ValidateIcon(icon);

            if (string.IsNullOrWhiteSpace(features))
                throw new ArgumentException("Room Features is required", nameof(features));

            return new RoomFeatures
            {
                RoomTypeID = roomTypeID,
                Icon = icon,
                Features = features.Trim()
            }; 
        }

        public void UpdateRoomFeature(
              string? icon,
            string features)
        {
            if (icon != null)
                ValidateIcon(icon);

            if (string.IsNullOrWhiteSpace(features))
                throw new ArgumentException("Room Features is required", nameof(features));

            Icon = icon;
            Features = features;
        }

        public static void ValidateIcon(string iconHtml)
        {
            // Basic validation: must match Font Awesome pattern
            if (!Regex.IsMatch(iconHtml, @"^<i class=""fa-[\w\s-]+""></i>$"))
                throw new ArgumentException("Invalid icon format", nameof(iconHtml));            
        }
    }
}
