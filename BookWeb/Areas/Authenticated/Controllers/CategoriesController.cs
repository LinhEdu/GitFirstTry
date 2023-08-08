using BookWeb.Contast;
using BookWeb.Data;
using BookWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;

namespace BookWeb.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize(Roles = SD.StoreOwner_Role + "," +SD.Admin_Role)]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _hostEnvironment;

    public CategoriesController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
    {
        _db = db;
        _hostEnvironment = hostEnvironment;
    }
    
    //GET
    [Authorize(Roles = SD.StoreOwner_Role)]
    // abc = c

    public IActionResult Index(string searchString)
    {
        var listCategories = _db.Categories.ToList();
        if (!String.IsNullOrEmpty(searchString))
        {
            listCategories = listCategories.Where(c => c.Name.Contains(searchString)).ToList();
        }
        return View(listCategories);
    }

    [Authorize(Roles = SD.Admin_Role)]
    public IActionResult IndexWaitToApprove(string searchString)
    {
        var listCategories = _db.Categories.Where(_ => _.Status == SD.Category_Status_Added).ToList();
        if (!String.IsNullOrEmpty(searchString))
        {
            listCategories = listCategories.Where(c => c.Name.Contains(searchString)).ToList();
        }

        return View(listCategories);
    }

    [HttpGet]
    public IActionResult Upsert(int? id)
    {
        //create
        if (id == null)
        {
            return View(new Category());
        }
        
        //update
        var findCategory = _db.Categories.Find(id);

        return View(findCategory);
    }

    [HttpPost]
    public IActionResult Upsert(Category category)
    {
        if (category.Name != String.Empty)
        {
            //create 
            if (category.Id == 0)
            {
                category.Status = SD.Category_Status_Added;
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            
            //update
            category.Status = SD.Category_Status_Added;
            _db.Categories.Update(category);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    public IActionResult Delete(int id)
    {
        var deleteId = _db.Categories.Find(id);
        _db.Categories.Remove(deleteId);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = SD.Admin_Role)]
    public IActionResult Approved(int id)
    {
        var category = _db.Categories.Find(id);
        category.Status = SD.Category_Status_Approved;
        _db.Categories.Update(category);
        _db.SaveChanges();
        return RedirectToAction(nameof(IndexWaitToApprove));
    }

    [HttpPost]
    public IActionResult Import()
    {
        var file = Request.Form.Files[0];
        if (file == null || file.Length <= 0)
            return BadRequest("No File is Seleted");

        var filePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        List<Category> importedData = ReadExcelFile(filePath);

        var categoriesNeedToAdd = new List<Category>();
        foreach (var category in importedData)
        {
            var cate = new Category()
            {
                Name = category.Name,
                Description = category.Description,
                Status = SD.Category_Status_Added
            };
            categoriesNeedToAdd.Add(cate);

        }
        _db.Categories.AddRange(categoriesNeedToAdd);
        _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    private List<Category> ReadExcelFile(string filePath)
    {
        var data = new List<Category>();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row < rowCount; row++)
            {
                var rowData = new Category()
                {
                    Name = worksheet.Cells[row, 1].Value.ToString(),
                    Description = worksheet.Cells[row, 2].Value.ToString(),
                };
                
                data.Add(rowData);
            }
        }

        return data;
    }


    public IActionResult Create()
    {
        return View(new Category());
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        _db.Categories.Add(category);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Update(int categoryId)
    {
        var category = _db.Categories.Find(categoryId);
        return View(category);
    }

    [HttpPost]
    public IActionResult Update(Category category)
    {
        if (ModelState.IsValid)
        {
            var categoryFromDB = _db.Categories.Find(category.Id);
            categoryFromDB.Name = category.Name;
            categoryFromDB.Description = category.Description;
            categoryFromDB.Status = category.Status;

            _db.Categories.Update(categoryFromDB);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }
}