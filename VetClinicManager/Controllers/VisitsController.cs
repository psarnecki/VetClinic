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
    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);
        
        if (currentUserId == null) return Unauthorized();

        if (User.IsInRole("Client"))
        {
            var visits = await _visitService.GetVisitsForOwnerAsync(currentUserId);
            return View("IndexUser", visits);
        }

        var vetId = User.IsInRole("Vet") ? currentUserId : null;
        var staffVisits = await _visitService.GetVisitsForStaffAsync(vetId);
        var viewName = User.IsInRole("Vet") ? "IndexVet" : "IndexReceptionist";
        
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

        if (User.IsInRole("Vet") && staffVisit.AssignedVet?.Id != currentUserId)
        {
            TempData["ErrorMessage"] = "You can only access visits assigned to you.";
            return Forbid();
        }

        var viewName = User.IsInRole("Vet") ? "DetailsVet" : "DetailsReceptionist";

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

        var visitEditDto = await _visitService.GetForEditAsync(id, currentUserId, User.IsInRole("Vet"));
        
        if (visitEditDto == null) return NotFound();
        
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

            var originalVisit = await _visitService.GetForEditAsync(id, currentUserId, User.IsInRole("Vet"));
            if (originalVisit != null)
            {
                visitEditDto.Animal = originalVisit.Animal;
            }
            
            return View(visitEditDto);
        }

        var success = await _visitService.UpdateVisitAsync(id, visitEditDto, currentUserId, User.IsInRole("Vet"));
        
        if (!success)
        {
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
}