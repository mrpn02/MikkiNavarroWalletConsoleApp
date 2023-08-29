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
    public interface IDepositFundService
    {
        decimal Deposit(int userId, string accountNumber);
    }

    public class DepositFundService : IDepositFundService
    {
        private readonly ITransaction _transaction;

        public DepositFundService(ITransaction transaction)
        {
            _transaction = transaction;
        }

        public decimal Deposit(int userId, string accountNumber)
        {
            Console.WriteLine("Enter deposit amount:");

            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid deposit amount entered.");
                return 0; // Return 0 if the deposit amount is invalid.
            }

            using var cmd = _transaction.CreateCommand("sp_DepositAmount", CommandType.StoredProcedure);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
            cmd.Parameters.Add("@AccountNumber", SqlDbType.Char).Value = accountNumber;
            cmd.Parameters.Add("@DateOfTransaction", SqlDbType.DateTime).Value = DateTime.Now;

            SqlParameter endBalanceParam = new SqlParameter("@EndBalance", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(endBalanceParam);

            try
            {
                cmd.ExecuteNonQuery();
                decimal endBalance = (decimal)endBalanceParam.Value;

                Console.WriteLine($"Deposited {amount}. New balance: {endBalance}");
                _transaction.Commit();
                return endBalance;
            }
            catch (SqlException ex)
            {
                // Here, you can check for specific SQL error numbers to provide more tailored error messages.
                if (ex.Number == 50000 && ex.Message.Contains("Concurrency conflict"))
                {
                    Console.WriteLine("Concurrency conflict detected. Please retry the operation.");
                }
                else
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }

                _transaction.Rollback();
                return 0; // Return 0 if an error occurs during deposit.
            }
            finally
            {
                _transaction.Connection.Close();
            }
        }
    }
}
