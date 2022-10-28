public class NullLogger: Logger
{
    public static NullLogger Create()
    {
        return new NullLogger();
    }
    
    private NullLogger(){}
        
    public void Log(string message) { }

    public void LogWithData(string message, object obj) { }
}