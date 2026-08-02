using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IkProjesi.Migrations
{
    /// <inheritdoc />
    public partial class DeneyimYerineBakanIlkGiris : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLogin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubstituteId",
                table: "IzinTalepler",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KullanilanUcretsiz",
                table: "IzinHaklari",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiences_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepler_SubstituteId",
                table: "IzinTalepler",
                column: "SubstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_PersonelId",
                table: "Experiences",
                column: "PersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_IzinTalepler_Personeller_SubstituteId",
                table: "IzinTalepler",
                column: "SubstituteId",
                principalTable: "Personeller",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IzinTalepler_Personeller_SubstituteId",
                table: "IzinTalepler");

            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropIndex(
                name: "IX_IzinTalepler_SubstituteId",
                table: "IzinTalepler");

            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubstituteId",
                table: "IzinTalepler");

            migrationBuilder.DropColumn(
                name: "KullanilanUcretsiz",
                table: "IzinHaklari");
        }
    }
}
