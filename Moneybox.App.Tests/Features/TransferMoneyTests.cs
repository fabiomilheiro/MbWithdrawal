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
        private TransferMoney _sut;
        private Mock<IAccountRepository> _accountRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private Account _account1, _account2;
        private Account _updatedAccount1, _updatedAccount2;

        public TransferMoneyTests()
        {
            _account1 = new Account
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
            _account2 = new Account
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
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _accountRepositoryMock
                .Setup(r => r.GetAccountById(_account1.Id))
                .Returns(_account1);
            _accountRepositoryMock
                .Setup(r => r.GetAccountById(_account2.Id))
                .Returns(_account2);

            _accountRepositoryMock.Setup(r => r.Update(_account1)).Callback<Account>(acc => _updatedAccount1 = acc);
            _accountRepositoryMock.Setup(r => r.Update(_account2)).Callback<Account>(acc => _updatedAccount2 = acc);

            _notificationServiceMock = new Mock<INotificationService>();

            _sut = new TransferMoney(
                _accountRepositoryMock.Object,
                _notificationServiceMock.Object);
        }

        [Fact]
        public void Execute_NewBalanceIsNegative_Throws()
        {
            Action action = () => _sut.Execute(_account1.Id, _account2.Id, _account1.Balance + 1);

            action.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds*");
        }

        [Fact]
        public void Execute_NewBalanceLessThan500_SendsNotification()
        {
            _account1.Balance = 1000;

            _sut.Execute(_account1.Id, _account2.Id, 501);

            _notificationServiceMock.Verify(s => s.NotifyFundsLow(_account1.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_PayInLimitReached_Throws()
        {
            _account2.PaidIn = Account.PayInLimit - 100;

            Action action = ()=> _sut.Execute(_account1.Id, _account2.Id, 101);

            action.Should().Throw<InvalidOperationException>().WithMessage("Account pay in limit reached*");
        }

        [Fact]
        public void Execute_PayInToLimitDifferenceLessThan500_SendsNotification()
        {
            _account2.PaidIn = Account.PayInLimit - 1000;

            _sut.Execute(_account1.Id, _account2.Id, 501);

            _notificationServiceMock.Verify(s => s.NotifyApproachingPayInLimit(_account2.User.Email), Times.Once);
        }

        [Fact]
        public void Execute_ValidationsPassed_UpdatesBothAccounts()
        {
            _sut.Execute(_account1.Id, _account2.Id, 10);

            _updatedAccount1.Should().BeEquivalentTo(new Account
            {
                Id = _account1.Id,
                Balance = 990,
                PaidIn = 0,
                Withdrawn = -10,
                User = new User
                {
                    Id = _account1.User.Id,
                    Email = "john.smith@test.com",
                    Name = "John Smith"
                }
            });

            _updatedAccount2.Should().BeEquivalentTo(new Account
            {
                Id = _account2.Id,
                Balance = 5010,
                PaidIn = 10,
                Withdrawn = 0,
                User = new User
                {
                    Id = _account2.User.Id,
                    Email = "mary.smith@test.com",
                    Name = "Mary Smith"
                }
            });
        }
    }
}