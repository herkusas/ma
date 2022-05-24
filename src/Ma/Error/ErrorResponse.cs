namespace Ma.Error;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
public class ErrorResponse
{
    public ErrorResponse(string message)
    {
        Message = message;
    }

    public int Code { get; set; }
    public string Message { get; }

    public static ErrorResponse Error401 { get; } = new ErrorResponse401();
    public static ErrorResponse Error403 { get; } = new ErrorResponse403();

    private class ErrorResponse401 : ErrorResponse
    {
        public ErrorResponse401() : base("Unauthorized")
        {
            Code = 401;
        }
    }

    private class ErrorResponse403 : ErrorResponse
    {
        public ErrorResponse403() : base("Forbidden")
        {
            Code = 403;
        }
    }
}
