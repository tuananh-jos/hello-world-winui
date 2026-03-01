using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Data.IDataSource;
using App7.Domain.Entities;

namespace App7.Data.DataSource;
public class SampleOrderDetailDataSource : ISampleOrderDetailDataSource
{
    public Task<List<SampleOrderDetail>> GetAllAsync()
    {
        var result =
        new List<SampleOrderDetail>()
        {
            new SampleOrderDetail()
            {
                ProductID = 28,
                ProductName = "Rössle Sauerkraut",
                Quantity = 15,
                Discount = 0.25,
                QuantityPerUnit = "25 - 825 g cans",
                UnitPrice = 45.60,
                CategoryName = "Produce",
                CategoryDescription = "Dried fruit and bean curd",
                Total = 513.00
            },
            new SampleOrderDetail()
            {
                ProductID = 39,
                ProductName = "Chartreuse verte",
                Quantity = 21,
                Discount = 0.25,
                QuantityPerUnit = "750 cc per bottle",
                UnitPrice = 18.0,
                CategoryName = "Beverages",
                CategoryDescription = "Soft drinks, coffees, teas, beers, and ales",
                Total = 283.50
            },
            new SampleOrderDetail()
            {
                ProductID = 46,
                ProductName = "Spegesild",
                Quantity = 2,
                Discount = 0.25,
                QuantityPerUnit = "4 - 450 g glasses",
                UnitPrice = 12.0,
                CategoryName = "Seafood",
                CategoryDescription = "Seaweed and fish",
                Total = 18.00
            }
        };

        return Task.FromResult(result);
    }

    public Task InsertAsync(SampleOrderDetail orderDetail) => throw new NotImplementedException();
}
