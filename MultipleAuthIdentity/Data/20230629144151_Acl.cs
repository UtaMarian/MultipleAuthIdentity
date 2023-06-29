using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultipleAuthIdentity.Data
{
    public partial class Acl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "AccessListItem",
               columns: table => new
               {
                   Id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                   HasAccess = table.Column<bool>(type: "bit", nullable: false),
                   Reason = table.Column<string>(type: "nvarchar(max)", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_AccessListItem", x => x.Id);
               });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
               name: "AccessListItem");
        }
    }
}
