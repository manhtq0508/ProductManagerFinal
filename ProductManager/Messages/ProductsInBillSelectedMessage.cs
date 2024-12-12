using ProductManager.CombineData;

namespace ProductManager.Messages;

public record ProductsInBillSelectedMessage(List<ProductInBill> productsInBill);
