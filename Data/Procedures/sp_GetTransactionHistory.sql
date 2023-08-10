CREATE PROCEDURE sp_GetTransactionHistory
    @UserId INT
AS
BEGIN
    SELECT 
        TransactionType,
        Amount,
        AccountNumberFrom,
        AccountNumberTo,
        DateOfTransaction,
        EndBalance
    FROM Transactions
    WHERE UserId = @UserId
    ORDER BY DateOfTransaction DESC;
END;
