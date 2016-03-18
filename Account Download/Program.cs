using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account_Download
{
    class Program
    {
        static void Main(string[] args)
        {
            Utilities.login();
            Utilities.updateAccountRecords();
            Utilities.updateContactRecords();
            Utilities.updateOpportunityRecords();
            Utilities.updateTaskRecords();
            Utilities.updateUserRecords();
        }
    }
}
