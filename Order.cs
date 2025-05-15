using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model;

public class Order
{
    public int IdOrder { get; set; }
    public int IdProduct { get; set; }
    public Product Product { get; set; }
    public int Amount { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
}