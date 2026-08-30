using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NigdeTerminal.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "firmalar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ad = table.Column<string>(type: "text", nullable: false),
                    firma_turu = table.Column<string>(type: "text", nullable: false),
                    merkez_cikis_yapabilir = table.Column<bool>(type: "boolean", nullable: false),
                    aktif_mi = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_firmalar", x => x.id);
                    table.CheckConstraint("ck_firmalar_firma_turu", "firma_turu IN ('Sehirlerarasi', 'YerelMinibus')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_firmalar_ad",
                table: "firmalar",
                column: "ad",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "firmalar");
        }
    }
}
