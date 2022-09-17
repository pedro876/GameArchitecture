using Architecture.ObserverGroup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture.Logging;

public static class LogExtensions
{
    public static void Log(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        Debug.Log(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.Log, msg);
    }

    public static void LogWarning(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        Debug.LogWarning(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.LogWarning, msg);
    }

    public static void LogError(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        Debug.LogError(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.LogError, msg);
    }

    public static void LogSuccess(this Object obj, params object[] msgs)
    {
        string msg = $"<color=green>{BuildMsg(obj, msgs)}</color>";
        Debug.Log(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.LogSuccess, msg);
    }

    private static string BuildMsg(Object obj, object[] msgs)
    {
        string joinedMsg = string.Join("; ", msgs);
        string finalMsg = $"Obj {obj.name}: {joinedMsg}";
        return finalMsg;
    }
}