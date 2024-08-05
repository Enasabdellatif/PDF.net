namespace RestaurantBookingReport.Models
{
    public class RestaurantReport
    {
        public string RestaurantTitle { get; set; }
        public int TotalReservations { get; set; }
        public int TotalPax { get; set; }
        public List<SeatTimeData> SeatData { get; set; } = new List<SeatTimeData>();
    }
}
