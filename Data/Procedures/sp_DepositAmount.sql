CREATE PROCEDURE sp_DepositAmount
    @UserId INT,
    @Amount DECIMAL(18,2),
    @AccountNumber CHAR(12),
    @DateOfTransaction DATETIME,
    @EndBalance DECIMAL(18,2) OUTPUT
AS
BEGIN
    DECLARE @RowVersion ROWVERSION

    SELECT @EndBalance = Balance, @RowVersion = AccountRowVersion FROM Users WHERE Id = @UserId

    SET @EndBalance = @EndBalance + @Amount

    UPDATE Users SET Balance = @EndBalance WHERE Id = @UserId AND AccountRowVersion = @RowVersion

    IF @@ROWCOUNT = 0
        THROW 50000, 'Concurrency conflict: The data has been modified by someone else. Please retry the operation.', 1

    INSERT INTO Transactions (TransactionType, Amount, AccountNumberFrom, DateOfTransaction, EndBalance, UserId) 
    VALUES ('Deposit', @Amount, @AccountNumber, @DateOfTransaction, @EndBalance, @UserId)
END
