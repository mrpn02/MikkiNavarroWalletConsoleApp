CREATE PROCEDURE sp_WithdrawFunds
    @AccountNumber BIGINT,
    @Amount DECIMAL(18, 2),
    @UserId INT,
    @NewBalance DECIMAL(18, 2) OUTPUT
AS
BEGIN
    BEGIN TRANSACTION;

    DECLARE @CurrentBalance DECIMAL(18, 2);

    SELECT @CurrentBalance = Balance FROM Users WHERE AccountNumber = @AccountNumber;

    IF @CurrentBalance >= @Amount
    BEGIN
        UPDATE Users
        SET Balance = @CurrentBalance - @Amount
        WHERE AccountNumber = @AccountNumber;

        SELECT @NewBalance = Balance FROM Users WHERE AccountNumber = @AccountNumber;

        INSERT INTO Transactions (TransactionType, Amount, AccountNumberFrom, AccountNumberTo, DateOfTransaction, EndBalance, UserId)
        VALUES ('Withdraw', @Amount, @AccountNumber, NULL, GETDATE(), @NewBalance, @UserId);

        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Insufficient funds', 16, 1);
    END

END;
