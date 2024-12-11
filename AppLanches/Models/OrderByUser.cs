using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLanches.Models
{
    public class OrderByUser
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
