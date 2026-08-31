using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NigdeTerminal.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitExitDepartureDateAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cikis_kayitlari_cikis_tarihi",
                table: "cikis_kayitlari");

            migrationBuilder.Sql("""
                ALTER TABLE cikis_kayitlari
                ADD COLUMN cikis_saati time without time zone;

                UPDATE cikis_kayitlari
                SET cikis_saati = (cikis_tarihi AT TIME ZONE 'Europe/Istanbul')::time;

                ALTER TABLE cikis_kayitlari
                ALTER COLUMN cikis_saati SET NOT NULL;

                ALTER TABLE cikis_kayitlari
                ALTER COLUMN cikis_tarihi TYPE date
                USING (cikis_tarihi AT TIME ZONE 'Europe/Istanbul')::date;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_cikis_kayitlari_cikis_tarihi_cikis_saati",
                table: "cikis_kayitlari",
                columns: new[] { "cikis_tarihi", "cikis_saati" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cikis_kayitlari_cikis_tarihi_cikis_saati",
                table: "cikis_kayitlari");

            migrationBuilder.Sql("""
                ALTER TABLE cikis_kayitlari
                ALTER COLUMN cikis_tarihi TYPE timestamp with time zone
                USING ((cikis_tarihi + cikis_saati) AT TIME ZONE 'Europe/Istanbul');

                ALTER TABLE cikis_kayitlari
                DROP COLUMN cikis_saati;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_cikis_kayitlari_cikis_tarihi",
                table: "cikis_kayitlari",
                column: "cikis_tarihi");
        }
    }
}
