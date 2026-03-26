using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalService.Migrations
{
    /// <inheritdoc />
    public partial class m2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Proposals",
                newName: "ExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_TaskId",
                table: "Proposals",
                column: "TaskId",
                unique: true,
                filter: "\"Status\" = 'ACCEPTED'");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_TaskId_ExecutorId",
                table: "Proposals",
                columns: new[] { "TaskId", "ExecutorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proposals_TaskId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_TaskId_ExecutorId",
                table: "Proposals");

            migrationBuilder.RenameColumn(
                name: "ExecutorId",
                table: "Proposals",
                newName: "UserId");
        }
    }
}
