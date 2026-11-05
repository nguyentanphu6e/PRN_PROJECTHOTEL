namespace BusinessObjects;
public enum UserRole { Customer, Receptionist, Administrator }
public enum RoomStatus { Available, Occupied, Cleaning, Maintenance }
public enum BookingStatus { Pending, Confirmed, CheckedIn, CheckedOut, Cancelled }
public enum ServiceOrderStatus { Pending, Processing, Completed, Cancelled }
public enum PaymentStatus { Unpaid, PartiallyPaid, Paid, Refunded }
public enum RoomPriceBand { Any, Under500000, From500000To1000000, From1000000To2000000, Over2000000 }
public enum RoomPriceSort { Default, LowToHigh, HighToLow }
