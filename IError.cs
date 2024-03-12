namespace Cherry
{
    public interface IError
    {
        int Code { get; }
        string Message { get; }
    }
}