using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Models;

namespace NigdeTerminal.App.Data;

public sealed class NigdeTerminalDbContext(DbContextOptions<NigdeTerminalDbContext> options)
    : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<ExitRecord> ExitRecords => Set<ExitRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var company = modelBuilder.Entity<Company>();

        company.ToTable("firmalar", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "ck_firmalar_firma_turu",
                "firma_turu IN ('Sehirlerarasi', 'YerelMinibus')"));

        company.HasKey(x => x.Id);

        company.Property(x => x.Id)
            .HasColumnName("id");

        company.Property(x => x.Name)
            .HasColumnName("ad")
            .IsRequired();

        company.HasIndex(x => x.Name)
            .IsUnique();

        company.Property(x => x.CompanyType)
            .HasColumnName("firma_turu")
            .HasConversion(
                value => value == CompanyType.Intercity ? "Sehirlerarasi" : "YerelMinibus",
                value => value == "Sehirlerarasi" ? CompanyType.Intercity : CompanyType.LocalMinibus)
            .IsRequired();

        company.Property(x => x.CanDepartFromCenter)
            .HasColumnName("merkez_cikis_yapabilir")
            .IsRequired();

        company.Property(x => x.IsActive)
            .HasColumnName("aktif_mi")
            .HasDefaultValue(true)
            .IsRequired();

        company.HasData(
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e01"),
                Name = "AKSARAY BİRLİK",
                CompanyType = CompanyType.LocalMinibus,
                CanDepartFromCenter = false,
                IsActive = true
            },
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e02"),
                Name = "DERİNKUYU",
                CompanyType = CompanyType.LocalMinibus,
                CanDepartFromCenter = false,
                IsActive = true
            },
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e03"),
                Name = "KARACAERLER",
                CompanyType = CompanyType.LocalMinibus,
                CanDepartFromCenter = false,
                IsActive = true
            },
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e04"),
                Name = "NİĞDE AYDOĞANLAR SEYAHAT",
                CompanyType = CompanyType.Intercity,
                CanDepartFromCenter = true,
                IsActive = true
            },
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e05"),
                Name = "NİĞDE İNAN TURİZM",
                CompanyType = CompanyType.Intercity,
                CanDepartFromCenter = true,
                IsActive = true
            },
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e06"),
                Name = "LÜKS EREĞLİ",
                CompanyType = CompanyType.Intercity,
                CanDepartFromCenter = true,
                IsActive = true
            },
            new Company
            {
                Id = Guid.Parse("5aa15e8f-faf7-4bf3-a259-09e4433a5e07"),
                Name = "NET TURİZM SEYAHAT",
                CompanyType = CompanyType.Intercity,
                CanDepartFromCenter = true,
                IsActive = true
            });

        var exitRecord = modelBuilder.Entity<ExitRecord>();

        exitRecord.ToTable("cikis_kayitlari", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_cikis_kayitlari_odeme_yontemi",
                "odeme_yontemi IN ('Nakit', 'KrediKarti')");
            tableBuilder.HasCheckConstraint(
                "ck_cikis_kayitlari_tarife_ucreti",
                "tarife_ucreti >= 0");
        });

        exitRecord.HasKey(x => x.Id);

        exitRecord.Property(x => x.Id)
            .HasColumnName("id");

        exitRecord.Property(x => x.CompanyId)
            .HasColumnName("firma_id")
            .IsRequired();

        exitRecord.Property(x => x.VehiclePlate)
            .HasColumnName("plaka")
            .IsRequired();

        exitRecord.Property(x => x.DepartureDate)
            .HasColumnName("cikis_tarihi")
            .HasColumnType("date")
            .IsRequired();

        exitRecord.Property(x => x.DepartureTime)
            .HasColumnName("cikis_saati")
            .HasColumnType("time without time zone")
            .IsRequired();

        exitRecord.Property(x => x.PaymentMethod)
            .HasColumnName("odeme_yontemi")
            .HasConversion(
                value => value == PaymentMethod.Cash ? "Nakit" : "KrediKarti",
                value => value == "Nakit" ? PaymentMethod.Cash : PaymentMethod.CreditCard)
            .IsRequired();

        exitRecord.Property(x => x.TariffName)
            .HasColumnName("tarife_adi")
            .IsRequired();

        exitRecord.Property(x => x.TariffAmount)
            .HasColumnName("tarife_ucreti")
            .IsRequired();

        exitRecord.Property(x => x.DepartedFromCenter)
            .HasColumnName("merkezden_cikti_mi")
            .HasDefaultValue(false)
            .IsRequired();

        exitRecord.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId);

        exitRecord.HasIndex(x => x.CompanyId);
        exitRecord.HasIndex(x => new { x.DepartureDate, x.DepartureTime });
    }
}
