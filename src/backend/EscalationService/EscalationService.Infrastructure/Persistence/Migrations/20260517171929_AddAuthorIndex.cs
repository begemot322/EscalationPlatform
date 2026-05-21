using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EscalationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_escalations_author_id",
                table: "escalations",
                column: "author_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_escalations_author_id",
                table: "escalations");
        }
    }
}
