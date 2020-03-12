using System;
using Moneybox.App.Domain.Services;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public void Withdraw(decimal amount)
        {
            var newBalance = this.Balance - amount;
            if (newBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            this.Balance -= newBalance;
            this.Withdrawn -= amount;
        }

        public void NotifyIfFundsLow(INotificationService notificationService)
        {
            if (this.Balance < 500m)
            {
                notificationService.NotifyFundsLow(this.User.Email);
            }
        }

        public void PayIn(decimal amount)
        {
            var paidIn = this.PaidIn + amount;

            if (paidIn > Account.PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            this.Balance += amount;
            this.PaidIn += amount;
        }

        public void NotifyIfPainLimitIsClose(INotificationService notificationService)
        {
            if (Account.PayInLimit - this.PaidIn < 500m)
            {
                notificationService.NotifyApproachingPayInLimit(this.User.Email);
            }
        }
    }
}
