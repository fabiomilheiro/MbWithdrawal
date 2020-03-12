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
            account1 = new Account
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
            account2 = new Account
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
            accountRepositoryMock = new Mock<IAccountRepository>();
            accountRepositoryMock
                .Setup(r => r.GetAccountById(account1.Id))
                .Returns(account1);
            accountRepositoryMock
                .Setup(r => r.GetAccountById(account2.Id))
                .Returns(account2);

            accountRepositoryMock.Setup(r => r.Update(account1)).Callback<Account>(acc => updatedAccount1 = acc);
            accountRepositoryMock.Setup(r => r.Update(account2)).Callback<Account>(acc => updatedAccount2 = acc);

            notificationServiceMock = new Mock<INotificationService>();

            sut = new TransferMoney(
                accountRepositoryMock.Object,
                notificationServiceMock.Object);
        }

        [Fact]
        public void Execute_NewBalanceIsNegative_Throws()
        {
            Action action = () => sut.Execute(account1.Id, account2.Id, account1.Balance + 1);

            action.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds*");
        }

        [Fact]
        public void Execute_NewBalanceLessThan500_SendsNotification()
        {
            account1.Balance = 1000;

            sut.Execute(account1.Id, account2.Id, 501);

            notificationServiceMock.Verify(s => s.NotifyFundsLow(account1.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_PayInLimitReached_Throws()
        {
            account2.PaidIn = Account.PayInLimit - 100;

            Action action = ()=> sut.Execute(account1.Id, account2.Id, 101);

            action.Should().Throw<InvalidOperationException>().WithMessage("Account pay in limit reached*");
        }

        [Fact]
        public void Execute_PayInToLimitDifferenceLessThan500_SendsNotification()
        {
            account2.PaidIn = Account.PayInLimit - 1000;

            sut.Execute(account1.Id, account2.Id, 501);

            notificationServiceMock.Verify(s => s.NotifyApproachingPayInLimit(account2.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_ValidationsPassed_UpdatesBothAccounts()
        {
            sut.Execute(account1.Id, account2.Id, 10);

            updatedAccount1.Should().BeEquivalentTo(new Account
            {
                Id = account1.Id,
                Balance = 990,
                PaidIn = 0,
                Withdrawn = -10,
                User = new User
                {
                    Id = account1.User.Id,
                    Email = "john.smith@test.com",
                    Name = "John Smith"
                }
            });

            updatedAccount2.Should().BeEquivalentTo(new Account
            {
                Id = account2.Id,
                Balance = 5010,
                PaidIn = 10,
                Withdrawn = 0,
                User = new User
                {
                    Id = account2.User.Id,
                    Email = "mary.smith@test.com",
                    Name = "Mary Smith"
                }
            });
        }
    }
}