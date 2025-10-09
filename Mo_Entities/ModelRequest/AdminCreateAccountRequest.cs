using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelRequest
{
    public class AdminCreateAccountRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
