using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace proyecto.Migrations
{
    /// <inheritdoc />
    public partial class MaterialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_material",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<string>(type: "text", nullable: true),
                    Modelo = table.Column<string>(type: "text", nullable: false),
                    NombreTela = table.Column<string>(type: "text", nullable: false),
                    TipoTela = table.Column<string>(type: "text", nullable: false),
                    Precio = table.Column<double>(type: "double precision", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioTotal = table.Column<double>(type: "double precision", nullable: false),
                    Proveedor = table.Column<string>(type: "text", nullable: false),
                    ProveedorContacto = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_material", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_material");
        }
    }
}
