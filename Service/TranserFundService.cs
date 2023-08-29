using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikkiNavarroWalletConsoleApp.Interfaces;

namespace MikkiNavarroWalletConsoleApp.Service
{
    public interface ITransferFundService
    {
        void Transfer(string accountNumberFrom, string accountNumberTo, decimal amount, int userIdFrom, int userIdTo, byte[] rowVersionFrom, byte[] rowVersionTo);
    }

    public class TransferFundService : ITransferFundService
    {
        private readonly ITransaction _database;

        public TransferFundService(ITransaction database)
        {
            _database = database;
        }

        /// <summary>
        /// Transfer with optimistic concurrency with checking for rowversion
        /// </summary>
        /// <param name="accountNumberFrom"></param>
        /// <param name="accountNumberTo"></param>
        /// <param name="amount"></param>
        /// <param name="userIdFrom"></param>
        /// <param name="userIdTo"></param>
        /// <param name="rowVersionFrom"></param>
        /// <param name="rowVersionTo"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Transfer(string accountNumberFrom, string accountNumberTo, decimal amount, int userIdFrom, int userIdTo, byte[] rowVersionFrom, byte[] rowVersionTo)
        {
            // Pre-transfer checks, don't allow negative or 0 transfers. Don't allow same account number transfers
            if (amount <= 0)
            {
                throw new ArgumentException("Amount to transfer should be greater than zero.");
            }

            if (string.Equals(accountNumberFrom, accountNumberTo))
            {
                throw new ArgumentException("Source and destination accounts cannot be the same.");
            }

            using var command = _database.CreateCommand("sp_TransferFunds", CommandType.StoredProcedure);

            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@AccountNumberFrom", accountNumberFrom);
            command.Parameters.AddWithValue("@AccountNumberTo", accountNumberTo);
            command.Parameters.AddWithValue("@UserIdFrom", userIdFrom);
            command.Parameters.AddWithValue("@UserIdTo", userIdTo);
            command.Parameters.AddWithValue("@RowVersionFrom", rowVersionFrom);
            command.Parameters.AddWithValue("@RowVersionTo", rowVersionTo);

            SqlParameter newBalanceFromParam = new SqlParameter
            {
                ParameterName = "@NewBalanceFrom",
                SqlDbType = SqlDbType.Decimal,
                Direction = ParameterDirection.Output
            };
            SqlParameter newBalanceToParam = new SqlParameter
            {
                ParameterName = "@NewBalanceTo",
                SqlDbType = SqlDbType.Decimal,
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(newBalanceFromParam);
            command.Parameters.Add(newBalanceToParam);

            try
            {
                command.ExecuteNonQuery();

                if (newBalanceFromParam.Value != DBNull.Value && newBalanceToParam.Value != DBNull.Value)
                {
                    // Handle success
                    Console.WriteLine($"Transaction Successful!");
                    Console.WriteLine($"Amount Transferred: {amount}");
                    Console.WriteLine($"From Account Number: {accountNumberFrom}");
                    Console.WriteLine($"To Account Number: {accountNumberTo}");
                }
                else
                {
                    throw new Exception("Transfer failed. Insufficient funds or an error occurred.");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000 && ex.Message.Contains("Concurrency conflict detected"))
                {
                    // Handle concurrency conflict
                    throw new InvalidOperationException("Concurrency conflict detected while transferring funds.");
                }
                throw;
            }
        }


    }

}
