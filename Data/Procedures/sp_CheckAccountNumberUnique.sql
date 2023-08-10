CREATE PROCEDURE sp_CheckAccountNumberUnique
    @AccountNumber CHAR(12),
    @IsUnique BIT OUTPUT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE AccountNumber = @AccountNumber)
        SET @IsUnique = 0
    ELSE
        SET @IsUnique = 1
END
