using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaProject.Model
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupNumber { get; set; }

        public User? User { get; set; }
        public int UserId { get; set; }
    }
}
