using BookWeb.Contast;
using BookWeb.Services.IServices;
using BookWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize(Roles = SD.Admin_Role)]

public class UserManagementController : BaseController
{
    private readonly IUserServices _userServices;

    public UserManagementController(IUserServices userServices)
    {
        _userServices = userServices;
    }
    
    //Get
    [HttpGet]
    public async Task<ActionResult> Index()
    {
        // // lấy id của người đăng nhập hiện tại
        // var currentUserId = GetCurrentUserId();
        //
        // // dùng để tránh trường hợp xóa nhầm role của mình
        // var userList = _db.Users.Where(u => u.Id != currentUserId);
        //
        // foreach (var user in userList)
        // {
        //     var roleTemp = await _userManager.GetRolesAsync(user);
        //     user.Role = roleTemp.FirstOrDefault();
        // }
        //
        // return View(userList.ToList());
        
        // lấy id của người đăng nhâ hiện tại
        var currentUserId = GetCurrentUserId();
        var userList = await _userServices.GetAllUser(currentUserId);

        return View(userList);
    }

    [HttpGet]
    public async Task<IActionResult> Update(string id)
    {
        // if (id != null)
        // {
        //     //khoi tao 
        //     UserVM userVm = new UserVM();
        //     //get user data
        //     var user = _db.Users.Find(id);
        //     //gan du lieu cho obj user in UserVm
        //     userVm.User = user;
        //     //lay role hien tai cua user duoc chon
        //     var roleTemp = await _userManager.GetRolesAsync(user);
        //     //gan role hien tai vao bien role
        //     userVm.Role = roleTemp.First();
        //     //lay du lieu cho danh sach role
        //     userVm.Rolelist = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem()
        //     {
        //         Text = i,
        //         Value = i
        //     });
        //     return View(userVm);
        // }

        if (id != null)
        {
            var userVm = await _userServices.GetUserUpdate(id);
            return View(userVm);
        }
        
        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> Update(UserVM userVm)
    {
        // //validate du lieu
        // if (userVm.User.Id != String.Empty
        //     && userVm.User.PhoneNumber != null
        //     && userVm.User.FullName != string.Empty
        //     && userVm.User.Role != String.Empty)
        // {
        //     //kiem ra user can update
        //     var user = _db.Users.Find(userVm.User.Id);
        //     //update du lieu cho obj user
        //     user.FullName = userVm.User.FullName;
        //     user.PhoneNumber = userVm.User.PhoneNumber;
        //     user.Address = userVm.User.Address;
        //     
        //     //tim ra role hien tai cua user
        //     var oldRole = await _userManager.GetRolesAsync(user);
        //     //remove user ra khoi role cua
        //     await _userManager.RemoveFromRoleAsync(user, oldRole.First());
        //     //add user vao role moi
        //     await _userManager.AddToRoleAsync(user, userVm.Role);
        //     
        //     //update va luu thong tin
        //     _db.Users.Update(user);
        //     _db.SaveChanges();
        //     return RedirectToAction(nameof(Index));
        // }
        //
        // //truong hop mot trong cac truong hop tren bi loi
        // userVm.Rolelist = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem()
        // {
        //     Text = i,
        //     Value = i
        // });
        // return View(userVm);
        
        // validate dữ liệu
        if (userVm.User.Id != String.Empty 
            && userVm.User.PhoneNumber != null 
            && userVm.User.FullName != string.Empty 
            && userVm.User.Role != String.Empty)
        {
            await _userServices.Update(userVm);
            return RedirectToAction(nameof(Index));
        }
        
        // trường hợp một trong các trường trên bị lỗi
        userVm.Rolelist = _userServices.GetRoleDropDown();
        return View(userVm);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string id)
    {
        // var user = _db.Users.Find(id);
        // if (user == null)
        // {
        //     return View();
        // }
        //
        // ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
        // {
        //     Email = user.Email
        // };
        //
        // return View(confirmEmailVm);
        
        var user = await _userServices.GetUserById(id);

        if (user == null)
        {
            return View();
        }

        ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
        {
            Email = user.Email
        };

        return View(confirmEmailVm);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
    {
        // if(ModelState.IsValid)
        // {
        //     var user = await _userManager.FindByEmailAsync(confirmEmailVm.Email);
        //
        //     var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //
        //     return RedirectToAction("ResetPassword", "UserManagement", new { token = token, email = user.Email });
        // }
        //
        // return View(confirmEmailVm);
        
        if (ModelState.IsValid)
        {
            var (email, token) = await _userServices.ConfirmEmail(confirmEmailVm);

            return RedirectToAction("ResetPassword", "UserManagement", 
                new { token = token, email = email });
        }

        return View(confirmEmailVm);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string token, string email)
    {
        // if (token == null || email == null) 
        // {
        //     ModelState.AddModelError("","Invalid password reset token");
        // }
        //
        // ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel()
        // {
        //     Email = email,
        //     Token = token
        // };
        // return View (resetPasswordViewModel);
        
        if (token == null || email == null)
        {
            ModelState.AddModelError("", "Invalid password reset token");
        }

        ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel()
        {
            Email = email,
            Token = token
        };
        return View(resetPasswordViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        // if (ModelState.IsValid)
        // {
        //     var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
        //     if (user != null)
        //     {
        //         var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token,
        //             resetPasswordViewModel.Password);
        //         if (result.Succeeded)
        //         {
        //             return RedirectToAction(nameof(Index));
        //         }
        //     }
        // }
        // return View(resetPasswordViewModel);
        
        if (ModelState.IsValid)
        {
            var isSuccessFully = await _userServices.ResetPassword(resetPasswordViewModel);
            if (isSuccessFully)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        return View(resetPasswordViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        // var user = await _userManager.FindByIdAsync(id);
        // if (user == null)
        // {
        //     return  NotFound();
        // }
        //
        // await _userManager.DeleteAsync(user);
        // return RedirectToAction(nameof(Index));
        
        var isSuccessfully = await _userServices.Delete(id);
        if (!isSuccessfully)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> LockUnlock(string id)
    {
        // var claimsIdentity = (ClaimsIdentity)User.Identity;
        // var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //
        // var userNeedToLock = _db.Users.Where(u => u.Id == id).First();
        // if (userNeedToLock.Id == claims.Value)
        // {
        //     
        // }
        //
        // if (userNeedToLock.LockoutEnd != null && userNeedToLock.LockoutEnd > DateTime.Now)
        // {
        //     userNeedToLock.LockoutEnd = DateTime.Now;
        // }
        // else
        // {
        //     userNeedToLock.LockoutEnd = DateTime.Now.AddYears(1);
        // }
        //
        // _db.SaveChanges();
        // return RedirectToAction(nameof(Index));
        
        await _userServices.LockUnlock(GetCurrentUserId(), id);
        return RedirectToAction(nameof(Index));
    }
}