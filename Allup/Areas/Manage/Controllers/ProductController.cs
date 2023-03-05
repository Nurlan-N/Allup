using Allup.DataAccessLayer;
using Allup.Extentions;
using Allup.Helpers;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]   
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment = null)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Product?> products = _context.Products.Where(p=> p.IsDeleted == false );
             
            return View(PageNatedList<Product>.Create(products,pageIndex,3));
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false ).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(b => b.IsDeleted == false).ToListAsync();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(b => b.IsDeleted == false).ToListAsync();
            if (!ModelState.IsValid) return View(product);

            if(!await _context.Brands.AnyAsync(b => b.IsDeleted ==false && b.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", $"Daxil olunan Bran Id {product.BrandId} Yalnisdir");
                return View(product);
            }
            if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", $"Daxil olunan Bran Id {product.CategoryId} Yalnisdir");
                return View(product);
            }
           
            if(product.TagIds != null && product.TagIds.Count() > 0)
            {
                List<ProductTag> productTags = new List<ProductTag>();

                foreach (var tagId in product.TagIds)
                {
                    if (!await _context.Tags.AnyAsync(c => c.IsDeleted == false && c.Id == tagId))
                    {
                        ModelState.AddModelError("TagIds", $"Daxil olunan Bran Id {tagId} Yalnisdir");
                        return View(product);
                    }
                    ProductTag productTag = new ProductTag()
                    {
                        TagId = tagId,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"
                    };
                    productTags.Add(productTag);
                }
                product.ProductTags = productTags;
            }
            else
            {
                ModelState.AddModelError("TagIds", "Tag Secilmelidir");
                return View(product);
            }

            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Main File Yalniz JPG Formatda ola biler");
                    return View(product);
                }
                if (!product.MainFile.CheckFileLength(300))
                {
                    ModelState.AddModelError("MainFile", "Main File Yalniz 300Kb  ola biler");
                    return View(product);
                }
                product.MainImage = await product.MainFile.CreateFileAsync(_webHostEnvironment, "assets", "images");
            }
            else
            {
                ModelState.AddModelError("MainFile", "Main File mutleqdir");
                return View(product);
            }

            if (product.HoverFile != null)
            {
                if (!product.HoverFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverFile", "Main File Yalniz JPG Formatda ola biler");
                    return View(product);
                }
                if (!product.HoverFile.CheckFileLength(300))
                {
                    ModelState.AddModelError("HoverFile", "Main File Yalniz 300Kb  ola biler");
                    return View(product);
                }
                product.HoverImage = await product.HoverFile.CreateFileAsync(_webHostEnvironment, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("MainFile", "Main File mutleqdir");
                return View(product);
            }
            if (product.Files == null)
            {
                ModelState.AddModelError("Files", $"Sekil Mutleqdir!!!");
                return View(product);
            }
            if (product.Files.Count() > 6)
            {
                ModelState.AddModelError("Files", $"Maksimum 6  sekil yukleye bilersiniz");
                return View(product);
            }
            if (product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();
                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("file", "Main File Yalniz JPG Formatda ola biler");
                        return View(product);
                    }
                    if (!file.CheckFileLength(300))
                    {
                        ModelState.AddModelError("file", "Main File Yalniz 300Kb  ola biler");
                        return View(product);
                    }
                    ProductImage productImage = new ProductImage()
                    {
                        Image = await file.CreateFileAsync(_webHostEnvironment, "assets", "images", "product"),
                        CreatedAt = DateTime.UtcNow.AddDays(4),
                        CreatedBy = "System"
                    };
                    productImages.Add(productImage);
                }
                product.ProductImages = productImages;

            }





            string code = product.Title.Substring(0, 2);
            code = code + _context.Brands.FirstOrDefault(b => b.Id == product.BrandId).Name.Substring(0, 1);
            code = code + _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId).Name.Substring(0, 1);
            product.Seria = code.ToLower().Trim();
            product.Code = _context.Products.Where(p => p.Seria == product.Seria)
                .OrderByDescending(p => p.Id).FirstOrDefault() != null ?
                _context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault().Code += 1 : 1;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages.Where(t => t.IsDeleted == false))
                .Include(p => p.ProductTags.Where(i => i.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
            if (product == null) return NotFound();

            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(b => b.IsDeleted == false).ToListAsync();

            product.TagIds = product.ProductTags != null && product.ProductTags.Count() > 0 ?
                product.ProductTags.Select(x => (byte)x.TagId).ToList() : new List<byte>();

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Uptade(int? id , Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(b => b.IsDeleted == false).ToListAsync();

            if(!ModelState.IsValid) return View();

            if(id == null || id !=product.Id ) return BadRequest();
            
            Product dbProduct = await _context.Products
                .Include(p => p.ProductImages.Where(pImages => pImages.IsDeleted == false))
                .Include(t => t.ProductTags.Where(t => t.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
            
            if (dbProduct == null) return NotFound();

            int canUpload = 6 - dbProduct.ProductImages.Count();
            if(product.Files != null &&  canUpload < product.Files.Count())
            {
                ModelState.AddModelError("Files", $"Maksimum {canUpload} Qeder sekil yukleye bilersiniz");
                return View(product);

            }
            if (product.Files != null && product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();
                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("file", "Main File Yalniz JPG Formatda ola biler");
                        return View(product);
                    }
                    if (!file.CheckFileLength(300))
                    {
                        ModelState.AddModelError("file", "Main File Yalniz 300Kb  ola biler");
                        return View(product);
                    }
                    ProductImage productImage = new ProductImage()
                    {
                        Image = await file.CreateFileAsync(_webHostEnvironment, "assets", "images", "product"),
                        CreatedAt = DateTime.UtcNow.AddDays(4),
                        CreatedBy = "System"
                    };
                    productImages.Add(productImage);
                }

                dbProduct.ProductImages.AddRange(productImages);
            }
            //StartHoverFile
            if (product.HoverFile != null)
            {
                if (!product.HoverFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverFile", "Main File Yalniz JPG Formatda ola biler");
                    return View(product);
                }
                if (!product.HoverFile.CheckFileLength(300))
                {
                    ModelState.AddModelError("HoverFile", "Main File Yalniz 300Kb  ola biler");
                    return View(product);
                }
                FileHelpers.DeleteFile(dbProduct.HoverImage, _webHostEnvironment, "assets", "images", "product");

                dbProduct.HoverImage = await product.HoverFile.CreateFileAsync(_webHostEnvironment, "assets", "images");
            }
            //StartMainFile
            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Main File Yalniz JPG Formatda ola biler");
                    return View(product);
                }
                if (!product.MainFile.CheckFileLength(300))
                {
                    ModelState.AddModelError("MainFile", "Main File Yalniz 300Kb  ola biler");
                    return View(product);
                }
                FileHelpers.DeleteFile(dbProduct.MainImage,_webHostEnvironment,"assets","images","product");

                dbProduct.MainImage = await product.MainFile.CreateFileAsync(_webHostEnvironment, "assets", "images");
            }
            if (product.Price != null) { dbProduct.Price = product.Price; }
            if (product.DiscountedPrice != null) { dbProduct.DiscountedPrice = product.DiscountedPrice; }
            if (product.Count != null) { dbProduct.Count = product.Count; }
            if (product.ExTax != null) { dbProduct.ExTax = product.ExTax; }
            if (product.Description != null) { dbProduct.Description = product.Description; }



            if (product.TagIds != null && product.TagIds.Count() > 0)
            {
                _context.ProductTags.RemoveRange(dbProduct.ProductTags);

                List<ProductTag> productTags = new List<ProductTag>();

                foreach (var tagId in product.TagIds)
                {
                    if (!await _context.Tags.AnyAsync(c => c.IsDeleted == false && c.Id == tagId))
                    {
                        ModelState.AddModelError("TagIds", $"Daxil olunan Bran Id {tagId} Yalnisdir");
                        return View(product);
                    }
                    ProductTag productTag = new ProductTag()
                    {
                        TagId = tagId,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"
                    };
                    productTags.Add(productTag);
                }
                dbProduct.ProductTags = productTags;
            }
            else
            {
                ModelState.AddModelError("TagIds", "Tag Secilmelidir");
                return View(product);
            }
            await _context.SaveChangesAsync();



            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            if (id == null) { return BadRequest(); }

            if (imageId == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(P => P.IsDeleted == false && P.Id == id);

            if (product == null) return NotFound();
            if (product.ProductImages?.Count() <= 1)
            {
                return BadRequest();
            }

            if (!product.ProductImages.Any(p => p.Id == imageId)) { return BadRequest(); }

            product.ProductImages.FirstOrDefault(product => product.Id == imageId).IsDeleted = true;
            await _context.SaveChangesAsync();

            FileHelpers.DeleteFile(product.ProductImages.FirstOrDefault(product => product.Id == imageId).Image, _webHostEnvironment, "assets", "images", "product");

           List<ProductImage> productImages = product.ProductImages.Where(p => p.IsDeleted == false).ToList();

            return PartialView("_ProductImagePartial", productImages);
        }
    }
}
