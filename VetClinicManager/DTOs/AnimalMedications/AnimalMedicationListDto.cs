using System.ComponentModel.DataAnnotations;
using VetClinicManager.DTOs.Shared;

namespace VetClinicManager.DTOs.AnimalMedications;

public class AnimalMedicationListDto
{
    public int Id { get; set; }
    
    [Display(Name = "Medication")]
    public MedicationBriefDto MedicationName { get; set; } = new();

    [Display(Name = "Start Date")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime StartDate { get; set; }

    [Display(Name = "End Date")]
    [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "In progress")]
    public DateTime? EndDate { get; set; }
}