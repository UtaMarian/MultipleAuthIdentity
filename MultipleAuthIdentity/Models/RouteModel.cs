using MultipleAuthIdentity.Data;

namespace MultipleAuthIdentity.Models
{
    public class RouteModel
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public string? Departure { get; set; }
        public string? Arrival { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public float? Price { get; set; }
        public string? UserId { get; set; }

        public RouteModel() { }
        public RouteModel(Routes route)
        {
            this.Id= route.Id;
            this.BusId= route.BusId;
            this.Arrival = route.Arrival;
            this.ArrivalDate = route.ArrivalDate;
            this.Price = route.Price;
            this.UserId = route.UserId;
            this.Departure = route.Departure;
            this.DepartureDate = route.DepartureDate;
        }
    }
}
