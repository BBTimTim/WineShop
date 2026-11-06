using Winery.Models;
using System.Collections.Generic;

namespace Winery.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;

        public List<Product> RelatedProducts { get; set; } = new List<Product>();

        public bool IsInWishlist { get; set; }
    }
}
