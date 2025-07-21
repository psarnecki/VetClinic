using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Models;
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
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser == null) return Unauthorized();

        if (User.IsInRole("Client"))
        {
            var visits = await _visitService.GetVisitsForOwnerAsync(currentUser.Id);
            return View("IndexUser", visits);
        }

        var vetId = User.IsInRole("Vet") ? currentUser.Id : null;
        var staffVisits = await _visitService.GetVisitsForStaffAsync(vetId);
        var viewName = User.IsInRole("Vet") ? "IndexVet" : "IndexReceptionist";
        
        return View(viewName, staffVisits);
    }

    // GET: Visits/Details/5
    [Authorize(Roles = "Admin,Receptionist,Vet,Client")] 
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null) return Unauthorized();

        if (User.IsInRole("Client"))
        {
            var visit = await _visitService.GetDetailsForOwnerAsync(id, currentUser.Id);
            if (visit == null) return NotFound();
            
            return View("DetailsUser", visit);
        }

        var staffVisit = await _visitService.GetDetailsForStaffAsync(id);

        if (staffVisit == null) return NotFound();

        if (User.IsInRole("Vet") && staffVisit.AssignedVet?.Id != currentUser.Id)
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
        var createDto = new VisitCreateDto
        {
            Animals = await _visitService.GetAnimalsSelectListAsync(),
            Vets = await _visitService.GetVetsSelectListAsync(),
            Statuses = _visitService.GetStatusesSelectList(),
            Priorities = _visitService.GetPrioritiesSelectList()
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
            createVisitDto.Animals = await _visitService.GetAnimalsSelectListAsync(createVisitDto.AnimalId);
            createVisitDto.Vets = await _visitService.GetVetsSelectListAsync(createVisitDto.AssignedVetId);
            createVisitDto.Statuses = _visitService.GetStatusesSelectList(createVisitDto.Status);
            createVisitDto.Priorities = _visitService.GetPrioritiesSelectList(createVisitDto.Priority);
            
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
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser == null) return Unauthorized();

        var visitEditDto = await _visitService.GetForEditAsync(id, currentUser.Id, User.IsInRole("Vet"));
        
        if (visitEditDto == null) return NotFound();

        visitEditDto.Vets = await _visitService.GetVetsSelectListAsync(visitEditDto.AssignedVetId);
        visitEditDto.Statuses = _visitService.GetStatusesSelectList(visitEditDto.Status);
        visitEditDto.Priorities = _visitService.GetPrioritiesSelectList(visitEditDto.Priority);

        return View(visitEditDto);
    }

    // POST: Visits/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist,Vet")]
    public async Task<IActionResult> Edit(int id, VisitEditDto visitEditDto)
    {
        if (id != visitEditDto.Id) return NotFound();
        
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            visitEditDto.Vets = await _visitService.GetVetsSelectListAsync(visitEditDto.AssignedVetId);
            visitEditDto.Statuses = _visitService.GetStatusesSelectList(visitEditDto.Status);
            visitEditDto.Priorities = _visitService.GetPrioritiesSelectList(visitEditDto.Priority);

            var originalVisit = await _visitService.GetForEditAsync(id, currentUser.Id, User.IsInRole("Vet"));
            if (originalVisit != null)
            {
                visitEditDto.Animal = originalVisit.Animal;
            }
            
            return View(visitEditDto);
        }

        var success = await _visitService.UpdateVisitAsync(id, visitEditDto, currentUser.Id, User.IsInRole("Vet"));
        
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
}