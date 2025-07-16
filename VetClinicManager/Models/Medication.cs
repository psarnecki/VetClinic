using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Models;

public class Medication
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Name { get; set; }

    public ICollection<AnimalMedication> AnimalMedications { get; set; } = new List<AnimalMedication>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}