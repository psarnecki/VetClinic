using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.Prescriptions;

public class PrescriptionEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A medication must be selected.")]
    [Display(Name = "Medication")]
    public int? MedicationId { get; set; }

    [Required(ErrorMessage = "Dosage information is required.")]
    [MaxLength(250)]
    [Display(Name = "Dosage")]
    public string Dosage { get; set; }
}