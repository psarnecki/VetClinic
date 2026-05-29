using FluentAssertions;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class VisitMapperTests
{
    private readonly VisitMapper _mapper = new();

    [Test]
    public void ToListVetRecDto_ShouldMapVisitWithAnimalOwnerAndVet()
    {
        var owner = new User
        {
            Id = "o1",
            UserName = "owner@test.local",
            Email = "owner@test.local",
            FirstName = "Eve",
            LastName = "Miller"
        };

        var vet = new User
        {
            Id = "v1",
            UserName = "vet@test.local",
            Email = "vet@test.local",
            FirstName = "Anne",
            LastName = "Taylor"
        };

        var animal = new Animal
        {
            Id = 7,
            Name = "Whiskers",
            Species = "Cat",
            BodyWeight = 5f,
            Gender = Gender.Female,
            Owner = owner
        };

        var visit = new Visit
        {
            Id = 100,
            Title = "Control visit",
            CreatedAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc),
            ScheduledAt = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Urgent,
            Animal = animal,
            AnimalId = animal.Id,
            AssignedVet = vet,
            AssignedVetId = vet.Id
        };

        var dto = _mapper.ToListVetRecDto(visit);

        dto.Id.Should().Be(100);
        dto.Title.Should().Be("Control visit");
        dto.Status.Should().Be(VisitStatus.Scheduled);
        dto.Priority.Should().Be(VisitPriority.Urgent);
        dto.Animal.Name.Should().Be("Whiskers");
        dto.AssignedVet.Should().NotBeNull();
        dto.AssignedVet!.FirstName.Should().Be("Anne");
        dto.Owner.Should().NotBeNull();
        dto.Owner!.FirstName.Should().Be("Eve");
    }

    [Test]
    public void ToEntity_WhenMappingFromCreateDto_ShouldMapVisitFields()
    {
        var createDto = new VisitCreateDto
        {
            Title = "Vaccination",
            Description = "Annual",
            ScheduledAt = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = 3,
            AssignedVetId = "vet-1"
        };

        var entity = _mapper.ToEntity(createDto);

        entity.Title.Should().Be("Vaccination");
        entity.Description.Should().Be("Annual");
        entity.Status.Should().Be(VisitStatus.Scheduled);
        entity.Priority.Should().Be(VisitPriority.Normal);
        entity.AnimalId.Should().Be(3);
        entity.AssignedVetId.Should().Be("vet-1");
    }

    [Test]
    public void ToListVetRecDto_WhenAssignedVetIsNull_ShouldSetAssignedVetToNull()
    {
        var owner = new User
        {
            Id = "o1",
            UserName = "o@test.local",
            Email = "o@test.local",
            FirstName = "O",
            LastName = "W"
        };

        var animal = new Animal
        {
            Id = 1,
            Name = "Pet",
            BodyWeight = 2f,
            Gender = Gender.Female,
            Owner = owner
        };

        var visit = new Visit
        {
            Id = 50,
            Title = "Triage",
            CreatedAt = DateTime.UtcNow,
            ScheduledAt = DateTime.UtcNow.AddHours(2),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            Animal = animal,
            AnimalId = animal.Id,
            AssignedVet = null,
            AssignedVetId = null
        };

        var dto = _mapper.ToListVetRecDto(visit);

        dto.AssignedVet.Should().BeNull();
    }
}