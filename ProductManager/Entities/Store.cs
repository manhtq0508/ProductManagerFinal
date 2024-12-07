using System.ComponentModel.DataAnnotations;

namespace ProductManager.Entities;

public class Store
{
    [Key]
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }

    // Navigation properties
    public List<Bill> Bills { get; set; } = [];
}
