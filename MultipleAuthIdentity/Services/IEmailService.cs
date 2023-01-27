using MultipleAuthIdentity.Models;

namespace MultipleAuthIdentity.Services
{
 
    

    public interface IEmailService
    {
        void SendEmail(Message message);
    }


}
