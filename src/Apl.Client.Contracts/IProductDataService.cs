using APLPX.Entity;
using System.Collections.Generic;

namespace APLPX.Client.Contracts
{
    public interface IProductDataService
    {
        List<ProductDetail> GetProductDetail(int[] filterIds, int[] priceListIds);
    }
}
