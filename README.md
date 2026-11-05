# PRN_PROJECTHOTEL

Hotel Management System built with C# WPF, .NET 8, EF Core, and SQL Server.

## Repository layout

- `Code/HotelManagement/` — WPF solution, source projects, and EF Core migrations.
- `Database/HotelManagementDb.sql` — idempotent SQL Server schema migration script. The script contains schema only; local customer/account/booking records are not included.

## Create or update the database

The default connection uses Windows authentication and SQL Server Express at `./SQLEXPRESS`, with database name `HotelManagementDb`.

From `Code/HotelManagement`:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project DataAccessObjects\DataAccessObjects.csproj --startup-project HotelManagement.csproj
```

To use SQL authentication, set `HOTEL_DB_CONNECTION` in your local environment before running the app. Do not commit credentials or database backups containing personal records.

Open `Code/HotelManagement/HotelManagement.sln` in Visual Studio and run the `HotelManagement` project.
