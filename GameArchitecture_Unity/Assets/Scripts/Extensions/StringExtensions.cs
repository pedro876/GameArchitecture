using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
    private const string successPrefix = "#s ";

    public static string ColorWrap(this string str, in string hexColor)
    {
        return $"<color={hexColor}>{str}</color>";
    }

    public static string ColorWrap(this string str, in Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        return $"<color={hexColor}>{str}</color>";
    }

    public static string AddTimestampPrefix(this string str, in string format = "HH:mm:ss")
    {
        return $"[{DateTime.Now.ToString(format)}] {str}";
    }

    public static string AddSuccessPrefix(this string str)
    {
        return $"{successPrefix}{str}";
    }

    public static bool HasSuccessPrefix(this string str)
    {
        return str.StartsWith(successPrefix);
    }

    public static string RemoveSuccessPrefix(this string str)
    {
        return str.Replace(successPrefix, "");
    }
}
