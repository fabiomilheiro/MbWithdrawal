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

        public TransferMoneyTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            _sut = new TransferMoney(
                _accountRepositoryMock.Object,
                _notificationServiceMock.Object);
        }

        [Fact]
        public void Execute_NewBalanceIsNegative_Throws()
        {
            Assert.False(true);
        }

        [Fact]
        public void Execute_NewBalanceLessThan500_SendsNotification()
        {
            Assert.False(true);
        }

        [Fact]
        public void Execute_PayInLimitReached_Throws()
        {
            Assert.False(true);
        }

        [Fact]
        public void Execute_CloseToPayInLimit_SendsNotification()
        {
            Assert.False(true);
        }

        [Fact]
        public void Execute_ValidationsPassed_UpdatesBothAccounts()
        {
            Assert.False(true);
        }
    }
}