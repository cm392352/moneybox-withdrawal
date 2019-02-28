using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using NSubstitute;
using System;
using Xunit;

namespace Moneybox.App.Tests
{
    public class TransferMoneyShould
    {

        private static Guid originatingAccountId => Guid.Parse("b0fee6fa-7091-417e-88f5-cc6ba99f162b");
        private static Guid destinationAccountId => Guid.Parse("921a2afb-406c-48f1-b71a-edcbc659730a");

        private static Guid originatingUserId => Guid.Parse("a3b059e3-4ca8-40a3-a088-b76e0b70d1f9");
        private static Guid destinationUserId => Guid.Parse("e22a0f51-4b3b-45c0-af07-bbd834ae19db");

        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoneyShould()
        {
            accountRepository = Substitute.For<IAccountRepository>();
            notificationService = Substitute.For<INotificationService>();
        }

        private TransferMoney SetupTransferMoney(decimal originatingBalance, decimal destinationBalance)
        {
            var originatingAccountUser = new User { Id = originatingUserId, Email = "originating@user.com", Name = "Originating User" };
            var originatingAccount = new Account { Id = originatingAccountId, Balance = originatingBalance, User = originatingAccountUser };

            var destinationAccountUser = new User { Id = destinationUserId, Email = "destination@user.com", Name = "Destination User" };
            var destinationAccount = new Account { Id = destinationAccountId, Balance = destinationBalance, User = destinationAccountUser };

            accountRepository.GetAccountById(originatingAccountId).Returns(originatingAccount);
            accountRepository.GetAccountById(destinationAccountId).Returns(destinationAccount);

            return new TransferMoney(accountRepository, notificationService);
        }

        [Fact]
        
        public void ThrowInvalidOperationExceptionWhenTransferExceedsAvailableFunds()
        {
            //Arrange
            var transferMoney = SetupTransferMoney(500m, 500m);
            //Act
            Action action = () => transferMoney.Execute(originatingAccountId, destinationAccountId, 600m);
            //Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ThrowInvalidOperationExceptionWhenTransferExceedsAccountPayInLimit()
        {
            //Arrange
            var transferMoney = SetupTransferMoney(6000m, 6000m);
            //Act
            Action action = () => transferMoney.Execute(originatingAccountId, destinationAccountId, 4500m);
            //Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void CallNotifyFundsLowIfTransferCausesLowFunds()
        {
            //Arrange
            var transferMoney = SetupTransferMoney(500m, 500m);
            //Act
           transferMoney.Execute(originatingAccountId, destinationAccountId, 400m);
            //Assert
            notificationService.Received().NotifyFundsLow(accountRepository.GetAccountById(originatingAccountId).User.Email);
        }

        [Fact]
        public void CallNotifyApproachingPayInLimitIfTransferCausesPayInLimitToBeClose()
        {
            //Arrange
            var transferMoney = SetupTransferMoney(5000m, 500m);
            //Act
            transferMoney.Execute(originatingAccountId, destinationAccountId, 3600m);
            //Assert
            notificationService.Received().NotifyApproachingPayInLimit(accountRepository.GetAccountById(destinationAccountId).User.Email);
        }

        [Fact]
        public void SuccessfullyIncrementDestinationAccountBalanceIfFundsAvailableAndNotExceedingPayInLimit()
        {
            //Arrange
            var transferMoney = SetupTransferMoney(5000m, 500m);
            //Act
            transferMoney.Execute(originatingAccountId, destinationAccountId, 2000m);
            var destinationAccountBalance = accountRepository.GetAccountById(destinationAccountId).Balance;
            //Assert

            Assert.Equal(2500m, destinationAccountBalance);
        }

        [Fact]
        public void SuccessfullyDebitSenderAccountBalanceIfFundsAvailableAndNotExceedingPayInLimit()
        {
            //Arrange
            var transferMoney = SetupTransferMoney(5000m, 500m);
            //Act
            transferMoney.Execute(originatingAccountId, destinationAccountId, 2000m);
            var originatingAccountBalance = accountRepository.GetAccountById(originatingAccountId).Balance;
            //Assert
            Assert.Equal(3000m, originatingAccountBalance);
        }
    }
}
