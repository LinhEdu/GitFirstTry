using BookWeb.Models;
using BookWeb.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Services.IServices;

public interface IUserServices
{
    Task<List<User?>> GetAllUser(string currentUserId);
    Task LockUnlock(string currentUserId, string id);
    Task<User?> GetUserById(string id);
    List<SelectListItem> GetRoleDropDown();
    Task<UserVM> GetUserUpdate(string id);
    Task Update(UserVM userVm);
    Task<(string, string)> ConfirmEmail(ConfirmEmailVM confirmEmailVm);
    Task<bool> Delete(string id);
    Task<bool> ResetPassword(ResetPasswordViewModel resetPasswordViewModel);
}
