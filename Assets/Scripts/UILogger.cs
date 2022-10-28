using System;
using Mirror;
using UnityEngine;

class UILogger : Logger
{
    public static UILogger Create(bool debugLoggerOn = true)
    {
        return new UILogger(debugLoggerOn);
    }

    public UIController UIController
    {
        set { uiController = value; }
    }
    
    private bool debugLoggerOn;
    private UIController uiController;

    private UILogger(bool debugLoggerOn)
    {
        this.debugLoggerOn = debugLoggerOn;
    }
    public void Log(string message)
    {
        if (debugLoggerOn)
            Debug.Log(message);
        
        uiController?.DisplayLog(message);
    }
    
    public void LogWithData(string message, object obj)
    {
        var machine = string.Empty;
        if (obj is NetworkBehaviour)
        {
            var networkObject = (NetworkBehaviour)obj;
            if (networkObject.isServer)
                machine = machine + "Server ";
            if (networkObject.isClient)
                machine = machine + "Client";
        }

        message = string.Format("{0}: {1}: {2}", DateTime.Now, machine, message);
        Log(message);
    }
}