namespace MultipleAuthIdentity.Models
{
    public class ReservationModel
    {
        public int routeId { get; set; }
        public List<string> TicketsType { get; set; }=new List<string>();
        public List<string> SeatsNumber { get; set; } = new List<string>();
        public string? Date { get; set; }
    }
}
