using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Controllers;

[Authorize]
public class AnimalsController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IAnimalService _animalService;
    
    public AnimalsController(UserManager<User> userManager, IAnimalService animalService)
    {
        _animalService = animalService ?? throw new ArgumentNullException(nameof(animalService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    // GET: Animals
    [Authorize(Roles = "Admin,Receptionist,Vet,Client")]
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Client"))
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (currentUserId == null) return Unauthorized();
                
            var animals = await _animalService.GetAnimalsForOwnerAsync(currentUserId);
            return View("IndexUser", animals);
        }
        else
        {
            var animals = await _animalService.GetAnimalsForStaffAsync();
                
            return View("IndexVetRec", animals);
        }
    }

    // GET: Animals/Details/5
    [Authorize(Roles = "Admin,Receptionist,Vet,Client")]
    public async Task<IActionResult> Details(int id)
    {
        if (User.IsInRole("Client"))
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (currentUserId == null) return Unauthorized();

            var model = await _animalService.GetAnimalDetailsForOwnerAsync(id, currentUserId);
            
            if (model == null) return NotFound();
            
            return View("DetailsUser", model);
        }
        else
        {
            var model = await _animalService.GetAnimalDetailsForStaffAsync(id);
            
            if (model == null) return NotFound();
            
            return View("DetailsVetRec", model);
        }
    }

    // GET: Animals/Create
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create()
    {
        var model = new AnimalCreateDto
        {
            Owners = await _animalService.GetOwnersForSelectListAsync(),
            Genders = _animalService.GetGendersSelectList()
        };
        
        return View(model);
    }

    // POST: Animals/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create(AnimalCreateDto animalCreateDto)
    {
        if (!ModelState.IsValid)
        {
            animalCreateDto.Owners = await _animalService.GetOwnersForSelectListAsync(animalCreateDto.OwnerId);
            animalCreateDto.Genders = _animalService.GetGendersSelectList(animalCreateDto.Gender);
            return View(animalCreateDto);
        }
    
        var newAnimalId = await _animalService.CreateAnimalAsync(animalCreateDto);
        TempData["SuccessMessage"] = "Animal created successfully.";
        
        return RedirectToAction(nameof(Details), new { id = newAnimalId });
    }

    // GET: Animals/Edit/5
    [Authorize(Roles = "Admin,Receptionist,Vet")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        
        var model = await _animalService.GetAnimalForEditAsync(id.Value);
        
        if (model == null) return NotFound();
        
        model.Owners = await _animalService.GetOwnersForSelectListAsync(model.OwnerId);
        model.Genders = _animalService.GetGendersSelectList(model.Gender);

        return View(model);
    }

    // POST: Animals/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist,Vet")]
    public async Task<IActionResult> Edit(int id, AnimalEditDto animalEditDto)
    {
        if (id != animalEditDto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            animalEditDto.Owners = await _animalService.GetOwnersForSelectListAsync(animalEditDto.OwnerId);
            animalEditDto.Genders = _animalService.GetGendersSelectList(animalEditDto.Gender);
            return View(animalEditDto);
        }

        var success = await _animalService.UpdateAnimalAsync(animalEditDto);

        if (success)
        {
            TempData["SuccessMessage"] = "Animal data updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Unable to save changes. The animal may have been deleted by another user.");
        animalEditDto.Owners = await _animalService.GetOwnersForSelectListAsync(animalEditDto.OwnerId);
        animalEditDto.Genders = _animalService.GetGendersSelectList(animalEditDto.Gender);
        
        return View(animalEditDto);
    }
    
    // GET: Animals/Delete/5
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        
        var model = await _animalService.GetAnimalForDeleteAsync(id.Value);
        
        if (model == null) return NotFound();
        
        return View(model);
    }

    // POST: Animals/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _animalService.DeleteAnimalAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = "Animal was deleted.";
        }
        else
        {
            TempData["ErrorMessage"] = "This animal cannot be deleted because it has associated visits or other dependencies.";
        }
    
        return RedirectToAction(nameof(Index));
    }
}