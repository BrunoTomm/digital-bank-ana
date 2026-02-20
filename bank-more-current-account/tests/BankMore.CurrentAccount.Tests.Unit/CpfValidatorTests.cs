using BankMore.CurrentAccount.Application.Validators;
using FluentAssertions;
using Xunit;

namespace BankMore.CurrentAccount.Tests.Unit;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25", true)]
    [InlineData("52998224725", true)]
    [InlineData("111.111.111-11", false)]
    [InlineData("00000000000", false)]
    [InlineData("123", false)]
    public void IsValid_ReturnsExpected(string cpf, bool expected)
    {
        CpfValidator.IsValid(cpf).Should().Be(expected);
    }

    [Fact]
    public void Normalize_RemovesNonDigits()
    {
        CpfValidator.Normalize("529.982.247-25").Should().Be("52998224725");
    }
}
