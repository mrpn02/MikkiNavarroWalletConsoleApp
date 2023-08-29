using MikkiNavarroWalletConsoleApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MikkiNavarroWalletConsoleApp.DBHelper.DBHelper;

namespace MikkiNavarroWalletConsoleApp.Service
{
    public class FetchAccountInformationService : IFetchAccountInformation
    {
        private readonly IDatabase _database;

        public FetchAccountInformationService(IDatabase database)
        {
            _database = database;
        }

        public AccountInformation FetchByAccountNumber(string accountNumber)
        {
            try
            {
                using var command = _database.CreateCommand("SELECT * FROM Users WHERE AccountNumber = @AccountNumber", CommandType.Text);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                _database.Connection.Close();

                _database.Connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapToAccountInformation(reader);
                }

                throw new Exception("Account not found.");
            }
            finally
            {
                _database.Connection.Close();
            }
        }

        public AccountInformation FetchByUserId(int userId)
        {
            _database.Connection.Open();

            try
            {

                using var command = _database.CreateCommand("SELECT * FROM Users WHERE Id = @UserId", CommandType.Text);
                command.Parameters.AddWithValue("@UserId", userId);

                _database.Connection.Close();

                _database.Connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapToAccountInformation(reader);
                }

                throw new Exception("User not found.");
            }
            finally
            {
                _database.Connection.Close();
            }
        }

        private AccountInformation MapToAccountInformation(SqlDataReader reader)
        {
            return new AccountInformation
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                LoginName = reader.GetString(reader.GetOrdinal("LoginName")),
                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
                RegisterDate = reader.GetDateTime(reader.GetOrdinal("RegisterDate")),
                AccountRowVersion = reader["AccountRowVersion"] as byte[]
            };
        }
    }

}
