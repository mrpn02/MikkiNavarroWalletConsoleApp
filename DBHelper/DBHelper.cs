using MikkiNavarroWalletConsoleApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikkiNavarroWalletConsoleApp.DBHelper
{
    public class DBHelper
    {
        public class Database : IDatabase
        {
            public SqlConnection Connection { get; }

            public Database(string connectionString)
            {
                Connection = new SqlConnection(connectionString);
                Connection.Open();
            }

            public SqlCommand CreateCommand(string commandText, CommandType commandType)
            {
                var command = new SqlCommand(commandText, Connection);
                command.CommandType = commandType;
                return command;
            }
        }

        public class TransactionalDatabase : Database, ITransaction
        {
            public SqlTransaction Transaction { get; private set; }

            public TransactionalDatabase(string connectionString) : base(connectionString) { }

            public SqlTransaction BeginTransaction()
            {
                Transaction = Connection.BeginTransaction();
                return Transaction;
            }

            public void Commit()
            {
                Transaction.Commit();
            }

            public void Rollback()
            {
                Transaction.Rollback();
            }
        }
    }
}
