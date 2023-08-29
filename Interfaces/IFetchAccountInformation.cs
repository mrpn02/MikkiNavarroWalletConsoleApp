using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikkiNavarroWalletConsoleApp.Interfaces
{
    public interface IFetchAccountInformation
    {
        AccountInformation FetchByAccountNumber(string accountNumber);
        AccountInformation FetchByUserId(int userId);
    }

    public class AccountInformation
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public DateTime RegisterDate { get; set; }
        public byte[] AccountRowVersion { get; set; }
    }
}
