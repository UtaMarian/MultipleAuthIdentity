namespace MultipleAuthIdentity.Data
{
    public class Routes
    {
        public int Id { get; set; }
        //check
        public int BusId { get; set; }
        public string? Departure { get; set; }
        public string? Arrival { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public float? Price { get; set; }
        public string? UserId { get; set; }
    }

}
