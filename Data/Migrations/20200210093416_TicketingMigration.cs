using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ticketo5.Data.Migrations
{
    public partial class TicketingMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(maxLength: 60, nullable: false),
                    description = table.Column<string>(nullable: true),
                    assignedBy = table.Column<string>(nullable: false),
                    ownedBy = table.Column<string>(nullable: false),
                    category = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    createdOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ticket");
        }
    }
}
