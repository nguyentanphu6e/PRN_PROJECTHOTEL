IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomTypes] (
        [RoomTypeId] int NOT NULL IDENTITY,
        [Name] nvarchar(80) NOT NULL,
        [Capacity] int NOT NULL,
        [BasePrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_RoomTypes] PRIMARY KEY ([RoomTypeId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [Services] (
        [ServiceId] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FullName] nvarchar(120) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [PasswordHash] nvarchar(200) NOT NULL,
        [Role] nvarchar(24) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [Rooms] (
        [RoomId] int NOT NULL IDENTITY,
        [RoomTypeId] int NOT NULL,
        [RoomNumber] nvarchar(20) NOT NULL,
        [Status] nvarchar(24) NOT NULL,
        [Floor] int NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([RoomId]),
        CONSTRAINT [FK_Rooms_RoomTypes_RoomTypeId] FOREIGN KEY ([RoomTypeId]) REFERENCES [RoomTypes] ([RoomTypeId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [BookingId] int NOT NULL IDENTITY,
        [BookingCode] nvarchar(24) NOT NULL,
        [CustomerId] int NOT NULL,
        [RoomId] int NOT NULL,
        [CheckIn] datetime2 NOT NULL,
        [CheckOut] datetime2 NOT NULL,
        [NightlyRateAtBooking] decimal(18,2) NOT NULL,
        [Status] nvarchar(24) NOT NULL,
        [GuestName] nvarchar(120) NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
        CONSTRAINT [FK_Bookings_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([RoomId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [InvoiceId] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [RoomTotal] decimal(18,2) NOT NULL,
        [ServiceTotal] decimal(18,2) NOT NULL,
        [PaymentStatus] nvarchar(24) NOT NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
        CONSTRAINT [FK_Invoices_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceOrders] (
        [ServiceOrderId] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPriceAtOrder] decimal(18,2) NOT NULL,
        [Status] nvarchar(24) NOT NULL,
        [ServiceName] nvarchar(100) NULL,
        CONSTRAINT [PK_ServiceOrders] PRIMARY KEY ([ServiceOrderId]),
        CONSTRAINT [FK_ServiceOrders_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ServiceOrders_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [PaymentId] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] nvarchar(40) NOT NULL,
        [Status] nvarchar(24) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
        CONSTRAINT [FK_Payments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([InvoiceId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bookings_BookingCode] ON [Bookings] ([BookingCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_CustomerId] ON [Bookings] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_RoomId_CheckIn_CheckOut] ON [Bookings] ([RoomId], [CheckIn], [CheckOut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_BookingId] ON [Invoices] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_InvoiceId] ON [Payments] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rooms_RoomNumber] ON [Rooms] ([RoomNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rooms_RoomTypeId] ON [Rooms] ([RoomTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RoomTypes_Name] ON [RoomTypes] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceOrders_BookingId] ON [ServiceOrders] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceOrders_ServiceId] ON [ServiceOrders] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105173753_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105173753_InitialCreate', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105181445_AddBookingGuestCount'
)
BEGIN
    ALTER TABLE [Bookings] ADD [GuestCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105181445_AddBookingGuestCount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105181445_AddBookingGuestCount', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105184520_AddServiceOrderUsedAt'
)
BEGIN
    ALTER TABLE [ServiceOrders] ADD [UsedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105184520_AddServiceOrderUsedAt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105184520_AddServiceOrderUsedAt', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105190556_AddBookingGuestPhone'
)
BEGIN
    ALTER TABLE [Bookings] ADD [GuestPhone] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105190556_AddBookingGuestPhone'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105190556_AddBookingGuestPhone', N'8.0.33');
END;
GO

COMMIT;
GO

