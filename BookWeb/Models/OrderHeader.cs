using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookWeb.Models;

public class OrderHeader
{
    [Key] 
    public int Id { get; set; }
    public int Total { get; set; }
    [Required] 
    public string Address { get; set; }
    [Required] 
    public DateTime OrderDate { get; set; }
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string Name { get; set; }
}