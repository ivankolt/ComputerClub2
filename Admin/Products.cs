using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerClub.Admin
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int QuantityStore { get; set; }
        public string Category { get; set; }
        public string Picture { get; set; }

        public string FullImagePath => $"/Image/{Picture}";
    }
}
