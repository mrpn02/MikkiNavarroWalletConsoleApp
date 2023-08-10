CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LoginName NVARCHAR(50) UNIQUE,
    AccountNumber CHAR(12) UNIQUE NOT NULL,
    PasswordHash VARBINARY(64),
    PasswordSalt VARBINARY(32),
    Balance DECIMAL(18,2),
    RegisterDate DATETIME,
    AccountRowVersion ROWVERSION
);

CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TransactionType NVARCHAR(10),
    Amount DECIMAL(18,2),
    AccountNumberFrom BIGINT,
    AccountNumberTo BIGINT,
    DateOfTransaction DATETIME,
    EndBalance DECIMAL(18,2),
    UserId INT FOREIGN KEY REFERENCES Users(Id)
);
