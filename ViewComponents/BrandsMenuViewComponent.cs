using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Winery.Data;

namespace Winery.ViewComponents
{
    public class BrandsMenuViewComponent : ViewComponent
    {
        private readonly DbContext _context;

        public BrandsMenuViewComponent(DbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Fetching only active brands from the database
            var brands = _context.Brands
                                 .Where(b => b.IsActive)
                                 .OrderBy(b => b.Name)
                                 .ToList();

            return View(brands);
        }
    }
}

