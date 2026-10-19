using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISII.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EstimatedDeliveryDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingMethods", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingMethods");
        }
    }
}
