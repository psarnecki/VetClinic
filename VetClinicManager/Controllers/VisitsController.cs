using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly IVisitService _visitService;
    private readonly UserManager<User> _userManager;

    public VisitsController(IVisitService visitService, UserManager<User> userManager)
    {
        _visitService = visitService;
        _userManager = userManager;
    }

    // GET: Visits
    [Authorize(Roles = "Admin,Receptionist,Vet,Client")] 
    public async Task<IActionResult> Index(string sortOrder)
    {
        var currentUserId = _userManager.GetUserId(User);
        
        if (currentUserId == null) return Unauthorized();

        ViewData["CurrentSort"] = sortOrder;
        ViewData["TitleSortParm"] = sortOrder == "title" ? "title_desc" : "title";
        ViewData["ScheduledSortParm"] = sortOrder == "scheduled" ? "scheduled_desc" : "scheduled";
        ViewData["StatusSortParm"] = sortOrder == "status" ? "status_desc" : "status";
        ViewData["PrioritySortParm"] = sortOrder == "priority" ? "priority_desc" : "priority";
        ViewData["AnimalSortParm"] = sortOrder == "animal" ? "animal_desc" : "animal";
        ViewData["OwnerSortParm"] = sortOrder == "owner" ? "owner_desc" : "owner";
        ViewData["VetSortParm"] = sortOrder == "vet" ? "vet_desc" : "vet";
        
        // Helper for icon CSS classes
        ViewData["TitleIcon"] = GetSortIcon(sortOrder, "title");
        ViewData["ScheduledIcon"] = GetSortIcon(sortOrder, "scheduled");
        ViewData["StatusIcon"] = GetSortIcon(sortOrder, "status");
        ViewData["PriorityIcon"] = GetSortIcon(sortOrder, "priority");
        ViewData["AnimalIcon"] = GetSortIcon(sortOrder, "animal");
        ViewData["OwnerIcon"] = GetSortIcon(sortOrder, "owner");
        ViewData["VetIcon"] = GetSortIcon(sortOrder, "vet");

        if (User.IsInRole("Client"))
        {
            var visits = await _visitService.GetVisitsForOwnerAsync(currentUserId, sortOrder);
            return View("IndexUser", visits);
        }

        var vetId = User.IsInRole("Vet") && !User.IsInRole("Admin") ? currentUserId : null;
        var staffVisits = await _visitService.GetVisitsForStaffAsync(vetId, sortOrder);
        var viewName = (User.IsInRole("Vet") && !User.IsInRole("Admin")) ? "IndexVet" : "IndexReceptionist";
        
        return View(viewName, staffVisits);
    }

    // GET: Visits/Details/5
    [Authorize(Roles = "Admin,Receptionist,Vet,Client")] 
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (currentUserId == null) return Unauthorized();

        if (User.IsInRole("Client"))
        {
            var visit = await _visitService.GetDetailsForOwnerAsync(id, currentUserId);
            if (visit == null) return NotFound();
            
            return View("DetailsUser", visit);
        }

        var staffVisit = await _visitService.GetDetailsForStaffAsync(id);

        if (staffVisit == null) return NotFound();

        if (User.IsInRole("Vet") && !User.IsInRole("Admin") && staffVisit.AssignedVet?.Id != currentUserId)
        {
            TempData["ErrorMessage"] = "Access denied. You can only access visits assigned to you.";
            return RedirectToAction(nameof(Index));
        }

        var viewName = (User.IsInRole("Vet") || User.IsInRole("Admin")) ? "DetailsVet" : "DetailsReceptionist";

        return View(viewName, staffVisit);
    }

    // GET: Visits/Create
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create()
    {
        var animals = await _visitService.GetAnimalsForSelectListAsync();
        var vets = await _visitService.GetVetsForSelectListAsync();
        
        var createDto = new VisitCreateDto
        {
            Animals = new SelectList(animals.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Name} ({a.Species})"
            }), "Value", "Text"),
            Vets = new SelectList(vets.Select(v => new SelectListItem
            {
                Value = v.Id,
                Text = $"{v.FirstName} {v.LastName}"
            }), "Value", "Text"),
            Statuses = GetEnumSelectList<VisitStatus>(),
            Priorities = GetEnumSelectList<VisitPriority>()
        };
        
        return View(createDto);
    }

    // POST: Visits/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create(VisitCreateDto createVisitDto)
    {
        if (!ModelState.IsValid)
        {
            var animals = await _visitService.GetAnimalsForSelectListAsync();
            var vets = await _visitService.GetVetsForSelectListAsync();

            createVisitDto.Animals = new SelectList(animals.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Name} ({a.Species})"
            }), "Value", "Text");
            createVisitDto.Vets = new SelectList(vets.Select(v => new SelectListItem
            {
                Value = v.Id,
                Text = $"{v.FirstName} {v.LastName}"
            }), "Value", "Text", createVisitDto.AssignedVetId);
            createVisitDto.Statuses = GetEnumSelectList<VisitStatus>(createVisitDto.Status);
            createVisitDto.Priorities = GetEnumSelectList<VisitPriority>(createVisitDto.Priority);
            
            return View(createVisitDto);
        }

        var newVisitId = await _visitService.CreateVisitAsync(createVisitDto);
        TempData["SuccessMessage"] = "Visit has been created successfully.";
        
        return RedirectToAction(nameof(Details), new { id = newVisitId });
    }

    // GET: Visits/Edit/5
    [Authorize(Roles = "Admin,Receptionist,Vet")]
    public async Task<IActionResult> Edit(int id)
    {
        var currentUserId = _userManager.GetUserId(User);
        
        if (currentUserId == null) return Unauthorized();
        
        bool isVetOnly = User.IsInRole("Vet") && !User.IsInRole("Admin");

        var visitEditDto = await _visitService.GetForEditAsync(id, currentUserId, isVetOnly);
        
        if (visitEditDto == null)
        {
            if (isVetOnly)
            {
                TempData["ErrorMessage"] = "Access denied. You can only edit visits assigned to you.";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        
        var vets = await _visitService.GetVetsForSelectListAsync();

        visitEditDto.Vets = new SelectList(vets.Select(v => new SelectListItem
        {
            Value = v.Id,
            Text = $"{v.FirstName} {v.LastName}"
        }), "Value", "Text", visitEditDto.AssignedVetId);
        visitEditDto.Statuses = GetEnumSelectList<VisitStatus>(visitEditDto.Status);
        visitEditDto.Priorities = GetEnumSelectList<VisitPriority>(visitEditDto.Priority);

        return View(visitEditDto);
    }

    // POST: Visits/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist,Vet")]
    public async Task<IActionResult> Edit(int id, VisitEditDto visitEditDto)
    {
        if (id != visitEditDto.Id) return NotFound();
        
        var currentUserId = _userManager.GetUserId(User);
        
        if (currentUserId == null) return Unauthorized();
        
        bool isVetOnly = User.IsInRole("Vet") && !User.IsInRole("Admin");

        if (!ModelState.IsValid)
        {
            var vets = await _visitService.GetVetsForSelectListAsync();
            
            visitEditDto.Vets = new SelectList(vets.Select(v => new SelectListItem
            {
                Value = v.Id,
                Text = $"{v.FirstName} {v.LastName}"
            }), "Value", "Text", visitEditDto.AssignedVetId);
            visitEditDto.Statuses = GetEnumSelectList<VisitStatus>(visitEditDto.Status);
            visitEditDto.Priorities = GetEnumSelectList<VisitPriority>(visitEditDto.Priority);

            var originalVisit = await _visitService.GetForEditAsync(id, currentUserId, isVetOnly);
            if (originalVisit != null)
            {
                visitEditDto.Animal = originalVisit.Animal;
            }
            
            return View(visitEditDto);
        }

        var success = await _visitService.UpdateVisitAsync(id, visitEditDto, currentUserId, isVetOnly);
        
        if (!success)
        {
            if (isVetOnly)
            {
                TempData["ErrorMessage"] = "Access denied. You can only update visits assigned to you.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Could not update the visit. Please try again.";
            return View(visitEditDto);
        }

        TempData["SuccessMessage"] = "Visit has been updated successfully.";
        
        return RedirectToAction(nameof(Details), new { id = visitEditDto.Id });
    }

    // GET: Visits/Delete/5
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _visitService.GetForDeleteAsync(id);

        if (dto == null) return NotFound();

        return View(dto);
    }

    // POST: Visits/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _visitService.DeleteVisitAsync(id);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Visit has been deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not find the visit to delete.";
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GenerateVisitReport(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        
        var roles = await _userManager.GetRolesAsync(user);

        try
        {
            var result = await _visitService.GeneratePdfReportAsync(id, user.Id, roles);
            
            if (result == null) return NotFound();
        
            return File(result.Value.FileContents, "application/pdf", result.Value.FileName);
        }
        catch(UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    
    private SelectList GetEnumSelectList<TEnum>(object? selectedValue = null)
        where TEnum : struct, Enum
    {
        var items = Enum.GetValues<TEnum>().Select(e =>
        {
            var display = typeof(TEnum).GetField(e.ToString())?
                .GetCustomAttribute<DisplayAttribute>();
            return new SelectListItem
            {
                Value = e.ToString(),
                Text = display?.GetName() ?? e.ToString()
            };
        });

        return new SelectList(items, "Value", "Text", selectedValue);
    }
    
    // Helper method to determine sort icon CSS class
    private string GetSortIcon(string? currentSort, string columnName)
    {
        if (string.IsNullOrEmpty(currentSort) && columnName == "scheduled")
            return "bi-arrow-down"; // Default sort by scheduled date descending
            
        if (currentSort == columnName)
            return "bi-arrow-up"; // Ascending
            
        if (currentSort == $"{columnName}_desc")
            return "bi-arrow-down"; // Descending
            
        return "bi-arrow-down-up"; // Not sorted
    }
}