using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NigdeTerminal.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "firmalar",
                columns: new[] { "id", "merkez_cikis_yapabilir", "firma_turu", "aktif_mi", "ad" },
                values: new object[,]
                {
                    { new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e01"), false, "YerelMinibus", true, "Aksaray" },
                    { new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e02"), false, "YerelMinibus", true, "Derinkuyu" },
                    { new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e03"), false, "YerelMinibus", true, "Karacaerler" },
                    { new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e04"), true, "Sehirlerarasi", true, "Aydoğanlar" },
                    { new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e05"), true, "Sehirlerarasi", true, "İnan" },
                    { new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e06"), true, "Sehirlerarasi", true, "Lüks Ereğli" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "firmalar",
                keyColumn: "id",
                keyValue: new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e01"));

            migrationBuilder.DeleteData(
                table: "firmalar",
                keyColumn: "id",
                keyValue: new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e02"));

            migrationBuilder.DeleteData(
                table: "firmalar",
                keyColumn: "id",
                keyValue: new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e03"));

            migrationBuilder.DeleteData(
                table: "firmalar",
                keyColumn: "id",
                keyValue: new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e04"));

            migrationBuilder.DeleteData(
                table: "firmalar",
                keyColumn: "id",
                keyValue: new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e05"));

            migrationBuilder.DeleteData(
                table: "firmalar",
                keyColumn: "id",
                keyValue: new Guid("5aa15e8f-faf7-4bf3-a259-09e4433a5e06"));
        }
    }
}
