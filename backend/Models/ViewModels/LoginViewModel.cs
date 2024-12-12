using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.ViewModels;

public class LoginViewModel
{

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
