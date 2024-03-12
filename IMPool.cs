using System;
using System.Collections.Generic;

namespace Cherry
{
    public interface IMPool
    {
        /// <summary>
        ///     是否已经注册自定义标签(可以是类型全称字符串)的对象池
        /// </summary>
        /// <param name="tag">自定义标签</param>
        /// <returns></returns>
        bool HasPool(string tag);

        /// <summary>
        ///     是否已经注册类型全称对象池
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasPool(Type type);

        /// <summary>
        ///     是否已经注册泛型全称的对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasPool<T>();

        /// <summary>
        ///     按泛型获取对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPool GetPool<T>();

        /// <summary>
        ///     按类型获取对象池
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IPool GetPool(Type type);

        /// <summary>
        ///     按标签获取对象池
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        IPool GetPool(string tag);

        /// <summary>
        ///     获取所有key
        /// </summary>
        /// <returns></returns>
        List<string> GetPoolKeys();

        /// <summary>
        ///     使用自定义标签(可以是类型全称字符串)注册对象池,可以是任意唯一字符串,例如资源的路径
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="func">工厂方法</param>
        /// <param name="max">最大数量</param>
        IPool RegisterPool(string tag, Func<object> func, Action<object> disposer = null, int max = 10,
            int min = 0, IPoolHelper helper = null);

        void RegisterPool(string tag, IPool pool);

        /// <summary>
        ///     使用类型全称注册对象池
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="func">工厂方法,如果为空使用Activator.CreateInstance(type)</param>
        /// <param name="max">最大数量</param>
        IPool RegisterPool(Type type, Func<object> func = null, Action<object> disposer = null, int max = 20,
            int count = 5, IPoolHelper helper = null);

        void RegisterPool(Type type, IPool pool);

        /// <summary>
        ///     使用泛型全称注册对象池
        /// </summary>
        /// <param name="func">工厂方法,如果为空使用new T()</param>
        /// <param name="max">最大数量</param>
        /// <typeparam name="T"></typeparam>
        IPool RegisterPool<T>(Func<object> func = null, Action<object> disposer = null, int max = 20, int min = 5,
            IPoolHelper helper = null) where T : new();

        void RegisterPool<T>(IPool pool);

        /// <summary>
        ///     使用自定义标签(可以是类型全称字符串)生成对象
        /// </summary>
        /// <param name="tag">标签</param>
        /// <returns></returns>
        object SpawnInstance(string tag);

        /// <summary>
        ///     使用自定义标签(可以是类型全称字符串)生成对象,并转换为指定类型
        /// </summary>
        /// <param name="tag">自定义标签</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T SpawnInstance<T>(string tag);

        /// <summary>
        ///     使用类型全称生成对象
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        object SpawnInstance(Type type);

        /// <summary>
        ///     使用泛型全称生成对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns></returns>
        T SpawnInstance<T>() where T : new();

        /// <summary>
        ///     回收对象，只有SpawnObject多态方法生成的对象才能被回收
        /// </summary>
        /// <param name="obj">回收对象</param>
        void RecycleInstance(object obj);

        /// <summary>
        ///     使用自定义标签(可以是类型全称字符串)清空对象池
        /// </summary>
        /// <param name="tag"></param>
        void ClearPool(string tag);

        /// <summary>
        ///     使用类型全称清空对象池
        /// </summary>
        /// <param name="type"></param>
        void ClearPool(Type type);

        /// <summary>
        ///     使用泛型全称清空对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ClearPool<T>();

        /// <summary>
        ///     清空所有对象池
        /// </summary>
        void ClearPools();
    }
}