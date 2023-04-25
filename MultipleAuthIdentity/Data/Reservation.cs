namespace MultipleAuthIdentity.Data
{
    public class Reservation
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int BusId { get; set; }
        public string UserId { get; set; }
        public DateTime DateSchedule { get; set; }
        public string TicketType { get; set; }
        public float Price { get; set; }
        public int SeatNumber { get; set; }

    }
}
