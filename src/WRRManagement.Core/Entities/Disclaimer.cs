using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class Disclaimer
    {
        public int DisclaimerID { get; internal set; }
        public int HotelID {  get; internal set; }
        public string ReservationDisclaimer { get; internal set; }
        public string EmailDisclaimer { get; internal set; }

        public Disclaimer() { }

        public static Disclaimer Create(
            int hotelID,
            string reservationDisclaimer,
            string emailDisclaimer) 
        {
            if (hotelID < 0)
                throw new ArgumentException("Valid HotelID is required", nameof(hotelID));

            if (string.IsNullOrWhiteSpace(reservationDisclaimer)) 
                throw new ArgumentException("Reservation disclaimer is required", nameof(reservationDisclaimer));

            if(string.IsNullOrWhiteSpace(emailDisclaimer))
                throw new ArgumentException("Email discliamer is required", nameof(emailDisclaimer));

            return new Disclaimer
            {
                HotelID = hotelID,
                ReservationDisclaimer = reservationDisclaimer,
                EmailDisclaimer = emailDisclaimer
            };             
        }
        public void UpdateReservationDisclaimer(string reservationDisclaimer)
        {
            if (string.IsNullOrWhiteSpace(reservationDisclaimer))
                throw new ArgumentException("Reservation disclaimer is required", nameof(reservationDisclaimer));

            ReservationDisclaimer = reservationDisclaimer.Trim();
        }

        public void UpdateEmailDisclaimer(string emailDisclaimer)
        {
            if (string.IsNullOrWhiteSpace(emailDisclaimer))
                throw new ArgumentException("Email discliamer is required", nameof(emailDisclaimer));
            EmailDisclaimer = emailDisclaimer.Trim();
        }
    }
}
