using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var withdrawalAccount = accountRepository.GetAccountById(fromAccountId);

            //Ive introduced a sequence dependency on these two methods, since we assume that validation has been called before 
            //WithdrawMoney.  
            //This isn't great, but considering we're using a UseCase style model (Feature) it is at least encapsulated in one method. 
            ValidateWithdrawMoney(withdrawalAccount, amount);
            withdrawalAccount.WithdrawMoney(amount);
        }

        private void ValidateWithdrawMoney(Account from, decimal amount)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("You cannot make a negative value withdrawal");
            }

            if (from.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds to make withdrawal");
            }

            if ((from.Balance - amount) < Account.NotifyFundsLowThreshold)
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }
        }
    }
}
