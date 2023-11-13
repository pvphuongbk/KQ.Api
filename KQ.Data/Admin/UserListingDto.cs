using KQ.DataDto.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataDto.Admin
{
    public class UserListingDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public DateTime ExpireDate { get; set; }
        public UserStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
