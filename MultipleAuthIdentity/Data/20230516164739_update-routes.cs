using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultipleAuthIdentity.Data
{
    public partial class updateroutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Routes");
        }
    }
}
