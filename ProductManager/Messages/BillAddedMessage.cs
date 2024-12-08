using ProductManager.Entities;

namespace ProductManager.Messages;

public record BillAddedMessage(Bill newBill);