using System.ComponentModel.DataAnnotations;
using BookWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.ViewModels;

public class UserVM
{
    [Required]
    public string Role { get; set; }
    public IEnumerable<SelectListItem> Rolelist { get; set; }
    public User User { get; set; }
}