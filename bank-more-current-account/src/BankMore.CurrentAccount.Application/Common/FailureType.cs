namespace BankMore.CurrentAccount.Application.Common;

public enum FailureType
{
    InvalidDocument,
    UserUnauthorized,
    InvalidAccount,
    InactiveAccount,
    InvalidValue,
    InvalidType,
    Error,
    InternalError
}
