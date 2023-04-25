namespace MultipleAuthIdentity.Data
{
    public class Review
    {
        public int Id { get; set; }
        public string Email;
        public string Subject;
        public string Content;
        public DateTime Date;
        //public Review() { 
        //    Email= string.Empty;
        //    Content= string.Empty;
        //    Subject= string.Empty;
        //   // Date = DateTime.Now;
        //}
        //public Review(string email,string content,string subject) { 
        //    Email= email;
        //    Content= content;
        //    Subject= subject;
        //   // Date = DateTime.Now;
        //}
        //public Review(string email, string content, string subject,DateTime date)
        //{
        //    Email = email;
        //    Content = content;
        //    Subject = subject;
        //    Date = date;
        //}
    }
}
