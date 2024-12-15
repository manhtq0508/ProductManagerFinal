using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProductManager.CombineData;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
namespace ProductManager.Helpers;

public class ExcelHelper(DatabaseService _dbService, IStoreRepo storeRepo, IBillRepo billRepo, IProductRepo productRepo, IBillDetailRepo billDetailRepo)
{
    // Export
    public async Task Export(string path)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var excelPackage = new ExcelPackage())
        {
            await ExportStores(excelPackage);
            await ExportProducts(excelPackage);
            await ExportBills(excelPackage);

            excelPackage.SaveAs(new FileInfo(path));
        }
    }

    private async Task ExportStores(ExcelPackage excelPackage)
    {
        const int ID_COLUMN = 1;
        const int NAME_COLUMN = 2;
        const int ADDRESS_COLUMN = 3;

        var worksheet = excelPackage.Workbook.Worksheets.Add("Stores");

        // Header
        worksheet.Cells[1, ID_COLUMN].Value = "ID";
        worksheet.Cells[1, NAME_COLUMN].Value = "Name";
        worksheet.Cells[1, ADDRESS_COLUMN].Value = "Address";

        for (int i = ID_COLUMN; i <= ADDRESS_COLUMN; i++)
        {
            worksheet.Cells[1, i].Style.Font.Bold = true;
            worksheet.Cells[1, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        // Data
        int row = 2; // Start from row 2, row 1 is header
        var stores = await storeRepo.GetStoresAsync();
        foreach (var store in stores)
        {
            worksheet.Cells[row, ID_COLUMN].Value = store.Id;
            worksheet.Cells[row, NAME_COLUMN].Value = store.Name;
            worksheet.Cells[row, ADDRESS_COLUMN].Value = store.Address;
            row++;
        }

        worksheet.Cells.AutoFitColumns();
    }

    private async Task ExportProducts(ExcelPackage excelPackage)
    {
        const int ID_COLUMN = 1;
        const int NAME_COLUMN = 2;
        const int PRICE_COLUMN = 3;

        var worksheet = excelPackage.Workbook.Worksheets.Add("Products");

        // Header
        worksheet.Cells[1, ID_COLUMN].Value = "ID";
        worksheet.Cells[1, NAME_COLUMN].Value = "Name";
        worksheet.Cells[1, PRICE_COLUMN].Value = "Price";

        for (int i = ID_COLUMN; i <= PRICE_COLUMN; i++)
        {
            worksheet.Cells[1, i].Style.Font.Bold = true;
            worksheet.Cells[1, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        // Data
        int row = 2; // Start from row 2, row 1 is header
        var products = await productRepo.GetProductsAsync();
        foreach (var product in products)
        {
            worksheet.Cells[row, ID_COLUMN].Value = product.Id;
            worksheet.Cells[row, NAME_COLUMN].Value = product.Name;
            worksheet.Cells[row, PRICE_COLUMN].Value = product.Price;
            worksheet.Cells[row, PRICE_COLUMN].Style.Numberformat.Format = "#,##0";
            row++;
        }

        worksheet.Cells.AutoFitColumns();
    }

    private async Task ExportBills(ExcelPackage excelPackage)
    {
        var stores = await storeRepo.GetStoresAsync();

        foreach (var store in stores)
        {
            int row = 1; // Start from row 1

            List<Bill> bills = new(await billRepo.GetBillsOfStoreAsync(store.Id));

            var worksheet = excelPackage.Workbook.Worksheets.Add($"{store.Id}_{store.Name}");

            foreach (var bill in bills)
            {
                List<ProductInBill> products = new(await billDetailRepo.GetListProductInBillAsync(bill.Id));

                ExportBillInfo(worksheet, bill, ref row);
                ExportProductsInBill(worksheet, products, ref row);
            }
        }
    }

    private void ExportBillInfo(ExcelWorksheet worksheet, Bill bill, ref int rowStart)
    {
        int ROW_BILL_ID = rowStart;
        int ROW_CREATED_AT = rowStart + 1;
        int HEADER_COLUMN = 1;
        int DATA_COLUMN = 2;

        worksheet.Cells[ROW_BILL_ID, HEADER_COLUMN].Value = "Bill ID:";
        worksheet.Cells[ROW_BILL_ID, HEADER_COLUMN].Style.Font.Bold = true;
        worksheet.Cells[ROW_BILL_ID, DATA_COLUMN].Value = bill.Id;
        worksheet.Cells[ROW_BILL_ID, DATA_COLUMN].Style.Font.Bold = true;

        worksheet.Cells[ROW_CREATED_AT, HEADER_COLUMN].Value = "Created at:";
        worksheet.Cells[ROW_CREATED_AT, HEADER_COLUMN].Style.Font.Bold = true;
        worksheet.Cells[ROW_CREATED_AT, DATA_COLUMN].Value = bill.CreatedDateTime.ToString();
        worksheet.Cells[ROW_CREATED_AT, DATA_COLUMN].Style.Font.Bold = true;
        worksheet.Cells.AutoFitColumns();

        rowStart += 2; // Skip 2 rows for Bill ID and Created at
    }

    private void ExportProductsInBill(ExcelWorksheet worksheet, List<ProductInBill> products, ref int rowStart)
    {
        int ROW_HEADER = rowStart;
        int ROW_DATA_START = rowStart + 1;
        int ID_COLUMN = 1;
        int NAME_COLUMN = 2;
        int PRICE_COLUMN = 3;
        int QUANTITY_COLUMN = 4;
        int TOTAL_COLUMN = 5;

        int BILL_TOTAL_TITLE_COLUMN = QUANTITY_COLUMN; // Column to display title of bill total
        int BILL_TOTAL_DATA_COLUMN = TOTAL_COLUMN; // Column to display total of bill

        worksheet.Cells[ROW_HEADER, ID_COLUMN].Value = "Product ID";
        worksheet.Cells[ROW_HEADER, NAME_COLUMN].Value = "Product Name";
        worksheet.Cells[ROW_HEADER, PRICE_COLUMN].Value = "Product Price";
        worksheet.Cells[ROW_HEADER, QUANTITY_COLUMN].Value = "Product Quantity";
        worksheet.Cells[ROW_HEADER, TOTAL_COLUMN].Value = "Product Total";
        for (int i = ID_COLUMN; i <= TOTAL_COLUMN; i++)
        {
            worksheet.Cells[ROW_HEADER, i].Style.Font.Bold = true;
            worksheet.Cells[ROW_HEADER, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[ROW_HEADER, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }
        int row = ROW_DATA_START;
        foreach (var product in products)
        {
            worksheet.Cells[row, ID_COLUMN].Value = product.Id;
            worksheet.Cells[row, NAME_COLUMN].Value = product.Name;
            worksheet.Cells[row, PRICE_COLUMN].Value = product.Price;
            worksheet.Cells[row, PRICE_COLUMN].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row, QUANTITY_COLUMN].Value = product.Quantity;
            worksheet.Cells[row, TOTAL_COLUMN].Formula = $"= {worksheet.Cells[row, PRICE_COLUMN].Address} * {worksheet.Cells[row, QUANTITY_COLUMN].Address}";
            worksheet.Cells[row, TOTAL_COLUMN].Style.Numberformat.Format = "#,##0";
            row++;
        }

        worksheet.Cells[row, BILL_TOTAL_TITLE_COLUMN].Value = "Total:";
        worksheet.Cells[row, BILL_TOTAL_TITLE_COLUMN].Style.Font.Bold = true;
        worksheet.Cells[row, BILL_TOTAL_DATA_COLUMN].Formula = $"= SUM({worksheet.Cells[ROW_DATA_START, TOTAL_COLUMN].Address}:{worksheet.Cells[row - 1, TOTAL_COLUMN].Address})";
        worksheet.Cells[row, BILL_TOTAL_DATA_COLUMN].Style.Numberformat.Format = "#,##0";

        worksheet.Cells.AutoFitColumns();

        rowStart = row + 1; // Skip 1 row for total
    }

    // Import
    public async Task Import(string path)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var excelPackage = new ExcelPackage(new FileInfo(path)))
        {
            var stores = await ImportStores(excelPackage);
            await ImportProducts(excelPackage);
            await ImportBills(excelPackage, stores);
        }

        _dbService.AppDbContext.ChangeTracker.Clear();
    }

    private async Task<List<Store>> ImportStores(ExcelPackage excelPackage)
    {
        var worksheet = excelPackage.Workbook.Worksheets["Stores"];
        if (worksheet == null)
            throw new Exception("Missing 'Stores' worksheet");

        const int ID_COLUMN = 1;
        const int NAME_COLUMN = 2;
        const int ADDRESS_COLUMN = 3;

        int row = 2; // Start from row 2, row 1 is header

        List<Store> stores = new();
        while (worksheet.Cells[row, ID_COLUMN].Value != null)
        {
            var store = new Store
            {
                Id = worksheet.Cells[row, ID_COLUMN].Value.ToString() ?? throw new Exception("Store ID is required"),
                Name = worksheet.Cells[row, NAME_COLUMN].Value.ToString() ?? throw new Exception("Store name is required"),
                Address = worksheet.Cells[row, ADDRESS_COLUMN].Value.ToString() ?? throw new Exception("Store address is required")
            };

            stores.Add(store);
            row++;
        }

        await storeRepo.AddListStoresAsync(stores);

        return stores;
    }

    private async Task ImportProducts(ExcelPackage excelPackage)
    {
        var worksheet = excelPackage.Workbook.Worksheets["Products"];
        if (worksheet == null)
            throw new Exception("Missing 'Products' worksheet");

        const int ID_COLUMN = 1;
        const int NAME_COLUMN = 2;
        const int PRICE_COLUMN = 3;

        int row = 2; // Start from row 2, row 1 is header

        List<Product> products = new();
        while (worksheet.Cells[row, ID_COLUMN].Value != null)
        {
            var product = new Product
            {
                Id = worksheet.Cells[row, ID_COLUMN].Value.ToString() ?? throw new Exception("Product ID is required"),
                Name = worksheet.Cells[row, NAME_COLUMN].Value.ToString() ?? throw new Exception("Product name is required"),
                Price = int.Parse(worksheet.Cells[row, PRICE_COLUMN].Value.ToString() ?? throw new Exception("Product price is required"))
            };
            products.Add(product);
            row++;
        }

        await productRepo.AddListProductsAsync(products);
    }

    private async Task ImportBills(ExcelPackage excelPackage, List<Store> stores)
    {
        foreach (var store in stores)
        {
            var worksheet = excelPackage.Workbook.Worksheets[$"{store.Id}_{store.Name}"];
            if (worksheet == null)
                throw new Exception($"Missing '{store.Id}_{store.Name}' worksheet");

            int row = 1; // Start from row 1

            // Import bills
            while (worksheet.Cells[row, 1].Value != null)
            {
                var billId = await ImportBillInfo(worksheet, store.Id, row);
                row += 3; // Skip 3 rows for Bill ID, Created at and products header
                row = await ImportProductsInBill(worksheet, billId, row);
            }
        }
    }

    private async Task<string> ImportBillInfo(ExcelWorksheet worksheet, string storeId, int rowStart)
    {
        int BILL_ID_TITLE_ROW = rowStart;
        int BILL_ID_DATA_COLUMN = 2;
        int CREATED_AT_TOTAL_ROW = rowStart + 1;
        int CREATED_AT_DATA_COLUMN = 2;

        string billId = worksheet.Cells[BILL_ID_TITLE_ROW, BILL_ID_DATA_COLUMN].Value.ToString() ?? throw new Exception("Bill ID is required");
        DateTime createdAt = DateTime.Parse(worksheet.Cells[CREATED_AT_TOTAL_ROW, CREATED_AT_DATA_COLUMN].Value.ToString() ?? throw new Exception("Created at is required"));

        var bill = new Bill
        {
            Id = billId,
            CreatedDateTime = createdAt,
            StoreId = storeId
        };

        await billRepo.AddBillAsync(bill);

        return bill.Id;
    }
    private async Task<int> ImportProductsInBill(ExcelWorksheet worksheet, string billId, int rowStart)
    {
        int ID_COLUMN = 1;
        int QUANTITY_COLUMN = 4;

        int row = rowStart;

        List<BillDetail> billDetails = new();

        while (worksheet.Cells[row, ID_COLUMN].Value != null)
        {
            string productId = worksheet.Cells[row, ID_COLUMN].Value.ToString() ?? throw new Exception("Product ID is required");
            int quantity = int.Parse(worksheet.Cells[row, QUANTITY_COLUMN].Value.ToString() ?? throw new Exception("Product quantity is required"));
            var billDetail = new BillDetail
            {
                BillId = billId,
                ProductId = productId,
                Quantity = quantity
            };
            billDetails.Add(billDetail);
            row++;
        }

        await billDetailRepo.AddListBillDetailAsync(billDetails);

        return rowStart + billDetails.Count + 1; // Skip 1 row for total
    }
}
