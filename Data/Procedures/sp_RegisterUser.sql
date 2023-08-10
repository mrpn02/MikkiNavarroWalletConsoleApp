CREATE PROCEDURE sp_RegisterUser
    @LoginName NVARCHAR(50),
    @AccountNumber CHAR(12),
    @PasswordHash VARBINARY(64),
    @PasswordSalt VARBINARY(32),
    @Balance DECIMAL(18,2),
    @RegisterDate DATETIME
AS
BEGIN
    INSERT INTO Users (LoginName, AccountNumber, PasswordHash, PasswordSalt, Balance, RegisterDate)
    VALUES (@LoginName, @AccountNumber, @PasswordHash, @PasswordSalt, @Balance, @RegisterDate);
END;
