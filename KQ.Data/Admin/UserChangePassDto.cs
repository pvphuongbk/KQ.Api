using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataDto.Admin
{
    public class UserChangePassDto
    {
        public int UserId { get; set; }
        public string NewPass { get; set; }
    }
}
