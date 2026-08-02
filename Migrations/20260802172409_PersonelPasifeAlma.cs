using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkProjesi.Migrations
{
    /// <inheritdoc />
    public partial class PersonelPasifeAlma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut personellerin hepsi calisan durumda oldugu icin
            // varsayilan true olmali, yoksa hepsi pasife duser.
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Personeller",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IseCikisTarihi",
                table: "Personeller",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "IseCikisTarihi",
                table: "Personeller");
        }
    }
}
