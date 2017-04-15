using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace OBJECT_CACHE_POOL
{
    public class SqlServerConnectionPoolTest
    {
        public void Test()
        {
            //实例化一个唯一的对象
            SqlServerConnectionPool POOL = SqlServerConnectionPool.SQL_POOL;
            //设置链接字符串
            SqlServerConnectionPool.SQL_CONN_STRING = "";
            //从缓存池里获取对象
            SqlConnection SQL_CONN = POOL.GetSqlConnFromPool();

            //记得,不用的时候,还给人家缓存池
            POOL.ReturnSqlConnToPool(SQL_CONN);

        }
    }

    public class OracleConnectionPoolTest
    {
        public void Test()
        {
            //实例化一个唯一的对象
            OracleConnectionPool POOL = OracleConnectionPool.ORACLE_POOL;
            //设置链接字符串
            OracleConnectionPool.ORACLE_CONN_STRING = "";
            //从缓存池里获取对象
            OracleConnection ORACLE_CONN = POOL.GetSqlConnFromPool();

            //记得,不用的时候,还给人家缓存池
            POOL.ReturnSqlConnToPool(ORACLE_CONN);

        }
    }
}
