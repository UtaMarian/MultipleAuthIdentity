﻿namespace MultipleAuthIdentity.Models
{
    public class AdminPanelModel
    {
        public int onlineUsers { get; set; } = 0;
        public int totalUsers { get; set; } = 0;
        public float totalMoney { get; set; } = 0;
        public int soldedTickets { get; set; } = 0;
        public List<string> montlyOnlineUsers { get; set; }= new List<string>(0);
        public List<int> providers { get; set; }= new List<int>();
        public List<int> dailyUsers { get; set; } = new List<int>();
    }
}
