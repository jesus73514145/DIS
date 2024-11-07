using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto.Migrations
{
    /// <inheritdoc />
    public partial class NombreDeLaMigracion1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SedeId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SedeId",
                table: "AspNetUsers",
                column: "SedeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_t_sedes_SedeId",
                table: "AspNetUsers",
                column: "SedeId",
                principalTable: "t_sedes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_t_sedes_SedeId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SedeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SedeId",
                table: "AspNetUsers");
        }
    }
}
