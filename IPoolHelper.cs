namespace Cherry
{
    public interface IPoolHelper<T> : IObjectHelper<T>
    {
        /// <summary>
        ///     是否初始化过
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        ///     通过对象保存默认数据
        /// </summary>
        /// <param name="obj"></param>
        void Init(T obj);
    }

    public interface IPoolHelper : IPoolHelper<object>
    {
    }
}