using BookWeb.Contast;
using BookWeb.Data;
using BookWeb.Models;
using BookWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize(Roles = SD.StoreOwner_Role)]
public class OrderOverViewsController : Controller
{
    private readonly ApplicationDbContext _db;
    [BindProperty] public OrderDetailsVM OrderVM { get; set; }

    public OrderOverViewsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var orderHeaderList = _db.OrderHeaders.Include(u => u.User).ToList();

        return View(orderHeaderList);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        OrderVM = new OrderDetailsVM()
        {
            OrderHeader = _db.OrderHeaders.Where(u => u.Id == id).Include(u => u.User).FirstOrDefault(),
            OrderDetails = _db.OrderDetails.Where(o => o.OrderHeaderId == id).Include(u => u.Product)
        };
        return View(OrderVM);
    }
}