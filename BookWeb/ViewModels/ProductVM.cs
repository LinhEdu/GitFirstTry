using BookWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.ViewModels;

public class ProductVM
{
    public Product Product { get; set; }
    public IEnumerable<SelectListItem> CategoryList { get; set; }
}