using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.DTOs.VisitUpdates;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Controllers;

[Authorize(Roles = "Admin,Vet")]
public class VisitUpdatesController : Controller
{
    private readonly IVisitUpdateService _visitUpdateService;
    private readonly IMedicationService _medicationService;
    private readonly UserManager<User> _userManager;
    
    public VisitUpdatesController(IVisitUpdateService visitUpdateService, IMedicationService medicationService, UserManager<User> userManager)
    {
        _visitUpdateService = visitUpdateService;
        _medicationService = medicationService;
        _userManager = userManager;
    }

    // GET: VisitUpdates/Create?visitId=5
    public async Task<IActionResult> Create(int visitId)
    {
        var createDto = await _visitUpdateService.GetForCreateAsync(visitId);
        
        if (createDto == null) return NotFound("The parent visit for this update could not be found.");
        
        var medications = await _medicationService.GetMedicationsForSelectListAsync();
        createDto.Medications = new SelectList(medications, "Id", "Name");
        
        return View(createDto);
    }

    // POST: VisitUpdates/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VisitUpdateCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            var medications = await _medicationService.GetMedicationsForSelectListAsync();
            createDto.Medications = new SelectList(medications, "Id", "Name");
            
            return View(createDto);
        }
        
        var currentUserId = _userManager.GetUserId(User)!;
        var visitId = await _visitUpdateService.CreateVisitUpdateAsync(createDto, currentUserId);
        
        TempData["SuccessMessage"] = "Visit update has been added successfully.";
        
        return RedirectToAction("Details", "Visits", new { id = visitId });
    }

    // GET: VisitUpdates/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");
        
        var editDto = await _visitUpdateService.GetForEditAsync(id, currentUserId, isAdmin);
        
        if (editDto == null) return NotFound();
        
        var medications = await _medicationService.GetMedicationsForSelectListAsync();
        editDto.Medications = new SelectList(medications, "Id", "Name");

        return View(editDto);
    }

    // POST: VisitUpdates/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VisitUpdateEditDto editDto)
    {
        if (id != editDto.Id) return BadRequest();
        
        if (!ModelState.IsValid)
        {
            var medications = await _medicationService.GetMedicationsForSelectListAsync();
            editDto.Medications = new SelectList(medications, "Id", "Name");
            
            return View(editDto);
        }

        try
        {
            var currentUserId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");
            
            var visitId = await _visitUpdateService.UpdateVisitUpdateAsync(editDto, currentUserId, isAdmin);
            
            TempData["SuccessMessage"] = "The update has been saved successfully.";
            return RedirectToAction("Details", "Visits", new { id = visitId });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // GET: VisitUpdates/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");
        
        var deleteDto = await _visitUpdateService.GetForDeleteAsync(id, currentUserId, isAdmin);
        
        if (deleteDto == null) return NotFound();

        return View(deleteDto);
    }

    // POST: VisitUpdates/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(VisitUpdateDeleteDto dto)
    {
        try
        {
            var currentUserId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");
            
            var visitId = await _visitUpdateService.DeleteVisitUpdateAsync(dto.Id, currentUserId, isAdmin);
            
            TempData["SuccessMessage"] = "The update has been deleted successfully.";
            return RedirectToAction("Details", "Visits", new { id = visitId });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}