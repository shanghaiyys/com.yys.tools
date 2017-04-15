/*
 * 
 * 作者：杨榆松
 * 日期：2017年4月14日
 * 注释：继承BasePool，实现sqlserver链接对象池
 * 
 * 
 * 为什么要使用单例模式？
 * 
 * Singleton 是一个著名的创建型设计模式，
 * 当你需要一个对象仅对应一个实例时通常需要使用它。
 * 设计模式一书(ISBN 0-201-70265-7)中对设计单例模式目的定义为保证一个类仅有一个实例，
 * 并提供全局唯一的方式来访问它。
 * 为了实现一个单例，我们需要一个私有构造函数以便于客户端应用程序无论如何都没法创建一个新对象，
 * 使用静态的只读属性来创建单例类的唯一实例。
 * .NET Framework 在JIT 过程中仅当有任何方法使用静态属性时才会将其实例化。
 * 如果属性没有被使用，那么也就不会创建实例。
 * 更准确地说，仅当有任何类/方法对类的静态成员进行调用时才会构造对应单例类的实例。
 * 这个特性称作惰性初始化并把创建对象的过程留给第一次访问实例属性的代码。
 * .NET Framework 保证共享类型初始化时的类型安全。
 * 所以我们不需要担心SqlServerConnectionPool对象的线程安全问题，
 * 因为在应用程序整个生命周期内创建一个实例。实例静态属性维护SqlServerConnectionPool类对象的唯一实例。
 * 
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace OBJECT_CACHE_POOL
{
    public sealed class SqlServerConnectionPool : BasePool
    {
        private SqlServerConnectionPool() { }

        public static readonly SqlServerConnectionPool SQL_POOL = new SqlServerConnectionPool();

        private static string _SQL_CONN_STRING = string.Empty;
        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        public static string SQL_CONN_STRING
        {
            get { return SqlServerConnectionPool._SQL_CONN_STRING; }
            set { SqlServerConnectionPool._SQL_CONN_STRING = value; }
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns></returns>
        protected override object Create()
        {
            SqlConnection SQL_CONN = new SqlConnection(_SQL_CONN_STRING);
            return SQL_CONN;
        }
        /// <summary>
        /// 验证对象
        /// </summary>
        /// <param name="OBJ"></param>
        /// <returns></returns>
        protected override bool Validate(object OBJ)
        {
            try
            {
                SqlConnection SQL_CONN = (SqlConnection)OBJ;
                return !SQL_CONN.State.Equals(ConnectionState.Closed);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="OBJ"></param>
        protected override void Expire(object OBJ)
        {
            try
            {
                SqlConnection SQL_CONN = (SqlConnection)OBJ;
                SQL_CONN.Close();
                SQL_CONN.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 缓存池中获取sql链接对象
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetSqlConnFromPool()
        {
            try
            {
                return (SqlConnection)base.GetObjectFromPool();
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }
        /// <summary>
        /// 把用完的对象还给缓存池
        /// </summary>
        /// <param name="SQL_CONN"></param>
        public void ReturnSqlConnToPool(SqlConnection SQL_CONN)
        {
            base.ReturnObjectToPool(SQL_CONN);
        }

    }
}
