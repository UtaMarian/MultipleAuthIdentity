namespace MultipleAuthIdentity.Models
{
    public class TicketModel
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int SeatNumber { get; set; }
        public float Price { get; set; }
    }
}
