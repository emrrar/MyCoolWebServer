﻿namespace MyCoolWebServer.ByTheCakeApplication.Services.Contracts
{
    using System.Collections.Generic;

    public interface IShoppingService
    {
        void CreateOrder(int userId, IEnumerable<int> productIds);
    }
}
