using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccessObjects;

public sealed class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public static string ConnectionString => Environment.GetEnvironmentVariable("HOTEL_DB_CONNECTION") ?? "Data Source=.\\SQLEXPRESS;Initial Catalog=HotelManagementDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public HotelDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>().UseSqlServer(ConnectionString).Options;
        return new HotelDbContext(options);
    }

    public HotelDbContext CreateDbContext() => CreateDbContext([]);
}
