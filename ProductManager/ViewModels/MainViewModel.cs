using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.Entities;
using ProductManager.Helpers;
using ProductManager.Interfaces;
using ProductManager.Services;
using ProductManager.Views;

namespace ProductManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly IProductRepo _productRepo;
    private readonly IStoreRepo _storeRepo;
    private readonly IBillRepo _billRepo;
    private readonly IBillDetailRepo _billDetailRepo;

    public MainViewModel(DatabaseService dbService, IProductRepo productRepo, IStoreRepo storeRepo, IBillRepo billRepo, IBillDetailRepo billDetailRepo)
    {
        _dbService = dbService;
        _dbService.InitializeDatabase("product_manager.db").Wait(); // Wait for the database to be initialized

        _productRepo = productRepo;
        _storeRepo = storeRepo;
        _billRepo = billRepo;
        _billDetailRepo = billDetailRepo;
    }

    [RelayCommand]
    private async Task OpenStores()
    {
        await Shell.Current.GoToAsync(nameof(StorePage));
    }

    [RelayCommand]
    private async Task OpenProducts()
    {
        await Shell.Current.GoToAsync(nameof(ProductPage));
    }

    [RelayCommand]
    private async Task ImportFromFile()
    {
        FilePickerFileType fileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> {
                                {DevicePlatform.WinUI, new string[] {".xlsx"} }
                            });

        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            FileTypes = fileType,
            PickerTitle = "Pick an Excel file"
        });

        if (file == null || string.IsNullOrEmpty(file.FullPath))
            return;

        try
        {
            var excelHelper = new ExcelHelper(_storeRepo, _billRepo, _productRepo, _billDetailRepo);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ExportToFile()
    {
        try
        {
            var excelHelper = new ExcelHelper(_storeRepo, _billRepo, _productRepo, _billDetailRepo);
            await excelHelper.Export("D:/");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task GenDemoData()
    {
        await ClearAllData(); // Ensure that all current data is cleared before generating demo data

        // Generate demo products
        List<Product> products = new List<Product>();
        for (int productId = 1; productId < 10; productId++)
        {
            products.Add(new Product
            {
                Id = $"PRO{productId}",
                Name = $"Product {productId}",
                Price = productId * 1000
            });
        }
        await _productRepo.AddListProductsAsync(products);

        // Generate demo stores
        List<Store> stores = new List<Store>();
        for (int storeId = 1; storeId <= 5; storeId++)
        {
            stores.Add(new Store
            {
                Id = $"STO{storeId}",
                Name = $"Store {storeId}",
                Address = $"Demo address for STO{storeId}"
            });
        }
        await _storeRepo.AddListStoresAsync(stores);

        // Generate demo bills (each store has 2 bills)
        List<Bill> bills = new List<Bill>();
        for (int storeId = 1; storeId <= 5; storeId++)
        {
            for (int billId = 1; billId <= 2; billId++)
            {
                bills.Add(new Bill
                {
                    Id = $"BIL{storeId}.{billId}",
                    StoreId = $"STO{storeId}",
                    CreatedDateTime = DateTime.Now
                });
            }
        }
        await _billRepo.AddListBillsAsync(bills);

        // Generate demo billNeedEdit details (each billNeedEdit has 5 products)
        List<BillDetail> billDetails = new List<BillDetail>();
        for (int storeId = 1; storeId <= 5; storeId++)
        {
            for (int billId = 1; billId <= 2; billId++)
            {
                for (int productId = 1; productId <= 5; productId++)
                {
                    billDetails.Add(new BillDetail
                    {
                        BillId = $"BIL{storeId}.{billId}",
                        ProductId = $"PRO{productId}",
                        Quantity = productId
                    });
                }
            }
        }
        await _billDetailRepo.AddListBillDetailAsync(billDetails);

        _dbService.AppDbContext.ChangeTracker.Clear();
    }

    [RelayCommand]
    private async Task ClearAllData()
    {
        var isYes = await Shell.Current.DisplayAlert(
            "Warning",
            "This action will delete all current data. Are you sure?",
            "Yes",
            "No");
        if (!isYes)
            return;
        try
        {
            _dbService.DeleteDatabase().Wait(); // Delete the database
            _dbService.InitializeDatabase("product_manager.db").Wait(); // Reinitialize the database
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
