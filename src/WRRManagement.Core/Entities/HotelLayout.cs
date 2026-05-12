namespace WRRManagement.Core.Entities
{
    public class HotelLayout
    {
        public int ID { get; set; }
        public int HotelID { get; set; }
        public string? HeaderFileName { get; set; }
        public string? FooterFileName { get; set; }
        public string? HotelCSS { get; set; }
        public string? EmailHeaderImage { get; set; }
        public string? EmailHotelLogo { get; set; }
        public string? StatusMessage { get; set; }
        public bool HeaderHtml { get; set; }
        public bool FooterHtml { get; set; }
    }
}
