using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace musiq.Models
{
    public class Post
    {
        public Post()
        {
            Comments = new HashSet<Comment>();
        }
        public int PostId { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Genre { get; set; }

        public DateTime Created { get; set; }

        public string Description { get; set; }

        public string Lyrics { get; set; }

        public string YoutubeLink { get; set; }

        public string Media { get; set; }

        public User User { get; set; }

        public IEnumerable<Comment> Comments { get; set; }

        [NotMapped]
        public IFormFile File { get; set; }
    }
}
