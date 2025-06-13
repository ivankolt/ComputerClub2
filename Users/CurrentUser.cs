using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerClub.Users
{
    public class CurrentUser
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public static CurrentUser Instance { get; set; } = new CurrentUser(); // Синглтон

        public int EmployeeId { get; set; }
    }
}
