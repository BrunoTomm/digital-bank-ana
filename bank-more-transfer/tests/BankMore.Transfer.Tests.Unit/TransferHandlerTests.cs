using BankMore.Transfer.Application.Commands.Transfer;
using BankMore.Transfer.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BankMore.Transfer.Tests.Unit;

public class TransferHandlerTests
{
    [Fact]
    public void TransferCommand_ShouldHaveCorrectProperties()
    {
        var cmd = new TransferCommand("acc-origin", 1001, 50.00m);
        cmd.OriginAccountId.Should().Be("acc-origin");
        cmd.DestinationAccountNumber.Should().Be(1001);
        cmd.Amount.Should().Be(50.00m);
    }
}
