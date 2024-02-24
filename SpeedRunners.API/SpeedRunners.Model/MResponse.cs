namespace SpeedRunners.Model
{
    public class MResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }

        public static MResponse Success(string msg = "成功") => new MResponse
        {
            Code = 666,
            Message = msg
        };

        public static MResponse Fail(string msg = "失败", int code = -1) => new MResponse
        {
            Code = code,
            Message = msg
        };
    }

    public class MResponse<T> : MResponse
    {
        public T Data { get; set; }
    }

    public static class MResponseMethod
    {
        public static MResponse<T> Success<T>(this T data, string msg = "成功")
        {
            return new MResponse<T>
            {
                Code = 666,
                Message = msg,
                Data = data,
            };
        }
    }
}
