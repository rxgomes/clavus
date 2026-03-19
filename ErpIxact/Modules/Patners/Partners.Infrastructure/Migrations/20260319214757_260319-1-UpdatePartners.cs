using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patners.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _2603191UpdatePartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_partners_doc_number",
                table: "partners",
                newName: "ix_partners_doc_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_partners_doc_number",
                table: "partners",
                newName: "IX_partners_doc_number");
        }
    }
}
