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
        public string Name { get; internal set; } = string.Empty;
        public string Email { get; internal set; } = string.Empty;
        public string AdminEmail { get; internal set; } = string.Empty;
        public string Address1 { get; internal set; } = string.Empty;
        public string Address2 { get; internal set; } = string.Empty;
        public string City { get; internal set; } = string.Empty;
        public string State { get; internal set; } = string.Empty;
        public string ZipCode { get; internal set; } = string.Empty;
        public string LocalPhone { get; internal set; } = string.Empty;
        public string? TollFreePhone { get; internal set; }
        public string Website { get; internal set; } = string.Empty;
        public string Description { get; internal set; } = string.Empty;
        public string CheckIn { get; internal set; } = string.Empty;
        public string CheckOut { get; internal set; } = string.Empty;

        internal Hotel() { }

        public static Hotel Create(
            string name,
            string email,
            string adminEmail,
            string address1,
            string city,
            string state,
            string zipCode,
            string localPhone,
            string? tollFreePhone,
            string checkIn,
            string checkOut)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Hotel name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email address is required", nameof(email));

            if (string.IsNullOrWhiteSpace(address1))
                throw new ArgumentException("Address is required", nameof(address1));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State is required", nameof(state));

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code is required", nameof(zipCode));

            if (string.IsNullOrWhiteSpace(localPhone))
                throw new ArgumentException("Local Phone number is required", nameof(localPhone));

            if (string.IsNullOrEmpty(checkIn))
                throw new ArgumentException("Check In Time is required", nameof(checkIn));

            if (string.IsNullOrEmpty(checkOut))
                throw new ArgumentException("Check out time is required", nameof(checkOut));

            return new Hotel
            {
                Name = name,
                Email = email,
                AdminEmail = adminEmail,
                Address1 = address1,
                City = city,
                State = state,
                ZipCode = zipCode,
                LocalPhone = localPhone,
                TollFreePhone = tollFreePhone,
                CheckIn = checkIn,
                CheckOut = checkOut
            };
        }

        public void UpdateHotel(
            string name,
            string email,
            string adminEmail,
            string address1,
            string address2,
            string city,
            string state,
            string zipCode,
            string localPhone,
            string? tollFreePhone,
            string website,
            string description,
            string checkIn,
            string checkOut)
        {
            if (string.IsNullOrWhiteSpace(address1))
                throw new ArgumentException("Address is required", nameof(address1));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State is required", nameof(state));

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code is required", nameof(zipCode));

            if (string.IsNullOrWhiteSpace(localPhone))
                throw new ArgumentException("Local Phone number is required", nameof(localPhone));

            if (string.IsNullOrEmpty(checkIn))
                throw new ArgumentException("Check In Time is required", nameof(checkIn));

            if (string.IsNullOrEmpty(checkOut))
                throw new ArgumentException("Check out time is required", nameof(checkOut));

            Name = name;
            Email = email;
            AdminEmail = adminEmail;
            Address1 = address1;
            Address2 = address2;
            City = city;
            State = state;
            ZipCode = zipCode;
            LocalPhone = localPhone;
            TollFreePhone = tollFreePhone;
            Website = website;
            Description = description;
            CheckIn = checkIn;
            CheckOut = checkOut;
        }

    }
}
