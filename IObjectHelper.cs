namespace Cherry
{
    public interface IObjectHelper<T>
    {
        /// <summary>
        ///     使用初始化的默认数据重置对象
        /// </summary>
        /// <param name="obj"></param>
        void Set(T obj);
    }

    public interface IObjectHelper : IObjectHelper<object>
    {
    }
}