namespace Masters.Shared.Error;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CS8618
public class ErrorResponse
{
    
    public int Code { get; set; }
    public string Message { get; set; }

    public static ErrorResponse Error401 { get; } = new ErrorResponse401();
    public static ErrorResponse Error403 { get; } = new ErrorResponse403();
    private class ErrorResponse401 : ErrorResponse
    {
        public ErrorResponse401()
        {
            Code = 401;
            Message = "Unauthorized";
        }

    }

    private class ErrorResponse403 : ErrorResponse
    {
        public ErrorResponse403()
        {
            Code = 403;
            Message = "Forbidden";
        }

    }
}
