using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_AssignedVetId",
                table: "Visits");

            migrationBuilder.RenameIndex(
                name: "IX_HealthRecords_AnimalId",
                table: "HealthRecords",
                newName: "Idx_HealthRecords_AnimalId");

            migrationBuilder.RenameIndex(
                name: "IX_Animals_OwnerId",
                table: "Animals",
                newName: "Idx_Animals_OwnerId");

            migrationBuilder.CreateIndex(
                name: "Idx_Visits_AssignedVetId_Status",
                table: "Visits",
                columns: new[] { "AssignedVetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "Idx_Visits_ScheduledAt",
                table: "Visits",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "Idx_Visits_Status",
                table: "Visits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "Idx_Animals_Name",
                table: "Animals",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Idx_Visits_AssignedVetId_Status",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "Idx_Visits_ScheduledAt",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "Idx_Visits_Status",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "Idx_Animals_Name",
                table: "Animals");

            migrationBuilder.RenameIndex(
                name: "Idx_HealthRecords_AnimalId",
                table: "HealthRecords",
                newName: "IX_HealthRecords_AnimalId");

            migrationBuilder.RenameIndex(
                name: "Idx_Animals_OwnerId",
                table: "Animals",
                newName: "IX_Animals_OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssignedVetId",
                table: "Visits",
                column: "AssignedVetId");
        }
    }
}
