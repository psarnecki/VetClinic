using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Controllers;

public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly UserManager<User> _userManager;

    public HomeController(IDashboardService dashboardService, UserManager<User> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    }

    // GET: Home
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View("IndexAnonymous");
        }

        if (User.IsInRole("Client"))
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var dashboardData = await _dashboardService.GetDashboardDataForClientAsync(userId);
            return View("IndexClient", dashboardData);
        }
        
        string? vetId = null;
        
        if (User.IsInRole("Vet") && !User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
        {
            vetId = _userManager.GetUserId(User);
            if (vetId == null) return Unauthorized();
        }
        
        var staffDashboardData = await _dashboardService.GetDashboardDataForStaffAsync(vetId);
        return View("IndexStaff", staffDashboardData);
    }

    // GET: Home/Privacy
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Home/Error
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}