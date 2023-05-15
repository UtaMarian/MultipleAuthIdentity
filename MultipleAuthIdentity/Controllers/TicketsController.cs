using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using NReco.PdfGenerator;
using Org.BouncyCastle.Ocsp;
using System.Dynamic;

namespace MultipleAuthIdentity.Controllers
{
    
    public class TicketsController : Controller
    {


        private readonly AuthDbContext _context;
        private readonly UserManager<AppUser> userManager;
        public TicketsController(AuthDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        
        [HttpPost("curse")]
        public List<TravelRoutes> getCurse(CurseDto req)
        {


            DateTime dateTime;
            DateTime.TryParse(req.Date, out dateTime);


            var curse = _context.Routes.ToList();
            List<Routes> rute = new();
            List<TravelRoutes> travel = new();
            foreach (var c in curse)
            {
                if (req.Arrival.Equals(c.Arrival) && req.Departure.Equals(c.Departure))
                {
                    var bus = _context.Bus.Find(c.BusId);
                    if (bus != null)
                    {
                        travel.Add(new TravelRoutes(c, bus.Bus_Plate_number, bus.Bus_Type, bus.Capacity));
                    }
                }
            }
            return travel;
        }

        [HttpPost("reservations")]
        public List<int> getLocuriPerCursa(SeatsDto dto)
        {
            DateTime dateTime = DateTime.Parse(dto.DepartureDay);

           
            var rs = from m in _context.Reservations where m.RouteId.ToString() == dto.Id & m.DateSchedule.Day == dateTime.Day select m;
            List<int> locuriIndisponibile=new List<int>();

            foreach(var r in rs)
            {
                locuriIndisponibile.Add(r.SeatNumber);
            }
            return locuriIndisponibile;

        }
        [HttpPost("mytickets")]
        public List<Reservation> GetReservations(TicketsDto req)
        {


            var tickets = _context.Reservations.ToList();
            List<Reservation> res= new();

            foreach (var c in tickets)
            {
                if (req.Id== c.UserId)
                {
                    res.Add(c);
                }
            }
            return res;
        }


        [HttpPost]
        public IActionResult Cautare_Curse()
        {
            string departure = Request.Form["departure"];
            string arrival = Request.Form["arrival"];
            string dateString = Request.Form["date"];
            DateTime date;
            DateTime.TryParse(dateString, out date);
            

            var curse = _context.Routes.ToList();
            List<Routes> rute = new();
            List<TravelRoutes> travel = new();
            foreach (var c in curse) 
            { 
                if(arrival.Equals(c.Arrival) && departure.Equals(c.Departure))
                {
                    var bus = _context.Bus.Find(c.BusId);
                    if(bus != null)
                    {
                        travel.Add(new TravelRoutes(c, bus.Bus_Plate_number, bus.Bus_Type, bus.Capacity));
                    }
                }
            }
            
            return View("Curse", travel);
            //return Redirect("Curse");
        }
        public IActionResult Curse()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Ticket()
        {
            AppUser? currentUser = _context.Users.FirstOrDefault(r=>r.Email== HttpContext.User.Identity.Name);
            List<Reservation> userReservations = _context.Reservations.Where(r => r.UserId == currentUser.Id).ToList();
            

            List<TicketModel> tickets = new List<TicketModel>();
            foreach(var reservation in userReservations)
            {
                Routes travelRoute = _context.Routes.Where(r => r.Id == reservation.RouteId).ToList().First();
                TicketModel ticket = new TicketModel();
                if(travelRoute!=null)
                {
                    ticket.From = travelRoute.Departure;
                    ticket.To = travelRoute.Arrival;
                    ticket.Date = (DateTime)reservation.DateSchedule;
                    ticket.Price = (float)travelRoute.Price;
                    ticket.SeatNumber = reservation.SeatNumber;
                    
                    tickets.Add(ticket);
                }
            }
            tickets=tickets.OrderByDescending(t => t.Date).ToList();
            return View(tickets);
        }

       
        public IActionResult Locuri(string id,string departure_day)
        {
            DateTime dateTime = DateTime.Parse(departure_day);

            TravelRoutes tr; 
            dynamic mymodel = new ExpandoObject();


            var route=_context.Routes.Find(int.Parse(id));
            if(route!= null)
            {
                var bus = _context.Bus.Find(route.BusId);
                if(bus != null )
                {
                   tr = new TravelRoutes(route, bus.Bus_Plate_number, bus.Bus_Type, bus.Capacity);
                   mymodel.TravelRoutes = tr;
                }
            }

            var rs = from m in _context.Reservations where m.RouteId.ToString()==id & m.DateSchedule.Day == dateTime.Day select m;
            mymodel.Reservations = rs;
            return View(mymodel);
        }

      

        [HttpPost]
        public async Task<IActionResult> ConfirmareRezervare([FromBody] ReservationModel data)
        {
            Routes route = _context.Routes.Find(data.routeId);
            Reservation reservation=new();
            DateTime scheduleDate = DateTime.Parse(data.Date);
            AppUser user= await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            reservation.BusId = route.BusId;
            reservation.RouteId = data.routeId;
            reservation.DateSchedule = scheduleDate;
            reservation.UserId = user.Id;
            for (var i=0;i<data.SeatsNumber.Count;i++)
            {
                reservation.SeatNumber = Int32.Parse(data.SeatsNumber[i]);
                reservation.TicketType = data.TicketsType[i];
                reservation.Price = (float)route.Price;
                _context.Reservations.Add(reservation);
                _context.SaveChanges();
            }
            return Ok("success");
        }

        public class CurseDto
        {
            public string Departure { get; set; } = string.Empty;
            public string Arrival { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;

        }

        public class TicketsDto
        {
            public string Id { get; set; } = string.Empty;
        }

        public class SeatsDto
        {
            public string Id { get; set; } = string.Empty;
            public string DepartureDay { get; set;}= string.Empty;
        }

    }
}
