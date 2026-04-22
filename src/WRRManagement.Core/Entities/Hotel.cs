using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WRRManagement.Core.Entities
{
    public class Hotel
    {
        public int HotelID { get; internal set; }
        public string Name { get; internal set; }
        public string Email { get; internal set; }
        public string Address { get; internal set; }
        public string City { get; internal set; }
        public string State { get; internal set; }
        public string ZipCode { get; internal set; }
        public string LocalPhone { get; internal set; }
        public string? TollFreePhone { get; internal set; }
        public string CheckInTime { get; internal set; }
        public string CheckOutTime { get; internal set; }

        internal Hotel() { }

        public static Hotel Create(
            string name,
            string email,
            string address,
            string city,
            string state,
            string zipCode,
            string localPhone,
            string? tollFreePhone,
            string checkInTime,
            string checkOutTime)
        {
            // Validate each parameter
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Hotel name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email address is required", nameof(email));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is required", nameof(address));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State is required", nameof(state));

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code is required", nameof(zipCode));

            if(string.IsNullOrWhiteSpace(localPhone))
                throw new ArgumentException("Local Phone number is required", nameof(localPhone));

            if(string.IsNullOrEmpty(checkInTime))
                throw new ArgumentException("Check In Time is required", nameof(checkInTime));

            if (string.IsNullOrEmpty(checkOutTime)) 
                throw new ArgumentException("Check out time is required", nameof(checkOutTime));

            return new Hotel 
            {
                Name = name,
                Email = email,
                Address = address,
                City = city,
                State = state,
                ZipCode = zipCode,
                LocalPhone = localPhone,
                TollFreePhone = tollFreePhone,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime
            };

        }

        public void UpdateHotel(
            string address,
            string city,
            string state,
            string zipCode,
            string localPhone,
            string? tollFreePhone,
            string checkInTime,
            string checkOutTime)
        {
            // Validate each parameter
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is required", nameof(address));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State is required", nameof(state));

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code is required", nameof(zipCode));

            if (string.IsNullOrWhiteSpace(localPhone))
                throw new ArgumentException("Local Phone number is required", nameof(localPhone));

            if (string.IsNullOrEmpty(checkInTime))
                throw new ArgumentException("Check In Time is required", nameof(checkInTime));

            if (string.IsNullOrEmpty(checkOutTime))
                throw new ArgumentException("Check out time is required", nameof(checkOutTime));
            Address = address;
            City = city;
            State = state;
            ZipCode = zipCode;
            LocalPhone = localPhone;
            TollFreePhone = tollFreePhone;
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            
        }

    }
}
