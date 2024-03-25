using System.Data;

namespace LibraryApp.Infrastructure.Helper;

public class TransactionParam
{
    public IDbTransaction Transaction { get; set; } = default!;

    public IDbConnection Connection { get; set; } = default!;
}

