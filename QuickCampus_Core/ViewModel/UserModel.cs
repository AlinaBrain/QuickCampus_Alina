﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCampus_Core.ViewModel
{
    public class UserModel
    {
        [Required, MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z\s]+$", ErrorMessage = "In name only characters allowed.")]
        public string? Name { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Password Must be between 6 and 50 characters", MinimumLength = 8)]
        [RegularExpression(@"(?!.* )(?=.*\d)(?=.*[A-Z]).{8,15}$",
        ErrorMessage = "Please enter a password that contains 1 small alphabet, 1 capital alphabet, 1 special character.")]
        public string? Password { get; set; }
        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? ConfirmPassword { get; set; }
        public string? Email { get; set; }
        [Required, MaxLength(10)]
        [Phone(ErrorMessage ="Please enter a valid mobile number")]
        public string? Mobile { get; set; }
    }
}
