namespace SmartMotoRental.Models;

public enum UserRole
{
    Admin = 1,
    Staff = 2,
    Customer = 3
}

public enum ChatSender
{
    System = 0,
    Admin = 1,
    User = 2
}

public enum RentalStatus
{
    Pending = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    BankTransfer = 2,
    EWallet = 3
}

public enum MotorbikeCondition
{
    New = 0,
    Good = 1,
    Fair = 2,
    Poor = 3
}

public enum MotorbikeStatus
{
    Available = 0,
    Unavailable = 1,
    Maintenance = 2,
    Rented = 3
}

public enum NotificationType
{
    System = 0,
    Rental = 1,
    Payment = 2,
    Promotion = 3
}


