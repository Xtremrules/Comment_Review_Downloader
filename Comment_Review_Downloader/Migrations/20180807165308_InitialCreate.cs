using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Comment_Review_Downloader.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amazon",
                columns: table => new
                {
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(nullable: true),
                    NOC = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amazon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YouTube",
                columns: table => new
                {
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    YouTube_Id = table.Column<string>(nullable: true),
                    NOC = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTube", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amazon");

            migrationBuilder.DropTable(
                name: "YouTube");
        }
    }
}
