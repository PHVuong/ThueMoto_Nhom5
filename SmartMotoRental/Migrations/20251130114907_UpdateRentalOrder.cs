using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMotoRental.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRentalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentalOrders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BikeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PickupLocation = table.Column<string>(type: "TEXT", nullable: false),
                    ReturnLocation = table.Column<string>(type: "TEXT", nullable: false),
                    PickupDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedPrice = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalOrders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_RentalOrders_Motorbikes_BikeId",
                        column: x => x.BikeId,
                        principalTable: "Motorbikes",
                        principalColumn: "BikeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_BikeId",
                table: "RentalOrders",
                column: "BikeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalOrders");
        }
    }
}
