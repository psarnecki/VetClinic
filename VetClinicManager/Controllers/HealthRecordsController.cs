using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Services;

namespace VetClinicManager.Controllers;

[Authorize]
public class HealthRecordsController : Controller
{
    private readonly IHealthRecordService _healthRecordService;

    public HealthRecordsController(IHealthRecordService healthRecordService)
    {
        _healthRecordService = healthRecordService;
    }

    // GET: HealthRecords/Details/5
    [Authorize(Roles = "Admin,Vet,Receptionist,Client")]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _healthRecordService.GetDetailsAsync(id);
    
        if (model == null) return NotFound();
    
        return View(model);
    }
    
    // GET: HealthRecords/Create?animalId=5
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Create(int animalId)
    {
        var model = await _healthRecordService.PrepareCreateDtoAsync(animalId);
    
        if (model == null)
        {
            TempData["ErrorMessage"] = "A health record already exists for this animal or the animal was not found.";
            return RedirectToAction("Details", "Animals", new { id = animalId });
        }
    
        return View(model);
    }

    // POST: HealthRecords/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Create(HealthRecordCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            var freshDto = await _healthRecordService.PrepareCreateDtoAsync(createDto.AnimalId);
            if (freshDto != null) createDto.AnimalName = freshDto.AnimalName;
            
            return View(createDto);
        }

        var newId = await _healthRecordService.CreateAsync(createDto);
        TempData["SuccessMessage"] = "Health record was created successfully.";
        
        return RedirectToAction(nameof(Details), new { id = newId });
    }

    // GET: HealthRecords/Edit/5
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _healthRecordService.GetForEditAsync(id);
        
        if (model == null) return NotFound();
    
        return View(model);
    }

    // POST: HealthRecords/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Edit(int id, HealthRecordEditDto editDto)
    {
        if (id != editDto.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var success = await _healthRecordService.UpdateAsync(editDto);
            if (success)
            {
                TempData["SuccessMessage"] = "Health record was updated successfully.";
                return RedirectToAction(nameof(Details), new { id = editDto.Id });
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. The record may have been deleted.");
            }
        }
    
        return View(editDto);
    }

    // GET: HealthRecords/Delete/5
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _healthRecordService.GetForDeleteAsync(id);
        
        if (model == null) return NotFound();
    
        return View(model);
    }

    // POST: HealthRecords/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _healthRecordService.DeleteAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = "Health record was deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not delete the health record.";
        }
    
        return RedirectToAction("Index", "Animals"); 
    }
}