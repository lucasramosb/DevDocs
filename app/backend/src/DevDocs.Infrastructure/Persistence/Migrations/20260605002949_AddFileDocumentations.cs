using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevDocs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileDocumentations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileDocumentations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceFileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Generator = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDocumentations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileDocumentations_ProjectId",
                table: "FileDocumentations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDocumentations_SourceFileId",
                table: "FileDocumentations",
                column: "SourceFileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDocumentations");
        }
    }
}
