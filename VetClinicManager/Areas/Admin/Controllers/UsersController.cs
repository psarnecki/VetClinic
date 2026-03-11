using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetClinicManager.Areas.Admin.DTOs.Users;
using VetClinicManager.Services;

namespace VetClinicManager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    // GET: Admin/Users (Index)
    public async Task<IActionResult> Index(string sortOrder)
    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["FirstNameSortParm"] = sortOrder == "firstname" ? "firstname_desc" : "firstname";
        ViewData["LastNameSortParm"] = sortOrder == "lastname" ? "lastname_desc" : "lastname";
        ViewData["EmailSortParm"] = sortOrder == "email" ? "email_desc" : "email";
        ViewData["RolesSortParm"] = sortOrder == "roles" ? "roles_desc" : "roles";

        // Helper for icon CSS classes
        ViewData["FirstNameIcon"] = GetSortIcon(sortOrder, "firstname");
        ViewData["LastNameIcon"] = GetSortIcon(sortOrder, "lastname");
        ViewData["EmailIcon"] = GetSortIcon(sortOrder, "email");
        ViewData["RolesIcon"] = GetSortIcon(sortOrder, "roles");

        var userListDtos = await _userService.GetAllUsersWithRolesAsync(sortOrder);
        
        return View(userListDtos);
    }

    // GET: Admin/Users/Create
    public async Task<IActionResult> Create()
    {
        var roles = await _userService.GetAllAvailableRolesAsync();

        var model = new UserCreateDto
        {
            AvailableRoles = roles
        };

        return View(model);
    }

    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateDto model)
    {
        model.AvailableRoles = await _userService.GetAllAvailableRolesAsync();

        if (!ModelState.IsValid) return View(model);
        
        var (result, user) = await _userService.CreateUserAsync(model);

        if (result.Succeeded) return RedirectToAction(nameof(Index));
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET: Admin/Users/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null) return NotFound();
        
        var model = await _userService.GetUserForEditAsync(id);

        if (model == null) return NotFound();
        
        return View(model);
    }

    // POST: Admin/Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditDto model)
    {
        if (id != model.Id) return NotFound();

        model.AvailableRoles = await _userService.GetAllAvailableRolesAsync();

        if (!ModelState.IsValid) return View(model);

        var result = await _userService.UpdateUserAsync(model);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User has been updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET: Admin/Users/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null) return NotFound();
        
        var model = await _userService.GetUserForDeleteAsync(id);

        if (model == null) return NotFound();
        
        return View(model);
    }

    // POST: Admin/Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (result.Succeeded) return RedirectToAction(nameof(Index));

        string errorMessage = "Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description));
        TempData["ErrorMessage"] = errorMessage;
        
        return RedirectToAction(nameof(Index));
    }
    
    // Helper method to determine sort icon CSS class
    private string GetSortIcon(string? currentSort, string columnName)
    {
        if (string.IsNullOrEmpty(currentSort) && columnName == "lastname")
            return "bi-arrow-up"; // Default sort by last name ascending
            
        if (currentSort == columnName)
            return "bi-arrow-up"; // Ascending
            
        if (currentSort == $"{columnName}_desc")
            return "bi-arrow-down"; // Descending
            
        return "bi-arrow-down-up"; // Not sorted
    }
}