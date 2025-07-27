using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionEntityAndRefactorModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalMedications_VisitUpdates_VisitUpdateId",
                table: "AnimalMedications");

            migrationBuilder.DropIndex(
                name: "IX_AnimalMedications_VisitUpdateId",
                table: "AnimalMedications");

            migrationBuilder.DropColumn(
                name: "VisitUpdateId",
                table: "AnimalMedications");

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dosage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    VisitUpdateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_VisitUpdates_VisitUpdateId",
                        column: x => x.VisitUpdateId,
                        principalTable: "VisitUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicationId",
                table: "Prescriptions",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_VisitUpdateId",
                table: "Prescriptions",
                column: "VisitUpdateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.AddColumn<int>(
                name: "VisitUpdateId",
                table: "AnimalMedications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMedications_VisitUpdateId",
                table: "AnimalMedications",
                column: "VisitUpdateId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalMedications_VisitUpdates_VisitUpdateId",
                table: "AnimalMedications",
                column: "VisitUpdateId",
                principalTable: "VisitUpdates",
                principalColumn: "Id");
        }
    }
}
