using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace musiq.Models
{
    public class User
    {
        public User()
        {
            Likes = new HashSet<Like>();
            Posts = new HashSet<Post>();
        }

        public int UserId { get; set; }

        [Required]
        public string Email { get; set; }

        public string Password { get; set; }

        [NotMapped]
        public string ConfirmPassword { get; set; }

        public string PassSalt { get; set; }

        public bool? IsAdmin { get; set; }

        public string Nickname { get; set; }

        public string Description { get; set; }

        public string LikedGenres { get; set; }

        public string ProfilePicture { get; set; }

        public ICollection<Like> Likes { get; set; }

        public ICollection<Post> Posts { get; set; }


        [NotMapped]
        public IFormFile File { get; set; }
    }
}
