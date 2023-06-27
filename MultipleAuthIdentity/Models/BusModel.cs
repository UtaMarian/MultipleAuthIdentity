using MultipleAuthIdentity.Data;

namespace MultipleAuthIdentity.Models
{
    public class BusModel
    {
        public int Id { get; set; }
        public string Bus_number { get; set; }
        public string Bus_Plate_number { get; set; }
        public string Bus_Type { get; set; }
        public int Capacity { get; set; }
        public string UserId { get; set; }

        public BusModel() { }
        public BusModel(Bus bus)
        {
            this.Id = bus.Id;
            this.Bus_number = bus.Bus_number;
            this.Bus_Plate_number = bus.Bus_Plate_number;
            this.Bus_Type = bus.Bus_Type;
            this.Capacity = bus.Capacity;
            this.UserId = bus.UserId;
        }
    }
}
