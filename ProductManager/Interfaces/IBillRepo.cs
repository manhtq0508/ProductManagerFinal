﻿using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IBillRepo
{
    Task<IEnumerable<Bill>> GetBillsOfStoreAsync(string storeId);
    Task<Bill> GetBillByIdAsync(string billId);
    Task AddBillAsync(Bill bill);
    Task AddListBillsAsync(List<Bill> bills);
    Task UpdateBillAsync(Bill bill);
    Task DeleteBillAsync(Bill bill);
    Task DeleteListBillsAsync(List<Bill> bills);
}
