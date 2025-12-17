using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.Prescriptions;

public class PrescriptionCreateDto
{
    [Required(ErrorMessage = "A medication must be selected.")]
    [Display(Name = "Medication")]
    public int? MedicationId { get; set; }

    [Required(ErrorMessage = "Dosage information is required.")]
    [MaxLength(250)]
    [Display(Name = "Dosage")]
    public string Dosage { get; set; }
}