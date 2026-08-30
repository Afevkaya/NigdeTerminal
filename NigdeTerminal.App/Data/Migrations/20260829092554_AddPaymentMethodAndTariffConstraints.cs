using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NigdeTerminal.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodAndTariffConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_cikis_kayitlari_odeme_yontemi",
                table: "cikis_kayitlari",
                sql: "odeme_yontemi IN ('Nakit', 'KrediKarti')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cikis_kayitlari_tarife_ucreti",
                table: "cikis_kayitlari",
                sql: "tarife_ucreti >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_cikis_kayitlari_odeme_yontemi",
                table: "cikis_kayitlari");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cikis_kayitlari_tarife_ucreti",
                table: "cikis_kayitlari");
        }
    }
}
