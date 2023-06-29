using MultipleAuthIdentity.Data;

namespace MultipleAuthIdentity.Models
{
    public class AccessControlList
    {
        List<AccessListItem> ali = new List<AccessListItem>();
        public IEnumerator<AccessListItem> GetEnumerator()
        {
            return ali.GetEnumerator();
        }
        public AccessControlList(List<AccessListItem> ali)
        {
            this.ali = ali;
        }
    }
}
