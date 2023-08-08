using BookWeb.Models;

namespace BookWeb.ViewModels;

public class OrderDetailsVM
{
    public OrderHeader OrderHeader { get; set; }
    public IEnumerable<OrderDetails> OrderDetails { get; set; }
}