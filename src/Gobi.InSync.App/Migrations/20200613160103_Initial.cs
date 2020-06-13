using Microsoft.EntityFrameworkCore.Migrations;

namespace Gobi.InSync.App.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Watches",
                table => new
                {
                    SourcePath = table.Column<string>(nullable: false),
                    TargetPath = table.Column<string>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Watches", x => new {x.SourcePath, x.TargetPath}); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Watches");
        }
    }
}