using KQ.DataDto.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataDto.Admin
{
    public class AddUserDto
    {
        public string Name { get; set; }
        public string Account { get; set; }
        public DateTime ExpireDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public string? Note { get; set; }
    }
}
