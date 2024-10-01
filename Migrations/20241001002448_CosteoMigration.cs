using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto.Migrations
{
    /// <inheritdoc />
    public partial class CosteoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "t_costeo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fec_Actualizacion",
                table: "t_costeo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fec_Creacion",
                table: "t_costeo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "t_costeo");

            migrationBuilder.DropColumn(
                name: "fec_Actualizacion",
                table: "t_costeo");

            migrationBuilder.DropColumn(
                name: "fec_Creacion",
                table: "t_costeo");
        }
    }
}
