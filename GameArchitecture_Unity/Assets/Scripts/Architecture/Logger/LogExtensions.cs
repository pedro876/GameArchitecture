using Architecture.ObserverGroup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture.Logging;

/// <summary>
/// This log extensions allow every Unity Object to log messages in a handled way. Logging this way will invoke events
/// that the LogDisplay observes and uses in order to show logs on the game screen.
/// </summary>
public static class LogExtensions
{
    private const string NORMAL_LOG_COLOR = "#FFFFFF";
    private const string WARNING_LOG_COLOR = "#DDD54F";
    private const string ERROR_LOG_COLOR = "#E43539";
    private const string SUCCESS_LOG_COLOR = "#3BB54A";

    public static void Log(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        msg = WrapMsgWithColor(msg, NORMAL_LOG_COLOR);
        Debug.Log(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.Log, msg);
    }

    public static void LogWarning(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        msg = WrapMsgWithColor(msg, WARNING_LOG_COLOR);
        Debug.LogWarning(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.LogWarning, msg);
    }

    public static void LogError(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        msg = WrapMsgWithColor(msg, ERROR_LOG_COLOR);
        Debug.LogError(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.LogError, msg);
    }

    public static void LogSuccess(this Object obj, params object[] msgs)
    {
        string msg = BuildMsg(obj, msgs);
        msg = WrapMsgWithColor(msg, SUCCESS_LOG_COLOR);
        Debug.Log(msg, obj);
        ObserverGroup<LogEvents>.InvokeEvent(LogEvents.LogSuccess, msg);
    }

    private static string BuildMsg(Object obj, object[] msgs)
    {
        string joinedMsg = string.Join("; ", msgs);
        string finalMsg = $"{obj.name}: {joinedMsg}";
        return finalMsg;
    }

    private static string WrapMsgWithColor(string msg, string hexColor)
    {
        return $"<color={hexColor}>{msg}</color>";
    }
}