using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

public class SqlConnectionScope : IDisposable
{
    [ThreadStatic]
    private static int nivel;
    [ThreadStatic]
    private static SqlConnection _conexao;

    public SqlConnection CurrentConnection
    {
        get { return _conexao; }
    }

    public SqlConnectionScope(string connectionString)
    {
        if (nivel == 0)
        {
            _conexao = new SqlConnection(connectionString);
            _conexao.Open();
        }
        nivel++;

    }

    public void Dispose()
    {
        nivel--;
        if (nivel <= 0)
        {
            _conexao.Close();
            _conexao.Dispose();
            _conexao = null;
            nivel = 0;
        }
    }
}