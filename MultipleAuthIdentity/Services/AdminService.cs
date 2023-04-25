using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using Org.BouncyCastle.Asn1.Mozilla;
using Sustainsys.Saml2.Metadata;

namespace MultipleAuthIdentity.Services
{
    public class AdminService :IAdminService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly string usersFilename;
        private readonly string moneyRoutesFilename;

        public AdminService(IConfiguration configuration, UserManager<AppUser> userManager, AuthDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
            usersFilename = "";
            moneyRoutesFilename = "";
        }

        public void changeUsersPanel(int month)
        {
            var onlineUsers = 0;

            int year = 2023;

           //List<AppUser> users = _userManager.Users.Where(u => u.LastSignIn);
            saveInFile(month-1, onlineUsers);
        }

        public void changeMoneyPanel()
        {

        }

        public List<string> getMonthlyUsers()
        {
            if (!File.Exists("monthlyOnlineUsers.txt"))
            {
                List<int> numbers = new List<int> { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
                using (FileStream fs = File.Create("monthlyOnlineUsers.txt"))
                {
                    // do nothing
                }
                using (StreamWriter writer = File.AppendText("monthlyOnlineUsers.txt"))
                {
                    foreach (int number in numbers)
                    {
                        writer.WriteLine(number);
                    }
                    writer.Close();
                }
            }

            List<string> result = new List<string>();
            string[] lines = File.ReadAllLines("monthlyOnlineUsers.txt");
            foreach (var line in lines)
            {
               
                    result.Add(line);
                
            }

            return result;

        }

        public void saveInFile(int monthIndex,int value)
        {
            if (!File.Exists("monthlyOnlineUsers.txt"))
            {
                List<int> numbers = new List<int> { 1, 1, 1, 1, 0,0,0,0,0,0,0,0 };
                // Create the file
                File.Create("monthlyOnlineUsers.txt");
                using (StreamWriter writer = File.AppendText("monthlyOnlineUsers.txt"))
                {
                    foreach (int number in numbers)
                    {
                        writer.WriteLine(number);
                    }
                }

            }
            
            string[] lines = File.ReadAllLines("monthlyOnlineUsers.txt");

            if (monthIndex < 0 || monthIndex >= lines.Length)
            {
                throw new ArgumentException("Index is out of range.");
            }

            lines[monthIndex] = value.ToString();

            File.WriteAllLines("monthlyOnlineUsers.txt", lines);

        }
    }
}
