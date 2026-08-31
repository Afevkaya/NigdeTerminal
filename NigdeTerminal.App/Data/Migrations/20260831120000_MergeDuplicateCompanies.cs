using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NigdeTerminal.App.Data.Migrations;

[DbContext(typeof(NigdeTerminalDbContext))]
[Migration("20260831120000_MergeDuplicateCompanies")]
public partial class MergeDuplicateCompanies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $cleanup$
            BEGIN
                UPDATE firmalar
                SET ad = 'Niğde Aydoğanlar Seyahat'
                WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e04'
                  AND ad = 'Aydoğanlar';

                UPDATE firmalar
                SET ad = 'Niğde İnan Turizm'
                WHERE id = '5aa15e8f-faf7-4bf3-a259-09e4433a5e05'
                  AND ad = 'İnan';

                IF EXISTS (
                    SELECT 1 FROM firmalar
                    WHERE id = '095bbe7a-3677-4947-8e94-4f1be182cd35'
                ) THEN
                    IF NOT EXISTS (
                        SELECT 1 FROM firmalar
                        WHERE id = '2d20fd37-bcf5-469e-9182-d4b8957e6f39'
                          AND ad IN ('NİĞDE İNAN TURİZM', 'Niğde İnan Turizm')
                    ) THEN
                        RAISE EXCEPTION 'İnan keeper company is missing or unexpected; cleanup cancelled';
                    END IF;

                    UPDATE cikis_kayitlari
                    SET firma_id = '2d20fd37-bcf5-469e-9182-d4b8957e6f39'
                    WHERE firma_id = '095bbe7a-3677-4947-8e94-4f1be182cd35';

                    DELETE FROM firmalar
                    WHERE id = '095bbe7a-3677-4947-8e94-4f1be182cd35'
                      AND ad = 'İNAN TURİZM';

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'İnan duplicate company has unexpected data; cleanup cancelled';
                    END IF;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM firmalar
                    WHERE id = 'edfef91a-24f3-493c-be05-604c8d67f7a0'
                ) THEN
                    IF NOT EXISTS (
                        SELECT 1 FROM firmalar
                        WHERE id = '80197fe2-bc37-4756-8477-8ab932a84eee'
                          AND ad IN ('NİĞDE AYDOĞANLAR SEYAHAT', 'Niğde Aydoğanlar Seyahat')
                    ) THEN
                        RAISE EXCEPTION 'Aydoğanlar keeper company is missing or unexpected; cleanup cancelled';
                    END IF;

                    UPDATE cikis_kayitlari
                    SET firma_id = '80197fe2-bc37-4756-8477-8ab932a84eee'
                    WHERE firma_id = 'edfef91a-24f3-493c-be05-604c8d67f7a0';

                    DELETE FROM firmalar
                    WHERE id = 'edfef91a-24f3-493c-be05-604c8d67f7a0'
                      AND ad = 'AYDOĞANLAR TURİZM';

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'Aydoğanlar duplicate company has unexpected data; cleanup cancelled';
                    END IF;
                END IF;

                UPDATE firmalar
                SET ad = 'Niğde İnan Turizm'
                WHERE id = '2d20fd37-bcf5-469e-9182-d4b8957e6f39'
                  AND ad IN ('NİĞDE İNAN TURİZM', 'Niğde İnan Turizm');

                UPDATE firmalar
                SET ad = 'Niğde Aydoğanlar Seyahat'
                WHERE id = '80197fe2-bc37-4756-8477-8ab932a84eee'
                  AND ad IN ('NİĞDE AYDOĞANLAR SEYAHAT', 'Niğde Aydoğanlar Seyahat');
            END
            $cleanup$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "Duplicate firma kayıtlarının güvenli biçimde ayrıştırılması mümkün olmadığı için bu veri temizliği geri alınamaz.");
    }
}
