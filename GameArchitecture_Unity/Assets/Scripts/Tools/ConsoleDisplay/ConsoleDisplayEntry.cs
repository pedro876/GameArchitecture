using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleDisplayEntry : MonoBehaviour
{
    private Text _text;
    private Image _image;
    private LogType _logType;
    private bool _isSuccess;

    private void Awake()
    {
        _text = GetComponent<Text>();
        _image = GetComponentInChildren<Image>();
        _logType = LogType.Log;
    }

    public void SetText(string newText)
    {
        _text.text = newText;
    }

    public void SetType(LogType logType, Sprite icon, bool isSuccess = false)
    {
        _image.sprite = icon;
        _isSuccess = isSuccess;
        _logType = logType;
    }

    public void MoveToBottom()
    {
        transform.SetAsLastSibling();
    }
}
