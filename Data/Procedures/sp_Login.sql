CREATE PROCEDURE sp_Login
    @LoginName NVARCHAR(50)
AS
BEGIN
    SELECT Id, LoginName, AccountNumber, PasswordHash, PasswordSalt, Balance
    FROM Users
    WHERE LoginName = @LoginName
END;
