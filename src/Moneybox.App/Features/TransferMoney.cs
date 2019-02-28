using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);
            var to = accountRepository.GetAccountById(toAccountId);

            //Ive introduced a sequence dependency on these two methods, since we assume that validation has been called before 
            //from.SentMoneyToAccount. 
            //This isn't great, but considering we're using a UseCase style model (Feature) it is at least encapsulated in one method. 
            ValidateTransferMoney(from, to, amount);
            from.SendMoneyToAccount(amount, to, notificationService);
            
            // This should be refactored to support a transaction so that the  operation is atomic. 
            accountRepository.Update(from);
            accountRepository.Update(to);
        }

        private void ValidateTransferMoney(Account from, Account to, decimal amount)
        {

            if(amount < 0)
            {
                throw new InvalidOperationException("You cannot make a negative value money transfer");
            }

            if (from.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if ((from.Balance - amount) < Account.NotifyFundsLowThreshold)
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }

            var paidIn = to.PaidIn + amount;
            if (paidIn > Account.PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (Account.PayInLimit - paidIn < Account.NotifyApproachingPayInLimitThreshold)
            {
                notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }
        }
    }
}
