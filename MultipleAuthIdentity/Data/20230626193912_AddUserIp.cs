using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultipleAuthIdentity.Data
{
    public partial class AddUserIp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AspNetUsers");
        }
    }
}
