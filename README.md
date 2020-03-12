# Moneybox Money Withdrawal

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* You should spend no more than 1 hour on this task, although there is no time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your work
* Your solution must compile and run first time
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Once you have completed test, zip up your solution, excluding any build artifacts to reduce the size, and email it back to our recruitment team.

Good luck!

## Notes

On github: https://github.com/fabiomilheiro/MoneyboxWithdrawal. You may see my approach by looking in the commits.

1. Created tests for `TransferMoneyTests` in order to ensure we can easily change the implementation and be confident that everything keeps working according to the specification.
2. Moved logic to the `Account` instance methods `Withdraw`, `NotifyIfFundsLow`, `PayIn` and `NotifyIfPaInLimitIsClose`.
3. Could have included the notification methods inside the `Withdraw` and `PaIn` methods but wasn't that would fit the desired business result.
Normally, I'd ask the business if we should the low balance notification without confirming if all validations had already passed.
4. After that, implementing the `WithdrawMoney.Execute` method using TDD approach was fast and easy.

If you have any questions, please let me know.

Thanks!
Fabio