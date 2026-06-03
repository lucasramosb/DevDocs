using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevDocs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SourceFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    GitHubSha = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GitHubBlobUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDocumentationFile = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTestFile = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SourceFiles_ProjectId_Path",
                table: "SourceFiles",
                columns: new[] { "ProjectId", "Path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourceFiles");
        }
    }
}
