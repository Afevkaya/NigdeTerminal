namespace NigdeTerminal.App.Models;

public sealed class Company
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public CompanyType CompanyType { get; set; }

    public bool CanDepartFromCenter { get; set; }

    public bool IsActive { get; set; } = true;
}
