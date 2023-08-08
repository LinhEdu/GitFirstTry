using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookWeb.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    [Required]
    public string NoPage { get; set; }
    public string ImageUrl { get; set; }
    [Required]
    public int Price { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}