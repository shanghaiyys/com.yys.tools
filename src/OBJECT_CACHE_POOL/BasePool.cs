/*
 * 作者:杨榆松
 * 日期：2017年4月14日
 * 注释：对象缓存池基类
 *       为什么要用对象缓存池：
 *       
 *       为了节省为每次对象都实例化,我们可以缓存并重用一些创建好的对象并通
 *       过节省为每次创建对象的时间和资源来大幅度提高程序性能
 *       
 * 
 *       为什么使用锁：
 *       当需要向对象池中添加或者移除一个对象时必须使用锁，
 *       由于这个过程LOCKED 和 UN_LOCKED 哈希表的内容会发生变化而我们不想在这个过程中发生冲突。
 * 
 */



using System;
using System.Timers;
using System.Collections;

namespace OBJECT_CACHE_POOL
{
    //对象缓存池抽象基类
    public abstract class BasePool
    {

        //获取数据库对象时的时间点
        private long LAST_CHECKOUT;

        //存储正在使用的连接对象
        private static Hashtable LOCKED;

        //存储未使用的连接对象,随时被使用
        private static Hashtable UN_LOCKED;

        //到期时间点(90秒)
        internal static long GARBAGE_INTERVAL = 90 * 1000;

        static BasePool()
        {
            //实例化安全线程对象
            LOCKED = Hashtable.Synchronized(new Hashtable());
            UN_LOCKED = Hashtable.Synchronized(new Hashtable());
        }

        internal BasePool()
        {
            //获取当前时间点
            LAST_CHECKOUT = DateTime.Now.Ticks;

            //时间轮询,剔除过期对象
            Timer DO_TIMER = new Timer();
            DO_TIMER.Enabled = true;
            DO_TIMER.Interval = GARBAGE_INTERVAL;
            DO_TIMER.Elapsed += new ElapsedEventHandler(CollectGarbage);
        }

        /// <summary>
        /// 时间轮询时间,剔除过期对象
        /// </summary>
        /// <param name="SENDER"></param>
        /// <param name="EA"></param>
        private void CollectGarbage(object SENDER, ElapsedEventArgs EA)
        {
            lock (this)
            {
                //获取当前时间点
                long NOW = DateTime.Now.Ticks;
                object OBJ;
                IDictionaryEnumerator IDE = UN_LOCKED.GetEnumerator();

                try
                {
                    //遍历待使用对象集合,过期的对象移除
                    while (IDE.MoveNext())
                    {
                        OBJ = IDE.Key;
                        if ((NOW - (long)UN_LOCKED[OBJ]) > GARBAGE_INTERVAL)
                        {
                            UN_LOCKED.Remove(OBJ);
                            Expire(OBJ);
                            OBJ = null;
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 从缓存池里获取对象
        /// </summary>
        /// <returns></returns>
        internal object GetObjectFromPool()
        {
            LAST_CHECKOUT = DateTime.Now.Ticks;
            object OBJ = null;

            lock (this)
            {
                try
                {
                    foreach (DictionaryEntry ITEM in UN_LOCKED)
                    {
                        OBJ = ITEM.Key;
                        UN_LOCKED.Remove(OBJ);
                        if (Validate(OBJ))
                        {
                            LOCKED.Add(OBJ, LAST_CHECKOUT);
                            return OBJ;
                        }
                        else
                        {
                            Expire(OBJ);
                            OBJ = null;
                        }
                    }
                }
                catch (Exception) { }

                OBJ = Create();
                LOCKED.Add(OBJ, LAST_CHECKOUT);
            }

            return OBJ;
        }

        /// <summary>
        /// 把用完的对象还给缓存池
        /// </summary>
        /// <param name="OBJ"></param>
        internal void ReturnObjectToPool(object OBJ)
        {
            if (null != OBJ)
            {
                LAST_CHECKOUT = DateTime.Now.Ticks;
                lock (this)
                {
                    LOCKED.Remove(OBJ);
                    UN_LOCKED.Add(OBJ, LAST_CHECKOUT);
                }
            }
        }

        /// <summary>
        /// 创建对象(子类重载实现)
        /// </summary>
        /// <returns></returns>
        protected abstract object Create();
        /// <summary>
        /// 验证对象(子类重载实现)
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected abstract bool Validate(object o);
        /// <summary>
        /// 释放对象(子类重载实现)
        /// </summary>
        /// <param name="o"></param>
        protected abstract void Expire(object o);
    }
}
