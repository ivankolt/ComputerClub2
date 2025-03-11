using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerClub
{
    public class PC
    {
        public int Id { get; set; }
        public string Zone { get; set; }
        public decimal PricePerHour { get; set; }
        public string VideoCard { get; set; }
        public string CPU { get; set; }
        public string Monitor { get; set; }
        public int MonitorHertz { get; set; }
        public string Keyboard { get; set; }
        public bool Activity { get; set; }

        public PC(int id, string zone, decimal pricePerHour, string videoCard, string cpu, string monitor, string keyboard, int monitorHertz, bool activity)
        {
            Id = id;
            Zone = zone;
            PricePerHour = pricePerHour;
            VideoCard = videoCard;
            CPU = cpu;
            Monitor = monitor;
            Keyboard = keyboard;
            MonitorHertz = monitorHertz;
            Activity = activity;
        }
    }
}
