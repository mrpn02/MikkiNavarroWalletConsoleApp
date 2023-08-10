using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Configuration;
using System.Text;

namespace MikkiNavarroWalletConsoleApp
{
    class Program
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["walletAppDBConnection"].ConnectionString;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("0. Exit");
                Console.WriteLine("-------------------");
                Console.Write("Option: "); ;
                string? option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Register();
                        break;
                    case "2":
                        Login();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static void Register()
        {
            Console.Write("Enter login name: ");
            string loginName = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            // Generate a random salt
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Use PBKDF2 to derive a key from the password
            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                hash = pbkdf2.GetBytes(32); // 32 bytes for the derived key
            }

            using (SqlConnection _sqlConnection = new SqlConnection(connectionString))
            {
                _sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("sp_RegisterUser", _sqlConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@LoginName", SqlDbType.NVarChar).Value = loginName;
                    cmd.Parameters.Add("@AccountNumber", SqlDbType.Char).Value = GenerateAccountNumber(_sqlConnection);
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary).Value = hash; // Storing hash
                    cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary).Value = salt; // Storing salt
                    cmd.Parameters.Add("@Balance", SqlDbType.Decimal).Value = 0.0m;
                    cmd.Parameters.Add("@RegisterDate", SqlDbType.DateTime).Value = DateTime.Now;

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Registration successful!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: " + e.Message);
                    }
                }
            }
        }


        static void Login()
        {
            Console.WriteLine("Enter login name:");
            string loginName = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("sp_Login", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@LoginName", SqlDbType.NVarChar).Value = loginName;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32(0);
                            string userName = reader.GetString(1);
                            string accountNumber = reader.GetString(2);
                            byte[] storedHash = (byte[])reader["PasswordHash"];
                            byte[] salt = (byte[])reader["PasswordSalt"];
                            decimal balance = reader.GetDecimal(5);

                            // Hash the input password with the stored salt
                            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                            byte[] hash = pbkdf2.GetBytes(32);

                            // Compare the computed hash with the stored hash
                            if (hash.SequenceEqual(storedHash))
                            {
                                Console.WriteLine($"Welcome, {userName}!");
                                UserMenu(userId, accountNumber, balance);
                            }
                            else
                            {
                                Console.WriteLine("Invalid login.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid login.");
                        }
                    }
                }
            }
        }



        static void UserMenu(int userId, string accountNumber, decimal balance)
        {
            while (true)
            {
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Deposit");
                Console.WriteLine("2. Withdraw");
                Console.WriteLine("3. Transfer");
                Console.WriteLine("4. View Transaction History");
                Console.WriteLine("0. Logout");
                Console.WriteLine("----------------------------------");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        balance = Deposit(userId, accountNumber, balance);
                        break;
                    case "2":
                        balance = Withdraw(userId, accountNumber);
                        break;
                    case "3":
                        Console.WriteLine("Enter the recipient's account number:");
                        string accountNumberTo = Console.ReadLine();
                        Console.WriteLine("Enter the amount to transfer:");
                        decimal amount;
                        if (decimal.TryParse(Console.ReadLine(), out amount))
                        {
                            Console.WriteLine("Enter recipient's User ID (you must know this):");
                            if (int.TryParse(Console.ReadLine(), out int userIdTo))
                            {
                                Transfer(accountNumber, accountNumberTo, amount, userId, userIdTo);
                                // Assuming you also retrieve the updated balance after transferring
                                //balance = GetBalance(userId, accountNumber);
                            }
                            else
                            {
                                Console.WriteLine("Invalid User ID.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                        }
                        break;
                    case "4":
                        ViewTransactionHistory(userId);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }


        // Deposit, Withdraw, Transfer, ViewTransactionHistory methods go here (similar structure as above)

        static string GenerateAccountNumber(SqlConnection connection)
        {
            string accountNumber;
            bool isUnique;

            do
            {
                // Generate a random 12-digit number
                Random random = new Random();
                long randomNumber = (long)(Math.Pow(10, 11) * (1 + random.Next(0, 9)) + Math.Pow(10, 10) * random.Next(0, 10) + random.Next((int)Math.Pow(10, 10), (int)Math.Pow(10, 11) - 1));

                // Format as a 12-character string, preserving leading zeros
                accountNumber = randomNumber.ToString("000000000000");

                // Check if the generated account number is unique
                using (SqlCommand cmd = new SqlCommand("sp_CheckUniqueAccountNumber", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AccountNumber", SqlDbType.Char, 12).Value = accountNumber;
                    SqlParameter isUniqueParam = cmd.Parameters.Add("@IsUnique", SqlDbType.Bit);
                    isUniqueParam.Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    isUnique = (bool)isUniqueParam.Value;
                }
            } while (!isUnique);

            return accountNumber;
        }


        static decimal Deposit(int userId, string accountNumber, decimal balance)
        {
            Console.WriteLine("Enter deposit amount:");
            decimal amount = decimal.Parse(Console.ReadLine());

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Call the stored procedure for depositing
                        using (SqlCommand cmd = new SqlCommand("sp_DepositAmount", connection, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
                            cmd.Parameters.Add("@AccountNumber", SqlDbType.Char).Value = accountNumber;
                            cmd.Parameters.Add("@DateOfTransaction", SqlDbType.DateTime).Value = DateTime.Now;

                            SqlParameter outParam = new SqlParameter("@EndBalance", SqlDbType.Decimal)
                            {
                                Direction = ParameterDirection.Output
                            };

                            cmd.Parameters.Add(outParam);

                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                throw new Exception("An error occurred during deposit.");
                            }

                            balance = (decimal)outParam.Value;
                        }

                        transaction.Commit();
                        Console.WriteLine($"Deposited {amount}. New balance: {balance}");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occurred: " + e.Message);
                    }
                }
            }

            return balance;
        }

        static decimal Withdraw(int userId, string accountNumber)
        {
            Console.WriteLine("Enter withdrawal amount:");
            decimal amount = decimal.Parse(Console.ReadLine());

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("sp_WithdrawFunds", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    cmd.Parameters.Add("@AccountNumber", SqlDbType.BigInt).Value = long.Parse(accountNumber);
                    cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    // Output parameter
                    SqlParameter newBalanceParam = new SqlParameter("@NewBalance", SqlDbType.Decimal);
                    newBalanceParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(newBalanceParam);

                    try
                    {
                        cmd.ExecuteNonQuery();

                        decimal newBalance = (decimal)newBalanceParam.Value;
                        Console.WriteLine($"Withdrawn {amount}. New balance: {newBalance}");
                        return newBalance;
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine("An error occurred: " + e.Message);
                        return 0; 
                    }
                }
            }
        }


        static void Transfer(string accountNumberFrom, string accountNumberTo, decimal amount, int userIdFrom, int userIdTo)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_TransferFunds", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@AccountNumberFrom", accountNumberFrom);
                    command.Parameters.AddWithValue("@AccountNumberTo", accountNumberTo);
                    command.Parameters.AddWithValue("@UserIdFrom", userIdFrom);
                    command.Parameters.AddWithValue("@UserIdTo", userIdTo);
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

                    connection.Open();
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
            }
        }

        static void ViewTransactionHistory(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetTransactionHistory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    Console.WriteLine("Transaction History:");
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine("{0,-15} {1,-10} {2,-15} {3,-15} {4,-20} {5,-15}", "Type", "Amount", "From", "To", "Date", "End Balance");

                    while (reader.Read())
                    {
                        Console.WriteLine("{0,-15} {1,-10} {2,-15} {3,-15} {4,-20} {5,-15}",
                            reader["TransactionType"],
                            reader["Amount"],
                            reader["AccountNumberFrom"],
                            reader["AccountNumberTo"],
                            reader["DateOfTransaction"],
                            reader["EndBalance"]);
                    }

                    reader.Close();
                }
            }
        }

    }
}
