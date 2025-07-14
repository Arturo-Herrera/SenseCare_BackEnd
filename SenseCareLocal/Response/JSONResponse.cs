public class JSONResponse
{
    public int Status { get; set; }
    public string Message { get; set; }
    public MessageType MessageType { get; set; }
    public object? Data { get; set; }
}
