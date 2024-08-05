namespace RestaurantBookingReport.Models
{
    public class SeatTimeData
    {
        public string Time { get; set; }
        public int TotalReservations { get; set; }
        public int TotalPeople { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
