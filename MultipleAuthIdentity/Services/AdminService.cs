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

        public AdminService(IConfiguration configuration, UserManager<AppUser> userManager, AuthDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public void changeUsersPanel(int month)
        {
            var onlineUsers = _userManager.Users.Where(c => c.LastSignIn.Value.Month == month).Count();
            
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
            string filePath = "monthlyOnlineUsers.txt";
            if (!File.Exists(filePath))
            {
                List<int> numbers = new List<int> { 1, 1, 1, 1, 0,0,0,0,0,0,0,0 };
                // Create the file
          
                File.Create(filePath).Close();
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    foreach (int number in numbers)
                    {
                        writer.WriteLine(number);
                    }
                    writer.Close(); 
                }
               
            }
            
            string[] lines = File.ReadAllLines(filePath);

            if (monthIndex < 0 || monthIndex >= lines.Length)
            {
                throw new ArgumentException("Index is out of range.");
            }

            lines[monthIndex] = value.ToString();

            File.WriteAllLines(filePath, lines);

        }

        public List<string> GetDailyUserCount()
        {
            Dictionary<int,int> ret=new Dictionary<int, int>();
            List<string> dailyUsers=new List<string>();

            string filePath = "dailyUsers.txt";
            int currentDay = DateTime.Now.Day;

            if (!File.Exists(filePath))
            {
                List<int> numbers = new List<int> { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; ;
                File.Create(filePath).Close(); ;
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    foreach (int number in numbers)
                    {
                        writer.WriteLine(number);
                    }
                    writer.Close();
                }

            }

            string[] lines = File.ReadAllLines(filePath);


            var query = from u in _context.Users
                        where u.LastSignIn.Value.Month == DateTime.Now.Month && u.LastSignIn.Value.Day == currentDay
                        group u by u.LastSignIn.Value.Day into stats
                        select new
                        {
                            Day = stats.Key,
                            TotalCount = stats.Count()
                        };

            foreach (var res in query)
            {
                ret.Add(res.Day, res.TotalCount);
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (i + 1 == currentDay)
                {
                    lines[i] = ret.GetValueOrDefault(currentDay).ToString();
                }
            }
            File.WriteAllLines(filePath,lines);
            dailyUsers = File.ReadAllLines(filePath).ToList();

            

            return dailyUsers;
        }
       
       
    }
    public class UserCountPerDay
    {
        public int SignInDay{ get; set; }
        public int UserCount { get; set; }
    }
}
