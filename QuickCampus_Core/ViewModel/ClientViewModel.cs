﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCampus_Core.ViewModel
{
    public class ClientViewModel
    {
        [Required(ErrorMessage = "Name is required"), MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z\s]+$", ErrorMessage = "Only characters allowed.")]
        public string? Name { get; set; }
        public string? Address { get; set; }
        [Required]
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Please enter a valid 10-digit mobile number that does not start with 0.")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "You must provide an email address.")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Only Email allowed.")]
        [MaxLength(100, ErrorMessage = "can't exceed more than 100 characters.")]
        [EmailAddress(ErrorMessage = "Not a valid email address.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Subscription plan is required"), MaxLength(20)]
        public string? SubscriptionPlan { get; set; }
        [Required(ErrorMessage = "Longitude is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$")]
        public decimal? Longitude { get; set; }
        [Required(ErrorMessage ="Client Role is required.")]
        public int? RoleId { get; set; }
        [Required(ErrorMessage = "Longitude is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$")]
        public decimal? Latitude { get; set; }
        [Required(ErrorMessage = "You must provide an UserName.")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Only Username allowed.")]
        [MaxLength(100, ErrorMessage = "can't exceed more than 100 characters.")]
        [EmailAddress(ErrorMessage = "Not a valid Username address.")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Name is required"), MaxLength(20)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Only Password allowed.")]
        public string? Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
