using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace proyecto.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_costeo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Empresa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cantidad_Prendas = table.Column<int>(type: "integer", nullable: false),
                    Tela1_Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tela1_Costo = table.Column<double>(type: "double precision", nullable: false),
                    Tela1_Cantidad = table.Column<double>(type: "double precision", nullable: false),
                    Tela2_Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tela2_Costo = table.Column<double>(type: "double precision", nullable: true),
                    Tela2_Cantidad = table.Column<double>(type: "double precision", nullable: true),
                    Molde = table.Column<double>(type: "double precision", nullable: false),
                    Tizado = table.Column<double>(type: "double precision", nullable: false),
                    Corte = table.Column<double>(type: "double precision", nullable: false),
                    Confección = table.Column<double>(type: "double precision", nullable: false),
                    Botones = table.Column<double>(type: "double precision", nullable: false),
                    Pegado_Botón = table.Column<double>(type: "double precision", nullable: false),
                    Otros = table.Column<double>(type: "double precision", nullable: true),
                    Avios = table.Column<double>(type: "double precision", nullable: false),
                    Tricotex = table.Column<double>(type: "double precision", nullable: false),
                    Acabados = table.Column<double>(type: "double precision", nullable: false),
                    CostoTransporte = table.Column<double>(type: "double precision", nullable: false),
                    CU_Final = table.Column<double>(type: "double precision", nullable: false),
                    CT_Final = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_costeo", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_costeo");
        }
    }
}
