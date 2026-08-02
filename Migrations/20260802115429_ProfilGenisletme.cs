using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IkProjesi.Migrations
{
    /// <inheritdoc />
    public partial class ProfilGenisletme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adres",
                table: "Personeller",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DogumTarihi",
                table: "Personeller",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Personeller",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KullanilanMazeretIzin",
                table: "Personeller",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Telefon",
                table: "Personeller",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unvan",
                table: "Personeller",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Turu",
                table: "IzinTalepler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Egitimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Kurum = table.Column<string>(type: "text", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Egitimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Egitimler_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaasKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric", nullable: false),
                    Tur = table.Column<int>(type: "integer", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaasKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaasKayitlari_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zimmetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    EsyaAdi = table.Column<string>(type: "text", nullable: false),
                    SeriNo = table.Column<string>(type: "text", nullable: true),
                    ZimmetTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IadeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zimmetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Egitimler_PersonelId",
                table: "Egitimler",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_MaasKayitlari_PersonelId",
                table: "MaasKayitlari",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_PersonelId",
                table: "Zimmetler",
                column: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Egitimler");

            migrationBuilder.DropTable(
                name: "MaasKayitlari");

            migrationBuilder.DropTable(
                name: "Zimmetler");

            migrationBuilder.DropColumn(
                name: "Adres",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "DogumTarihi",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "KullanilanMazeretIzin",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "Telefon",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "Unvan",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "Turu",
                table: "IzinTalepler");
        }
    }
}
