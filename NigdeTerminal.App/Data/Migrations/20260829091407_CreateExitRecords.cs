using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NigdeTerminal.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateExitRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cikis_kayitlari",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firma_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plaka = table.Column<string>(type: "text", nullable: false),
                    cikis_tarihi = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    odeme_yontemi = table.Column<string>(type: "text", nullable: false),
                    tarife_adi = table.Column<string>(type: "text", nullable: false),
                    tarife_ucreti = table.Column<decimal>(type: "numeric", nullable: false),
                    merkezden_cikti_mi = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cikis_kayitlari", x => x.id);
                    table.ForeignKey(
                        name: "FK_cikis_kayitlari_firmalar_firma_id",
                        column: x => x.firma_id,
                        principalTable: "firmalar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cikis_kayitlari_cikis_tarihi",
                table: "cikis_kayitlari",
                column: "cikis_tarihi");

            migrationBuilder.CreateIndex(
                name: "IX_cikis_kayitlari_firma_id",
                table: "cikis_kayitlari",
                column: "firma_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cikis_kayitlari");
        }
    }
}
