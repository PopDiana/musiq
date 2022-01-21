using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace musiq.Models
{
    public class Like
    {
        public int LikeId { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }
    }
}
