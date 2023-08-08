using BookWeb.Contast;
using BookWeb.Data;
using BookWeb.Models;
using BookWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace BookWeb.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize(Roles = SD.StoreOwner_Role)]

public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _hostEnvironment;

    public BooksController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
    {
        _db = db;
        _hostEnvironment = hostEnvironment;
    }

    public IActionResult Index()
    {
        var listAllData = _db.Products.Include(x => x.Category).ToList();
        ViewData["Message"] = TempData["Message"];
        return View(listAllData);
    }

    [HttpGet]
    public IActionResult Upsert(int? id)
    {
        ProductVM productVm = new ProductVM()
        {
            Product = new Product(),
            CategoryList = categoriesSelectListItems()
        };
        
        //create
        if (id == null)
        {
            return View(productVm);
        }
        
        //update
        productVm.Product = _db.Products.Find(id);

        return View(productVm);
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVm)
    {
        //validate dữ liệu
        if (productVm.Product.Title != String.Empty
            && productVm.Product.NoPage != String.Empty
            && productVm.Product.Price > 0
            && productVm.Product.Quantity > 0
            && productVm.Product.CategoryId != 0)
        {
            // webRootPath - đường dẫn của máy tính tới folder wwwroot 
            string webRootPath = _hostEnvironment.WebRootPath;
            // lấy file người truyền vào
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                // generate filename
                string fileName = Guid.NewGuid().ToString();
                
                // tạo ra đường dẫn để tới folder product
                var uploads = Path.Combine(webRootPath, @"images/products");
                // lấy ra đuôi file
                var extension = Path.GetExtension(files[0].FileName);
                
                // trong trường hợp update người dùng thay thế bức ảnh thành bức ảnh mới
                if (productVm.Product.ImageUrl != null)
                {
                    // to edit path so we need to delete the old path and update new one
                    var imagePath = Path.Combine(webRootPath, productVm.Product.ImageUrl.TrimStart('/'));
                    // kiểm tra xem bức ảnh đó có nằm trong folder ko 
                    if (System.IO.File.Exists(imagePath))
                    {
                        // nếu có xóa đi
                        System.IO.File.Delete(imagePath);
                    }
                }

                // lưu dữ liệu bức ảnh vào folder products
                using (var filesStreams =
                       new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStreams);
                }

                // lưu đường dẫn file
                productVm.Product.ImageUrl = @"/images/products/" + fileName + extension;
            }
            else
            {
                //update without change the images
                if (productVm.Product.Id != 0)
                {
                    Product objFromDb = _db.Products.Find(productVm.Product.Id);
                    productVm.Product.ImageUrl = objFromDb.ImageUrl;
                }
            }

            // create
            if (productVm.Product.Id == 0)
            {
                _db.Products.Add(productVm.Product);
                _db.SaveChanges();
                TempData["Message"] = "Success: Add Successfully";
                return RedirectToAction(nameof(Index));
            }

            // update
            var productDb = _db.Products.Find(productVm.Product.Id);
            productDb.Author = productVm.Product.Author;
            productDb.Title = productVm.Product.Title;
            productDb.Category = productVm.Product.Category;
            productDb.Description = productVm.Product.Description;
            productDb.Price = productVm.Product.Price;
            productDb.NoPage = productVm.Product.NoPage;
            productDb.ImageUrl = productVm.Product.ImageUrl;
            productDb.Quantity = productVm.Product.Quantity;

            _db.Products.Update(productDb);
            _db.SaveChanges();
            TempData["Message"] = "Success: Update Successfully";
            return RedirectToAction(nameof(Index));
        }
        
        // trường hợp validate ko thành công thì mình sẽ trả về dữ liệu cũ 
        ViewData["Message"] = "Error: Invalid Input, Please Recheck Again";
        // lấy lại dữ liệu cho category list bởi vì khi post về thì category list == null
        productVm.CategoryList = categoriesSelectListItems();

        return View(productVm);
    }

    [HttpPost]
    public IActionResult Import()
    {
        var file = Request.Form.Files[0];
        if (file == null || file.Length <= 0)
            return BadRequest("No File is Selected");
        var filePath = Path.Combine(_hostEnvironment.WebRootPath, "Uploads", file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        List<Product> importedData = ReadExcelFile(filePath);

        var booksNeedToAdd = new List<Product>();
        foreach (var product in importedData)
        {
            var boks = new Product()
            {
                Title = product.Title,
                Description = product.Description,
                Author = product.Author,
                NoPage = product.NoPage,
                ImageUrl = String.Empty,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId
            };
            
            booksNeedToAdd.Add(boks);
        }
        
        _db.Products.AddRange(booksNeedToAdd);
        _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    private List<Product> ReadExcelFile(string filePath)
    {
        var data = new List<Product>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var categoryName = worksheet.Cells[row, 7].Value?.ToString();
                var categoryId = _db.Categories
                    .FirstOrDefault(_ => _.Name.ToLower().Trim() == categoryName.ToLower().Trim()).Id;
                var rowData = new Product()
                {
                    Title = worksheet.Cells[row, 1].Value.ToString(),
                    Description = worksheet.Cells[row, 2].Value.ToString(),
                    Author = worksheet.Cells[row, 3].Value.ToString(),
                    NoPage = worksheet.Cells[row, 4].Value.ToString(),
                    Price = int.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out int price) ? price : 0,
                    Quantity = int.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out int quantity) ? quantity : 0,
                    CategoryId = categoryId
                };
                
                data.Add(rowData);
            }
        }

        return data;
    }

    [NonAction]
    private IEnumerable<SelectListItem> categoriesSelectListItems()
    {
        var categories = _db.Categories.Where(_ => _.Status == SD.Category_Status_Approved).ToList();
        var result = categories.Select(i => new SelectListItem()
        {
            Text = i.Name,
            Value = i.Id.ToString()
        });
        return result;
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        if (id == null)
        {
            ViewData["Message"] = "Error: Id input null";
        }

        var productNeedToDelete = _db.Products.Find(id);
        _db.Products.Remove(productNeedToDelete);
        _db.SaveChanges();
        TempData["Message"] = "Success: Delete Successfully";
        return RedirectToAction(nameof(Index));
    }
}