using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal NotifyFundsLowThreshold = 500m;
        public const decimal NotifyApproachingPayInLimitThreshold = 500m;

        public Account()
        {

        }

        public Account(decimal balance)
        {
            Balance = balance;
        }

        public Guid Id { get; set; }

        public User User { get; set; }

         /*
         * Changed to a private setter for better encapsulation. Added a parameterised constructor for use on newly created objects only
         * to set balance of a new account. Existing accounts would load the balance from the data store and then increment/decrement via methods
         * such as SendMoneyToAccount, ReceiveMoney and WithdrawMoney.
         * */
        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }

        public decimal PaidIn { get; private set; }

        public void SendMoneyToAccount(decimal amount, Account destinationAccount, INotificationService notificationService)
        {
            Balance -= amount;
            Withdrawn += amount;
            destinationAccount.ReceiveMoney(amount, this);
        }

        public void ReceiveMoney(decimal amount, Account originatingAccount)
        {
            this.Balance += amount;
            this.PaidIn += amount;
        }

        public void WithdrawMoney(decimal amount)
        {
            this.Balance -= amount;
            this.Withdrawn += amount;
            // I have made the assumption that a withdrawal does not decrement the PaidIn amount. 
        }
    }
}
