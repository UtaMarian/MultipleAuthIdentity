using MultipleAuthIdentity.Data;
using System.Collections;

namespace MultipleAuthIdentity.Models
{
    public class Reviews
    {
        List<Review> reviews = new List<Review>();
        public IEnumerator<Review> GetEnumerator()
        {
            return reviews.GetEnumerator();
        }
        public Reviews(List<Review> reviews)
        {
            this.reviews = reviews;
        }
    }

}
