namespace NigdeTerminal.Tests;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("NIGDE_TERMINAL_RUN_POSTGRES_TESTS"),
                "1",
                StringComparison.Ordinal))
        {
            Skip = "Set NIGDE_TERMINAL_RUN_POSTGRES_TESTS=1 when PostgreSQL is available.";
        }
    }
}
