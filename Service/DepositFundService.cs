using MikkiNavarroWalletConsoleApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MikkiNavarroWalletConsoleApp.DBHelper.DBHelper;


namespace MikkiNavarroWalletConsoleApp.Service
{
    public interface IDepositFundService
    {
        decimal Deposit(int userId, string accountNumber, decimal balance);
    }

    public class DepositFundService : IDepositFundService
    {
        //implement the IDatabase
        private readonly IDatabase _database;

        public DepositFundService(IDatabase database)
        {
            _database = database;
        }


        public decimal Deposit(int userId, string accountNumber, decimal balance)
        {
            throw new NotImplementedException();
        }
    }
}
