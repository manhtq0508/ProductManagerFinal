using System.ComponentModel.DataAnnotations;

namespace ProductManager.Entities;

public class Product
{
    [Key]
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required int Price { get; set; }

    // Navigation properties
    public List<BillDetail> BillDetails { get; set; } = [];
}
