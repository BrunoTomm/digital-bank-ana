namespace BankMore.Transfer.Application.Common;

public static class FailureTypeExtensions
{
    public static string ToCode(this FailureType value) => value switch
    {
        FailureType.InvalidAccount => "INVALID_ACCOUNT",
        FailureType.InvalidValue => "INVALID_VALUE",
        FailureType.TransferFailed => "TRANSFER_FAILED",
        FailureType.Error => "ERROR",
        FailureType.InternalError => "INTERNAL_ERROR",
        _ => "ERROR"
    };
}
