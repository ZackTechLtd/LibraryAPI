
namespace DataAccess
{
    using System.Data;

    public class TransactionParam
    {
        public IDbTransaction Transaction { get; set; }

        public IDbConnection Connection { get; set; }
    }
}
