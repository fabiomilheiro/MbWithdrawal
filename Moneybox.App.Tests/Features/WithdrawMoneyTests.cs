using System;
using FluentAssertions;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using Xunit;

namespace Moneybox.App.Tests.Features
{
    public class WithdrawMoneyTests
    {
        private WithdrawMoney sut;
        private Mock<IAccountRepository> accountRepositoryMock;
        private Mock<INotificationService> notificationServiceMock;
        private Account account;
        private Account updatedAccount;

        public WithdrawMoneyTests()
        {
            this.account = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 1000,
                PaidIn = 0,
                Withdrawn = 0,
                User = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "john.smith@test.com",
                    Name = "John Smith"
                }
            };
            this.accountRepositoryMock = new Mock<IAccountRepository>();
            this.accountRepositoryMock
                .Setup(r => r.GetAccountById(this.account.Id))
                .Returns(this.account);

            this.accountRepositoryMock.Setup(r => r.Update(this.account)).Callback<Account>(acc => this.updatedAccount = acc);

            this.notificationServiceMock = new Mock<INotificationService>();

            this.sut = new WithdrawMoney(this.accountRepositoryMock.Object, this.notificationServiceMock.Object);
        }

        

        [Fact]
        public void Execute_NewBalanceIsNegative_Throws()
        {
            Action action = () => this.sut.Execute(this.account.Id, this.account.Balance + 1);

            action.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds*");
            this.accountRepositoryMock.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void Execute_NewBalanceLessThan500_SendsNotification()
        {
            this.account.Balance = 1000;

            this.sut.Execute(this.account.Id, 501);

            this.notificationServiceMock.Verify(s => s.NotifyFundsLow(this.account.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_ValidationsPassed_UpdateAccount()
        {
            this.sut.Execute(this.account.Id, 10);

            this.updatedAccount.Should().BeEquivalentTo(new Account
            {
                Id = this.account.Id,
                Balance = 990,
                PaidIn = 0,
                Withdrawn = -10,
                User = new User
                {
                    Id = this.account.User.Id,
                    Email = "john.smith@test.com",
                    Name = "John Smith"
                }
            });
        }
    }
}