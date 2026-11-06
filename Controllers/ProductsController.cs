using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Winery.Models;
using Winery.Services;
using Winery.ViewModels;
using System.Threading.Tasks;

namespace Winery.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        // Injecting the Product Service through the constructor
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
 
       /// Displays a filtered and paginated list of products
        public async Task<IActionResult> Index(
            int? categoryId,
            int? brandId,
            string? search,
            string? sort,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            // Mapping URL parameters to our ViewModel
            var filter = new ProductListViewModel
            {
                SelectedCategoryId = categoryId,
                SelectedBrandId = brandId,
                SearchTerm = search,
                SortOrder = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = page
            };

            // Calling the service to handle the filtering logic
            var result = await _productService.GetProductsAsync(filter);

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _productService.GetRelatedProductsAsync(id);

            // 4. Placeholder for Wishlist logic (can be expanded later)
            bool isInWishlist = false;

            // 5. Build the ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                IsInWishlist = isInWishlist
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var filter = new ProductListViewModel
            {
                SearchTerm = term,
                PageSize = 5
            };

            var result = await _productService.GetProductsAsync(filter);
            var suggestions = result.Products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.SalePrice,
                image = p.MainImageUrl,
                brand = p.Brand?.Name
            });

            return Json(suggestions);
        }
    }
}
