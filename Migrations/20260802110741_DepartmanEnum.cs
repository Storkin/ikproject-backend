using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkProjesi.Migrations
{
    /// <inheritdoc />
    public partial class DepartmanEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Personeller\" ALTER COLUMN \"Departman\" TYPE integer USING (\"Departman\"::integer);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Personeller\" ALTER COLUMN \"Departman\" TYPE text USING (\"Departman\"::text);");
        }
    }
}
