using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.DTOs.AnimalMedications;
using VetClinicManager.Services;

namespace VetClinicManager.Controllers;

[Authorize(Roles = "Admin,Vet")]
public class AnimalMedicationsController : Controller
{
    private readonly IMedicationService _medicationService;
    private readonly IAnimalMedicationService _animalMedicationService;

    public AnimalMedicationsController(IMedicationService medicationService, IAnimalMedicationService animalMedicationService)
    {
        _medicationService = medicationService;
        _animalMedicationService = animalMedicationService;
    }

    // GET: AnimalMedications/Create?animalId=5
    [HttpGet]
    public async Task<IActionResult> Create(int animalId)
    {
        var createDto = await _animalMedicationService.GetForCreateAsync(animalId);
        
        if (createDto == null) return NotFound("Animal not found.");
        
        var medications = await _medicationService.GetMedicationsForSelectListAsync();
        createDto.Medications = new SelectList(medications, "Id", "Name");
        
        return View(createDto);
    }

    // POST: AnimalMedications/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AnimalMedicationCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            var medications = await _medicationService.GetMedicationsForSelectListAsync();
            createDto.Medications = new SelectList(medications, "Id", "Name", createDto.MedicationId);
            
            return View(createDto);
        }

        await _animalMedicationService.CreateAnimalMedicationAsync(createDto);
    
        TempData["SuccessMessage"] = "Medication has been added to the chart.";
    
        if (createDto.HealthRecordId > 0)
        {
            return RedirectToHealthRecord(createDto.HealthRecordId);
        }

        return RedirectToAction("Details", "Animals", new { id = createDto.AnimalId });
    }

    // GET: AnimalMedications/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var editDto = await _animalMedicationService.GetForEditAsync(id);
        
        if (editDto == null) return NotFound();
        
        var medications = await _medicationService.GetMedicationsForSelectListAsync();
        editDto.Medications = new SelectList(medications, "Id", "Name", editDto.MedicationId);
        
        return View(editDto);
    }

    // POST: AnimalMedications/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AnimalMedicationEditDto editDto)
    {
        if (id != editDto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            var medications = await _medicationService.GetMedicationsForSelectListAsync();
            editDto.Medications = new SelectList(medications, "Id", "Name", editDto.MedicationId);
            
            return View(editDto);
        }

        var success = await _animalMedicationService.UpdateAnimalMedicationAsync(editDto);
        
        if (!success) return NotFound();
        
        TempData["SuccessMessage"] = "Medication entry has been updated.";
        
        return RedirectToHealthRecord(editDto.HealthRecordId);
    }

    // GET: AnimalMedications/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var deleteDto = await _animalMedicationService.GetForDeleteAsync(id);
        
        if (deleteDto == null) return NotFound();
        
        return View(deleteDto);
    }

    // POST: AnimalMedications/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(AnimalMedicationDeleteDto dto)
    {
        await _animalMedicationService.DeleteAnimalMedicationAsync(dto.Id);
        TempData["SuccessMessage"] = "Medication entry has been deleted.";
        
        return RedirectToHealthRecord(dto.HealthRecordId);
    }

    private IActionResult RedirectToHealthRecord(int healthRecordId)
    {
        return RedirectToAction("Details", "HealthRecords", new { id = healthRecordId });
    }
}