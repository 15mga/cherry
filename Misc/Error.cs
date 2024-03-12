namespace Cherry.Misc
{
    public class Error : IError
    {
        public Error(int code, string message)
        {
            Message = message;
            Code = code;
        }

        public Error(string message)
        {
            Message = message;
        }

        public Error(string message, params object[] args)
        {
            Message = string.Format(message, args);
        }

        public Error(int code, string message, params object[] args)
        {
            Code = code;
            Message = string.Format(message, args);
        }

        public int Code { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code > 0 ? $"{Code}:{Message}" : $"{Message}";
        }
    }
}