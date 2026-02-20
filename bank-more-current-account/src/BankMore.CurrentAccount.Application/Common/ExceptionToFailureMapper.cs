using System.Net;

namespace BankMore.CurrentAccount.Application.Common;

public static class ExceptionToFailureMapper
{
    public static (HttpStatusCode StatusCode, FailureType FailureType, string Message) Map(Exception ex)
    {
        if (ex.GetType().FullName?.Contains("Oracle.ManagedDataAccess") == true)
            return (HttpStatusCode.InternalServerError, FailureType.InternalError, "Database error. Check logs and connection.");

        return ex switch
        {
            UnauthorizedAccessException u => (HttpStatusCode.Unauthorized, FailureType.UserUnauthorized, ExtractMessage(u.Message, "Unauthorized")),
            ApplicationException a when a.Message.StartsWith("INVALID_DOCUMENT") => (HttpStatusCode.BadRequest, FailureType.InvalidDocument, ExtractMessage(a.Message, "Invalid document")),
            ApplicationException a when a.Message.StartsWith("INVALID_ACCOUNT") => (HttpStatusCode.BadRequest, FailureType.InvalidAccount, ExtractMessage(a.Message, "Invalid account")),
            ApplicationException a when a.Message.StartsWith("INACTIVE_ACCOUNT") => (HttpStatusCode.BadRequest, FailureType.InactiveAccount, ExtractMessage(a.Message, "Inactive account")),
            ApplicationException a when a.Message.StartsWith("INVALID_VALUE") => (HttpStatusCode.BadRequest, FailureType.InvalidValue, ExtractMessage(a.Message, "Invalid value")),
            ApplicationException a when a.Message.StartsWith("INVALID_TYPE") => (HttpStatusCode.BadRequest, FailureType.InvalidType, ExtractMessage(a.Message, "Invalid type")),
            ApplicationException a => (HttpStatusCode.BadRequest, FailureType.Error, a.Message),
            _ => (HttpStatusCode.InternalServerError, FailureType.InternalError, "An unexpected error occurred.")
        };
    }

    private static string ExtractMessage(string fullMessage, string fallback)
    {
        if (string.IsNullOrEmpty(fullMessage)) return fallback;
        var idx = fullMessage.IndexOf('|');
        return idx >= 0 && idx < fullMessage.Length - 1
            ? fullMessage[(idx + 1)..].Trim()
            : (fullMessage.Length > 0 ? fullMessage : fallback);
    }
}
