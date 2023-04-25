using MultipleAuthIdentity.Data;

namespace MultipleAuthIdentity.Models
{
    public class Reservations
    {
        List<Reservation>? rezervari = new List<Reservation>();

        public IEnumerator<Reservation> GetEnumerator()
        {
            return rezervari.GetEnumerator();
        }
        public Reservations(List<Reservation> rezervari)
        {
            this.rezervari = rezervari;
        }
    }
}
