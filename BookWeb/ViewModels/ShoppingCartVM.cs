using BookWeb.Models;

namespace BookWeb.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart> ListCarts { get; set; }
    public OrderHeader OrderHeader { get; set; }
}