CREATE PROCEDURE sp_TransferFunds
    @Amount DECIMAL(18, 2),
    @AccountNumberFrom BIGINT,
    @AccountNumberTo BIGINT,
    @UserIdFrom INT,
    @UserIdTo INT,
    @RowVersionFrom ROWVERSION,
    @RowVersionTo ROWVERSION,
    @NewBalanceFrom DECIMAL(18, 2) OUTPUT,
    @NewBalanceTo DECIMAL(18, 2) OUTPUT
AS
BEGIN
    BEGIN TRANSACTION;

    DECLARE @CurrentBalanceFrom DECIMAL(18, 2);
    DECLARE @CurrentRowVersionFrom ROWVERSION;
    
    DECLARE @CurrentBalanceTo DECIMAL(18, 2);
    DECLARE @CurrentRowVersionTo ROWVERSION;

    SELECT 
        @CurrentBalanceFrom = Balance, 
        @CurrentRowVersionFrom = AccountRowVersion 
    FROM Users WHERE AccountNumber = @AccountNumberFrom;

    SELECT 
        @CurrentBalanceTo = Balance, 
        @CurrentRowVersionTo = AccountRowVersion 
    FROM Users WHERE AccountNumber = @AccountNumberTo;

    IF @CurrentRowVersionFrom != @RowVersionFrom OR @CurrentRowVersionTo != @RowVersionTo
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Concurrency conflict detected. Transaction aborted.', 16, 1);
        RETURN;
    END

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
