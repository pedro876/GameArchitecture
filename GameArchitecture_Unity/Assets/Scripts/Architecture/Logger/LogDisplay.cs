using Architecture.ObserverGroup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Architecture.Services;

namespace Architecture.Logging
{
    public class LogDisplay : Observer<LogEvents>, ILogDisplay
    {
        [Header("Configuration")]
        [SerializeField] uint maxMessages = 100;
        [SerializeField] bool autoScroll = true;
        [Header("Colors")]
        [SerializeField] Color logColor = Color.white;
        [SerializeField] Color warningColor = Color.yellow;
        [SerializeField] Color errorColor = Color.red;
        [SerializeField] Color successColor = Color.green;

        private ScrollRect _scrollRect;
        private RectTransform _scrollTransform;
        private RectTransform _contentTransform;
        private Dictionary<Transform, Text> _texts;
        private int _messageCount = 0;

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);

        public void Show(float width)
        {
            ChangeWidth(width);
            Show();
        }

        public void ChangeWidth(float width)
        {
            var size = _scrollTransform.sizeDelta;
            size.x = width;
            _scrollTransform.sizeDelta = size;
        }

        public override void Notify(LogEvents evt, object evtInfo)
        {
            switch (evt)
            {
                default:
                    Log(evtInfo, ref logColor);
                    break;
                case LogEvents.Log:
                    Log(evtInfo, ref logColor);
                    break;
                case LogEvents.LogWarning:
                    Log(evtInfo, ref warningColor);
                    break;
                case LogEvents.LogError:
                    Log(evtInfo, ref errorColor);
                    break;
                case LogEvents.LogSuccess:
                    Log(evtInfo, ref successColor);
                    break;
            }
        }

        private void Log(object msg, ref Color color)
        {
            Text lastText;
            if (_messageCount < maxMessages)
            {
                lastText = _texts[_contentTransform.GetChild(_messageCount)];
                lastText.gameObject.SetActive(true);
                _messageCount++;
            }
            else
            {
                var firstChild = _contentTransform.GetChild(0);
                lastText = _texts[firstChild];
                firstChild.SetAsLastSibling();
            }

            lastText.color = color;
            lastText.text = (string)msg;

            if (autoScroll)
            {
                _scrollRect.verticalNormalizedPosition = 0.0f;
            }
        }

        protected override void DoAwake()
        {
            _scrollRect = GetComponentInChildren<ScrollRect>();
            _scrollTransform = _scrollRect.GetComponent<RectTransform>();
            _contentTransform = GetComponentInChildren<VerticalLayoutGroup>().GetComponent<RectTransform>();

            Text textTemplate = GetComponentInChildren<Text>();
            _texts = new Dictionary<Transform, Text>();
            for (int i = 0; i < maxMessages; i++)
            {
                var obj = Instantiate(textTemplate.gameObject, _contentTransform);
                obj.SetActive(false);
                _texts[obj.transform] = obj.GetComponent<Text>();
            }
            DestroyImmediate(textTemplate.gameObject);
            ServiceLocator.Set<ILogDisplay>(this);
        }
    }
}