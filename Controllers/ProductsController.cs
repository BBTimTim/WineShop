using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Winery.Models;
using Winery.Services;
using Winery.ViewModels;

namespace Winery.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<User> _userManager;
        private readonly IShoppingCartService _cartService;
        // Injecting the Product Service through the constructor
        public ProductsController(IProductService productService, IShoppingCartService cartService, UserManager<User> userManager)
        {
            _productService = productService;
            _cartService = cartService;
            _userManager = userManager;
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
            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
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
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                isInWishlist = user?.WishlistItems.Any(w => w.ProductId == id) ?? false;
            }
            // 5. Build the ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                IsInWishlist = isInWishlist
            };
            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _productService.AddReviewAsync(productId, user.Id, model);
            if (result)
            {
                TempData["Success"] = "Thank you for your review!";
            }
            else
            {
                TempData["Error"] = "Failed to add review. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = productId });
        }

    }
}
