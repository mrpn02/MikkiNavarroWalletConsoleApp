CREATE PROCEDURE sp_TransferFunds
    @Amount DECIMAL(18, 2),
    @AccountNumberFrom BIGINT,
    @AccountNumberTo BIGINT,
    @UserIdFrom INT,
    @UserIdTo INT,
    @NewBalanceFrom DECIMAL(18, 2) OUTPUT,
    @NewBalanceTo DECIMAL(18, 2) OUTPUT
AS
BEGIN
    BEGIN TRANSACTION;

    DECLARE @CurrentBalanceFrom DECIMAL(18, 2);
    DECLARE @CurrentBalanceTo DECIMAL(18, 2);

    SELECT @CurrentBalanceFrom = Balance FROM Users WHERE AccountNumber = @AccountNumberFrom;
    SELECT @CurrentBalanceTo = Balance FROM Users WHERE AccountNumber = @AccountNumberTo;

    IF @CurrentBalanceFrom >= @Amount
    BEGIN
        UPDATE Users
        SET Balance = @CurrentBalanceFrom - @Amount
        WHERE AccountNumber = @AccountNumberFrom;

        UPDATE Users
        SET Balance = @CurrentBalanceTo + @Amount
        WHERE AccountNumber = @AccountNumberTo;

        SELECT @NewBalanceFrom = Balance FROM Users WHERE AccountNumber = @AccountNumberFrom;
        SELECT @NewBalanceTo = Balance FROM Users WHERE AccountNumber = @AccountNumberTo;

        INSERT INTO Transactions (TransactionType, Amount, AccountNumberFrom, AccountNumberTo, DateOfTransaction, EndBalance, UserId)
        VALUES ('Transfer', @Amount, @AccountNumberFrom, @AccountNumberTo, GETDATE(), @NewBalanceFrom, @UserIdFrom);

        INSERT INTO Transactions (TransactionType, Amount, AccountNumberFrom, AccountNumberTo, DateOfTransaction, EndBalance, UserId)
        VALUES ('Transfer', @Amount, @AccountNumberFrom, @AccountNumberTo, GETDATE(), @NewBalanceTo, @UserIdTo);

        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Insufficient funds', 16, 1);
    END

END;
