using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikkiNavarroWalletConsoleApp.Interfaces
{
    public interface IDatabase
    {
        SqlConnection Connection { get; }
        SqlCommand CreateCommand(string commandText, CommandType commandType);
    }

    public interface ITransaction : IDatabase
    {
        SqlTransaction BeginTransaction();
        void Commit();
        void Rollback();
    }
}
