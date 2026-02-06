using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace testproject.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [PersonalData]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [PersonalData]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [PersonalData]
        public DateTime BirthDate { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }

        [PersonalData]
        public string? AvatarUrl { get; set; }

        [PersonalData]
        public string? Bio { get; set; }

        [PersonalData]
        public string? Location { get; set; }
    }
}