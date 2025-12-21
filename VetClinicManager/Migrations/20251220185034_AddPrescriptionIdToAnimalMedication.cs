using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionIdToAnimalMedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrescriptionId",
                table: "AnimalMedications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMedications_PrescriptionId",
                table: "AnimalMedications",
                column: "PrescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalMedications_Prescriptions_PrescriptionId",
                table: "AnimalMedications",
                column: "PrescriptionId",
                principalTable: "Prescriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalMedications_Prescriptions_PrescriptionId",
                table: "AnimalMedications");

            migrationBuilder.DropIndex(
                name: "IX_AnimalMedications_PrescriptionId",
                table: "AnimalMedications");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "AnimalMedications");
        }
    }
}
