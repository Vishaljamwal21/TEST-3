﻿using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystum.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string Duration { get; set; }
    }
}
