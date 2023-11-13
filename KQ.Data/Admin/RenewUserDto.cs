using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataDto.Admin
{
    public class RenewUserDto
    {
        public int UserId { get; set; }
        public DateTime NewExpireDate { get; set; }
    }
}
