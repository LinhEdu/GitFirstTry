using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookWeb.Models;

public class OrderDetails
{
    [Key]
    public int Id { get; set; }
        
    public int Price { get; set; }
        
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product   { get; set; }
        
    public int OrderHeaderId { get; set; }
    [ForeignKey("OrderHeaderId")]
    public OrderHeader OrderHeader   { get; set; }
        
    public int Quantity { get; set; }
}