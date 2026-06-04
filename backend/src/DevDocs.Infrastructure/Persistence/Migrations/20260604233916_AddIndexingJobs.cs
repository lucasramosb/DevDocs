using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevDocs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexingJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndexingJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalFilesFound = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalFilesMapped = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalFilesIgnored = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexingJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexingJobs_ProjectId",
                table: "IndexingJobs",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexingJobs");
        }
    }
}
