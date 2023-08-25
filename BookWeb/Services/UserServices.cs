using BookWeb.Models;
using BookWeb.Repository.IRepository;
using BookWeb.Services.IServices;
using BookWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Services;

public class UserServices: IUserServices
{
    private readonly UserManager<IdentityUser?> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IRepository<User> _userRepository;
    private readonly IEmailServices _emailServices;

    public UserServices(
        IRepository<User> userRepository, 
        UserManager<IdentityUser?> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailServices emailServices)
    {
        _userManager = userManager;
        _userRepository = userRepository ;
        _roleManager = roleManager;
        _emailServices = emailServices;
    }

    public async Task<List<User?>> GetAllUser(string currentUserId)
    {
        //dùng để tránh trường hợp xóa nhầm role của mình
        var userList = await _userRepository.GetAllAsync(u => u.Id != currentUserId);

        foreach (var user in userList)
        {
            var roleTemp = await _userManager.GetRolesAsync(user);
            user.Role = roleTemp.FirstOrDefault();
        }

        await _emailServices.Send("linh1236589@gmail.com", "Test Mail Hieu Linh", "<p>Hello linh</p>");
        return userList.ToList();
    }
    
    public async Task LockUnlock(string currentUserId, string id)
    {
        // tìm kiếm user theo id
        var userNeedToLock = await _userRepository.GetByIdAsync(id);
        // chống tự khóa tài khoản chinh mình
        if (userNeedToLock.Id == currentUserId)
        {
            //hien ra loi ban dang khoa tai khoan cua chinh minh
        }

        // trường hợp tại khoản đang bị khóa tiên hành mở khóa
        if (userNeedToLock.LockoutEnd != null && userNeedToLock.LockoutEnd > DateTime.Now)
        {
            userNeedToLock.LockoutEnd = DateTime.Now;
        }
        // hiện tại tài khoản e ko bị khóa tiến hành khóa
        else
        {
            userNeedToLock.LockoutEnd = DateTime.Now.AddYears(1);
        }

        await _userRepository.SaveChangesAsync();
    }
    
    public async Task<User?> GetUserById(string id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public List<SelectListItem> GetRoleDropDown()
    {
        return _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem()
        {
            Text = i,
            Value = i
        }).ToList();
    }
    
    public async Task<UserVM> GetUserUpdate(string id)
    {
        // khởi tạo vm
        UserVM userVm = new UserVM();
        // get user data
        var user = await GetUserById(id);
        // gán dữ liệu cho obj user in UserVm
        userVm.User = user;
        // lấy role hiện tại của user được chọn
        var roleTemp = await _userManager.GetRolesAsync(user);
        // gán role hiện tại vào biến role
        userVm.Role = roleTemp.First();
        // lấy dữ liệu cho danh sách role
        userVm.Rolelist = GetRoleDropDown();

        return userVm;
    }

    public async Task Update(UserVM userVm)
    {
        // kiếm ra user cần update 
        var user = await GetUserById(userVm.User.Id);
        // update dữ liệu cho obj user
        user.FullName = userVm.User.FullName;
        user.PhoneNumber = userVm.User.PhoneNumber;
        user.Address = userVm.User.Address;

        // tìm ra role hiện tại của user
        var oldRole = await _userManager.GetRolesAsync(user);
        // remove user ra khỏi role của
        await _userManager.RemoveFromRoleAsync(user, oldRole.First());
        // add user vào role mới
        await _userManager.AddToRoleAsync(user, userVm.Role);

        // update và lưu thông tin
        _userRepository.Update(user);
        _userRepository.SaveChangesAsync();
    }

    public async Task<(string, string)> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
    {
        var user = await _userManager.FindByEmailAsync(confirmEmailVm.Email);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return (user.Email, token);
    }

    public async Task<bool> Delete(string id)
    {
        var user = await GetUserById(id);
        if (user == null)
        {
            return false;
        }
       
        await _userManager.DeleteAsync(user);
        return true;
    }

    public async Task<bool> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
        if (user != null)
        {
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token,
                resetPasswordViewModel.Password);
            if (result.Succeeded)
            {
                return true;
            }
        }

        return false;
    }
    
}