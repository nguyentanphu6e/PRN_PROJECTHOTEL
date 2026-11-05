using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class UserDAO(HotelDbContextFactory factory)
{
    public User? Authenticate(string email, string password)
    {
        using var db = factory.CreateDbContext();
        var user = db.Users.AsNoTracking().SingleOrDefault(x => x.Email == email && x.IsActive);
        return user is not null && PasswordHashing.Verify(password, user.PasswordHash) ? user : null;
    }

    public IReadOnlyList<User> GetActiveCustomers()
    {
        using var db = factory.CreateDbContext();
        return db.Users.AsNoTracking().Where(x => x.IsActive && x.Role == UserRole.Customer)
            .OrderBy(x => x.FullName).ToList();
    }

    public IReadOnlyList<User> GetAll()
    {
        using var db = factory.CreateDbContext();
        return db.Users.AsNoTracking().OrderBy(x => x.FullName).ToList();
    }

    public bool SetActive(int userId, bool isActive)
    {
        using var db = factory.CreateDbContext();
        var user = db.Users.SingleOrDefault(x => x.UserId == userId);
        if (user is null) return false;
        user.IsActive = isActive;
        db.SaveChanges();
        return true;
    }
}
