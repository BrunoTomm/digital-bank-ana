namespace BankMore.CurrentAccount.Application.Common;

public static class FailureTypeExtensions
{
    public static string ToCode(this FailureType value) => value switch
    {
        FailureType.InvalidDocument => "INVALID_DOCUMENT",
        FailureType.UserUnauthorized => "USER_UNAUTHORIZED",
        FailureType.InvalidAccount => "INVALID_ACCOUNT",
        FailureType.InactiveAccount => "INACTIVE_ACCOUNT",
        FailureType.InvalidValue => "INVALID_VALUE",
        FailureType.InvalidType => "INVALID_TYPE",
        FailureType.Error => "ERROR",
        FailureType.InternalError => "INTERNAL_ERROR",
        _ => "ERROR"
    };
}
