using ProductManager.Entities;

namespace ProductManager.Messages;

public record ProductAddedMessage(Product newProduct);