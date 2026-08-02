using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IkProjesi.Migrations
{
    /// <inheritdoc />
    public partial class IzinHakkiDevirSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IzinHaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    HakEdilen = table.Column<int>(type: "integer", nullable: false),
                    Devreden = table.Column<int>(type: "integer", nullable: false),
                    Kullanilan = table.Column<int>(type: "integer", nullable: false),
                    KullanilanMazeret = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinHaklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IzinHaklari_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IzinHaklari_PersonelId",
                table: "IzinHaklari",
                column: "PersonelId");

            // Personel tablosundaki mevcut izin bilgileri, kolonlar silinmeden önce
            // içinde bulunulan yılın hakkı olarak yeni tabloya taşınır.
            migrationBuilder.Sql(@"
                INSERT INTO ""IzinHaklari""
                    (""PersonelId"", ""Yil"", ""HakEdilen"", ""Devreden"", ""Kullanilan"", ""KullanilanMazeret"")
                SELECT
                    ""Id"",
                    EXTRACT(YEAR FROM CURRENT_DATE)::int,
                    ""YillikIzinHakki"",
                    0,
                    ""KullanılanIzin"",
                    ""KullanilanMazeretIzin""
                FROM ""Personeller"";");

            migrationBuilder.DropColumn(
                name: "KullanilanMazeretIzin",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "KullanılanIzin",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "YillikIzinHakki",
                table: "Personeller");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KullanilanMazeretIzin",
                table: "Personeller",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KullanılanIzin",
                table: "Personeller",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YillikIzinHakki",
                table: "Personeller",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Tablo silinmeden önce en güncel yılın hakları Personel tablosuna geri yazılır.
            migrationBuilder.Sql(@"
                UPDATE ""Personeller"" p
                SET ""YillikIzinHakki"" = h.""HakEdilen"" + h.""Devreden"",
                    ""KullanılanIzin"" = h.""Kullanilan"",
                    ""KullanilanMazeretIzin"" = h.""KullanilanMazeret""
                FROM (
                    SELECT DISTINCT ON (""PersonelId"")
                        ""PersonelId"", ""HakEdilen"", ""Devreden"", ""Kullanilan"", ""KullanilanMazeret""
                    FROM ""IzinHaklari""
                    ORDER BY ""PersonelId"", ""Yil"" DESC
                ) h
                WHERE p.""Id"" = h.""PersonelId"";");

            migrationBuilder.DropTable(
                name: "IzinHaklari");
        }
    }
}
