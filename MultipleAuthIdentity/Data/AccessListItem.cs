namespace MultipleAuthIdentity.Data
{
    public class AccessListItem
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool HasAccess { get; set; }
        public string Reason { get; set; }
    }
}
