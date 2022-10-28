public interface Logger
{
    public void Log(string message);
    public void LogWithData(string message, object obj);
}