using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using NSubstitute;
using System;
using Xunit;

namespace Moneybox.App.Tests
{
    public class TransferMoneyShould
    {

        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoneyShould()
        {
            accountRepository = Substitute.For<IAccountRepository>();
            notificationService = Substitute.For<INotificationService>();

            
        }

        [Fact]
        public void ThrowInvalidOperationExceptionWhenTransferExceedsAvailableFunds()
        {
            //Arrange
            var transferMoney = new Features.TransferMoney(null, null);
            //Act

            //Assert
            //transferMoney.Execute
        }
    }
}
