using System;
using Mirror;
using UnityEngine;

class DebugLogger:Logger
{
    public static DebugLogger Create()
    {
        return new DebugLogger();
    }
    
    private DebugLogger()
    {
    }
    
    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void LogWithData(string message, object obj)
    {
        var machine = string.Empty;
        if (obj.GetType() == typeof(NetworkBehaviour))
        {
            var networkObject = (NetworkBehaviour)obj;
            if (networkObject.isServer)
                machine = "Server";
            if (networkObject.isClient)
                machine = "Client";
        }

        message = string.Format("{0}: {1}: {2}", DateTime.Now, machine, message);
        Log(message);
    }
}