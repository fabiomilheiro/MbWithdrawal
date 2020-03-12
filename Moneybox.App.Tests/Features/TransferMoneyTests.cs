using System;
using FluentAssertions;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using Xunit;

namespace Moneybox.App.Tests.Features
{
    public class TransferMoneyTests
    {
        private TransferMoney sut;
        private Mock<IAccountRepository> accountRepositoryMock;
        private Mock<INotificationService> notificationServiceMock;
        private Account account1, account2;
        private Account updatedAccount1, updatedAccount2;

        public TransferMoneyTests()
        {
            this.account1 = new Account
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
            this.account2 = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 5000,
                PaidIn = 0,
                Withdrawn = 0,
                User = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "mary.smith@test.com",
                    Name = "Mary Smith"
                }
            };
            this.accountRepositoryMock = new Mock<IAccountRepository>();
            this.accountRepositoryMock
                .Setup(r => r.GetAccountById(this.account1.Id))
                .Returns(this.account1);
            this.accountRepositoryMock
                .Setup(r => r.GetAccountById(this.account2.Id))
                .Returns(this.account2);

            this.accountRepositoryMock.Setup(r => r.Update(this.account1)).Callback<Account>(acc => this.updatedAccount1 = acc);
            this.accountRepositoryMock.Setup(r => r.Update(this.account2)).Callback<Account>(acc => this.updatedAccount2 = acc);

            this.notificationServiceMock = new Mock<INotificationService>();

            this.sut = new TransferMoney(this.accountRepositoryMock.Object, this.notificationServiceMock.Object);
        }

        [Fact]
        public void Execute_NewBalanceIsNegative_Throws()
        {
            Action action = () => this.sut.Execute(this.account1.Id, this.account2.Id, this.account1.Balance + 1);

            action.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds*");
        }

        [Fact]
        public void Execute_NewBalanceLessThan500_SendsNotification()
        {
            this.account1.Balance = 1000;

            this.sut.Execute(this.account1.Id, this.account2.Id, 501);

            this.notificationServiceMock.Verify(s => s.NotifyFundsLow(this.account1.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_PayInLimitReached_Throws()
        {
            this.account2.PaidIn = Account.PayInLimit - 100;

            Action action = ()=> this.sut.Execute(this.account1.Id, this.account2.Id, 101);

            action.Should().Throw<InvalidOperationException>().WithMessage("Account pay in limit reached*");
        }

        [Fact]
        public void Execute_PayInToLimitDifferenceLessThan500_SendsNotification()
        {
            this.account2.PaidIn = Account.PayInLimit - 1000;

            this.sut.Execute(this.account1.Id, this.account2.Id, 501);

            this.notificationServiceMock.Verify(s => s.NotifyApproachingPayInLimit(this.account2.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_ValidationsPassed_UpdatesBothAccounts()
        {
            this.sut.Execute(this.account1.Id, this.account2.Id, 10);

            this.updatedAccount1.Should().BeEquivalentTo(new Account
            {
                Id = this.account1.Id,
                Balance = 990,
                PaidIn = 0,
                Withdrawn = -10,
                User = new User
                {
                    Id = this.account1.User.Id,
                    Email = "john.smith@test.com",
                    Name = "John Smith"
                }
            });

            this.updatedAccount2.Should().BeEquivalentTo(new Account
            {
                Id = this.account2.Id,
                Balance = 5010,
                PaidIn = 10,
                Withdrawn = 0,
                User = new User
                {
                    Id = this.account2.User.Id,
                    Email = "mary.smith@test.com",
                    Name = "Mary Smith"
                }
            });
        }
    }
}