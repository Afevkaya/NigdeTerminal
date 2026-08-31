using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NigdeTerminal.App.Data.Migrations;

[DbContext(typeof(NigdeTerminalDbContext))]
[Migration("20260831130000_AddNetTurizmCompany")]
public partial class AddNetTurizmCompany : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $seed$
            BEGIN
                UPDATE firmalar
                SET ad = 'AKSARAY BİRLİK'
                WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e01'
                  AND ad = 'Aksaray';

                UPDATE firmalar
                SET ad = 'DERİNKUYU'
                WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e02'
                  AND ad = 'Derinkuyu';

                UPDATE firmalar
                SET ad = 'KARACAERLER'
                WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e03'
                  AND ad = 'Karacaerler';

                UPDATE firmalar
                SET ad = 'LÜKS EREĞLİ'
                WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e06'
                  AND ad = 'Lüks Ereğli';

                UPDATE firmalar
                SET ad = 'NİĞDE İNAN TURİZM'
                WHERE id IN (
                    '5aa15e8f-faf7-4bf3-a259-09e4433a5e05',
                    '2d20fd37-bcf5-469e-9182-d4b8957e6f39')
                  AND ad IN ('Niğde İnan Turizm', 'NİĞDE İNAN TURİZM');

                UPDATE firmalar
                SET ad = 'NİĞDE AYDOĞANLAR SEYAHAT'
                WHERE id IN (
                    '5aa15e8f-faf7-4bf3-a259-09e4433a5e04',
                    '80197fe2-bc37-4756-8477-8ab932a84eee')
                  AND ad IN ('Niğde Aydoğanlar Seyahat', 'NİĞDE AYDOĞANLAR SEYAHAT');

                IF EXISTS (
                    SELECT 1 FROM firmalar
                    WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e07'
                      AND ad <> 'NET TURİZM SEYAHAT'
                ) THEN
                    RAISE EXCEPTION 'Net Turizm seed ID is already used by another company';
                END IF;

                UPDATE firmalar
                SET ad = 'NET TURİZM SEYAHAT'
                WHERE ad = 'Net Turizm Seyahat';

                UPDATE firmalar
                SET merkez_cikis_yapabilir = true
                WHERE ad IN (
                    'NİĞDE İNAN TURİZM',
                    'NİĞDE AYDOĞANLAR SEYAHAT',
                    'LÜKS EREĞLİ',
                    'NET TURİZM SEYAHAT');

                IF NOT EXISTS (
                    SELECT 1 FROM firmalar
                    WHERE ad = 'NET TURİZM SEYAHAT'
                ) THEN
                    INSERT INTO firmalar
                        (id, ad, firma_turu, merkez_cikis_yapabilir, aktif_mi)
                    VALUES
                        ('5aa15e8f-faf7-4bf3-a259-09e4433a5e07',
                         'NET TURİZM SEYAHAT', 'Sehirlerarasi', true, true);
                END IF;
            END
            $seed$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM firmalar
            WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e07'
              AND ad = 'NET TURİZM SEYAHAT';
            """);
    }
}
