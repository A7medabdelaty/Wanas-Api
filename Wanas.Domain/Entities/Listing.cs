using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanas.Domain.Entities
{
    public class Listing
    {
        public int Id { get; set; }
        public HashSet<Room> Rooms { get; set; } = new();
    }
}
