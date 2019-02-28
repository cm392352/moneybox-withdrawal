using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using NSubstitute;
using System;
using Xunit;

namespace Moneybox.App.Tests
{
    public class WithdrawMoneyShould
    {

        private static Guid withdrawalAccountId => Guid.Parse("b0fee6fa-7091-417e-88f5-cc6ba99f162b");
        private static Guid withdrawalUserId => Guid.Parse("a3b059e3-4ca8-40a3-a088-b76e0b70d1f9");

        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoneyShould()
        {
            accountRepository = Substitute.For<IAccountRepository>();
            notificationService = Substitute.For<INotificationService>();
        }

        private WithdrawMoney SetupWithdrawMoney(decimal originatingBalance)
        {
            var withdrawalUser = new User { Id = withdrawalUserId, Email = "withdrawal@user.com", Name = "Withdrawal User" };
            var withdrawalAccount = new Account(originatingBalance) { Id = withdrawalAccountId, User = withdrawalUser };

            accountRepository.GetAccountById(withdrawalAccountId).Returns(withdrawalAccount);

            return new WithdrawMoney(accountRepository, notificationService);
        }

        [Fact]

        public void ThrowInvalidOperationExceptionWhenWithdrawalExceedsAvailableFunds()
        {
            //Arrange
            var withdrawMoney = SetupWithdrawMoney(500m);
            //Act
            Action action = () => withdrawMoney.Execute(withdrawalAccountId, 600m);
            //Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void CallNotifyFundsLowIfWithdrawalCausesLowFunds()
        {
            //Arrange
            var withdrawMoney = SetupWithdrawMoney(500m);
            //Act
            withdrawMoney.Execute(withdrawalAccountId, 400m);
            //Assert
            notificationService.Received().NotifyFundsLow(accountRepository.GetAccountById(withdrawalAccountId).User.Email);
        }

        [Fact]
        public void SuccessfullyDebitAccountBalanceIfFundsAvailable()
        {
            //Arrange
            var withdrawMoney = SetupWithdrawMoney(500m);
            //Act
            withdrawMoney.Execute(withdrawalAccountId, 400m);
            var withdrawalAccountBalance = accountRepository.GetAccountById(withdrawalAccountId).Balance;
            //Assert
            Assert.Equal(100m, withdrawalAccountBalance);
        }
        [Fact]

        public void ThrowExceptionIfNegativeWithdrawalAttempted()
        {
            //Arrange
            var withdrawMoney = SetupWithdrawMoney(500m);
            //Act
            Action action = () => withdrawMoney.Execute(withdrawalAccountId, -300m);
            //Assert
            Assert.Throws<InvalidOperationException>(action);
        }
    }
}
