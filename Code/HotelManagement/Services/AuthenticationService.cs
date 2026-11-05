using BusinessObjects;
using DataAccessObjects;
using Repositories;

namespace Services;

public sealed class AuthenticationService(IUserRepository users)
{
    public User? Login(string email, string password) => users.Authenticate(email.Trim(), password);
}

public static class HotelServicesFactory
{
    private static readonly HotelDbContextFactory ContextFactory = new();

    public static AuthenticationService CreateAuthenticationService() =>
        new(new UserRepository(new UserDAO(ContextFactory)));

    public static HotelManagementService CreateHotelManagementService() =>
        new(new RoomRepository(new RoomDAO(ContextFactory)),
            new BookingRepository(new BookingDAO(ContextFactory)),
            new HotelServiceRepository(new ServiceDAO(ContextFactory)),
            new InvoiceRepository(new InvoiceDAO(ContextFactory)),
            new UserRepository(new UserDAO(ContextFactory)));
}
