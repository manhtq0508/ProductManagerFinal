using ProductManager.Data;
using ProductManager.Interfaces;
using ProductManager.Services;
using ProductManager.ViewModels;
using ProductManager.Views;

namespace ProductManager.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseService>();

        services.AddScoped<IBillRepo, BillRepo>();
        services.AddScoped<IBillDetailRepo, BillDetailRepo>();
        services.AddScoped<IProductRepo, ProductRepo>();
        services.AddScoped<IStoreRepo, StoreRepo>();

        services.AddSingleton<MainPage>();
        services.AddSingleton<MainViewModel>();

        // Store
        services.AddTransient<StorePage>();
        services.AddTransient<StoreViewModel>();
        services.AddTransient<AddStorePage>();
        services.AddTransient<AddStoreViewModel>();
        services.AddTransient<EditStorePage>();
        services.AddTransient<EditStoreViewModel>();

        // Product
        services.AddTransient<ProductPage>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<AddProductPage>();
        services.AddTransient<AddProductViewModel>();
        services.AddTransient<EditProductPage>();
        services.AddTransient<EditProductViewModel>();

        // Bill
        services.AddTransient<BillPage>();
        services.AddTransient<BillViewModel>();
        services.AddTransient<AddBillPage>();
        services.AddTransient<AddBillViewModel>();
        services.AddTransient<EditBillPage>();
        services.AddTransient<EditBillViewModel>();

        // BillDetail
        services.AddTransient<SelectProductInBillPage>();
        services.AddTransient<SelectProductInBillViewModel>();
        services.AddTransient<BillDetailPage>();
        services.AddTransient<BillDetailViewModel>();

        return services;
    }
}