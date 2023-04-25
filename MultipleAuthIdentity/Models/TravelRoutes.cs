using MultipleAuthIdentity.Data;

namespace MultipleAuthIdentity.Models
{

    public class TravelRoutes
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public string? Departure { get; set; }
        public string? Arrival { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public float? Price { get; set; }
        public string? Bus_Plate_number { get; set; }
        public string? Bus_Type { get; set; }
        public int Capacity { get; set; }
        public TravelRoutes(Routes r,string plate_nr,string busType,int capacity) 
        {
            Id = r.Id;
            BusId = r.BusId;
            Departure = r.Departure;
            Arrival = r.Arrival;
            DepartureDate= r.DepartureDate;
            ArrivalDate = r.ArrivalDate;
            Price = r.Price;
            Capacity = capacity;
            Bus_Plate_number = plate_nr;
            Bus_Type = busType;

        }
    }
}
