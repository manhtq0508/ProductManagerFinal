using System.ComponentModel.DataAnnotations;

namespace ProductManager.Entities;

public class Bill
{
    [Key]
    public required string Id { get; set; }
    public required DateTime CreatedDateTime { get; set; }

    // Navigation properties
    public string? StoreId { get; set; }
    public Store? Store { get; set; }

    public List<BillDetail> BillDetails { get; set; } = [];
}
