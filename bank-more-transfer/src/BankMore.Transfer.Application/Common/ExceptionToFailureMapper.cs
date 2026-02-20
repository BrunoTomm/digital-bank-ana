using System.Net;

namespace BankMore.Transfer.Application.Common;

public static class ExceptionToFailureMapper
{
    public static (HttpStatusCode StatusCode, FailureType FailureType, string Message) Map(Exception ex)
    {
        return ex switch
        {
            ApplicationException a when a.Message.StartsWith("INVALID_ACCOUNT") => (HttpStatusCode.BadRequest, FailureType.InvalidAccount, ExtractMessage(a.Message, "Invalid account")),
            ApplicationException a when a.Message.StartsWith("INVALID_VALUE") => (HttpStatusCode.BadRequest, FailureType.InvalidValue, ExtractMessage(a.Message, "Invalid value")),
            ApplicationException a when a.Message.StartsWith("TRANSFER_FAILED") => (HttpStatusCode.BadRequest, FailureType.TransferFailed, ExtractMessage(a.Message, "Transfer failed")),
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
