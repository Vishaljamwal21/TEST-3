﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SMS_APP.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }
        [NotMapped]
        public string Token { get; set; }
    }
}
