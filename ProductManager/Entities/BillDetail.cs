using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProductManager.Entities;

[PrimaryKey(nameof(BillId), nameof(ProductId))]
public class BillDetail
{
    public required string BillId { get; set; }
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }

    // Navigation properties
    public Bill? Bill { get; set; }
    public Product? Product { get; set; }
}
