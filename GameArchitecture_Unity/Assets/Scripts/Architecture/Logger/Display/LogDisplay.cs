using Architecture.ObserverGroup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architecture.Services;
using System;

//<a href="https://www.flaticon.com/free-icons/attention" title="attention icons">Attention icons created by Andrean Prabowo - Flaticon</a>
//<a href="https://www.flaticon.com/free-icons/error" title="error icons">Error icons created by dmitri13 - Flaticon</a>
//< a href = "https://www.flaticon.com/free-icons/error" title = "error icons" > Error icons created by Freepik - Flaticon</a>
//<a href="https://www.flaticon.com/free-icons/tick" title="tick icons">Tick icons created by Kiranshastry - Flaticon</a>
namespace Architecture.Logging
{
    internal class LogDisplay : Observer<LogEvents>, ILogDisplay
    {
        private static readonly Color TRANSPARENT_COLOR = Color.white * 0.7f;
        private static readonly Color OPAQUE_COLOR = Color.white;

        [Header("DEBUG")]
        [SerializeField] bool printLogsOnStart = false;

        [Header("Visibility")]
        [SerializeField] bool showNormalLogs = true;
        [SerializeField] bool showErrors = true;
        [SerializeField] bool showWarnings = true;
        [SerializeField] bool showSuccesses = true;

        [Header("Configuration")]
        [SerializeField] uint maxMessages = 100;
        [SerializeField] bool timestamps = true;
        [SerializeField] bool autoScroll = true;

        [Header("References")]
        [SerializeField] Button clearButton;
        [SerializeField] Button messageButton;
        [SerializeField] Button warningButton;
        [SerializeField] Button errorButton;
        [SerializeField] Button successButton;
        [SerializeField] Text textTemplate;
        ScrollRect _scrollRect;
        RectTransform _scrollTransform;
        RectTransform _contentTransform;
        private Dictionary<Transform, Text> _texts;
        private Dictionary<Transform, Image> _images;
        private int _messageCount = 0;
        private bool _scrollingDown = false;

        private Image _messageImage;
        private Image _warningImage;
        private Image _errorImage;
        private Image _successImage;

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

        public void Clear()
        {
            _messageCount = 0;
            for(int i = 0; i < maxMessages; i++)
            {
                _contentTransform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public override void Notify(LogEvents evt, object evtInfo)
        {
            switch (evt)
            {
                default:
                    if (showNormalLogs) LogOnDisplay(evtInfo, _messageImage.sprite);
                    break;
                case LogEvents.Log:
                    if(showNormalLogs) LogOnDisplay(evtInfo, _messageImage.sprite);
                    break;
                case LogEvents.LogWarning:
                    if(showWarnings) LogOnDisplay(evtInfo, _warningImage.sprite);
                    break;
                case LogEvents.LogError:
                    if(showErrors) LogOnDisplay(evtInfo, _errorImage.sprite);
                    break;
                case LogEvents.LogSuccess:
                    if(showSuccesses) LogOnDisplay(evtInfo, _successImage.sprite);
                    break;
            }
        }

        private void LogOnDisplay(object msg, Sprite icon)
        {
            (Text lastText, Image lastImage) = GetLastTextEntry();
            lastImage.sprite = icon;
            if (timestamps)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                lastText.text = $"<color=#FFFFFF>[{time}]</color> {msg}";
            }
            else
                lastText.text = (string)msg;
            

            if (autoScroll && !_scrollingDown)
            {
                StartCoroutine(ScrollDown());
            }
            return;

            IEnumerator ScrollDown()
            {
                _scrollingDown = true;
                yield return null;
                yield return null;
                yield return null;
                _scrollRect.verticalNormalizedPosition = 0.0f;
                _scrollingDown = false;
            }

            (Text, Image) GetLastTextEntry()
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
                Image img = _images[lastText.transform];
                return (lastText, img);
            }
        }

        protected override void DoAwake()
        {
            BindButtonActions();
            CreatePool();
            ServiceLocator.Set<ILogDisplay>(this);
        }

        private void BindButtonActions()
        {
            clearButton.onClick.AddListener(Clear);
            _messageImage = messageButton.GetComponent<Image>();
            _warningImage = warningButton.GetComponent<Image>();
            _errorImage = errorButton.GetComponent<Image>();
            _successImage = successButton.GetComponent<Image>();

            messageButton.onClick.AddListener(() =>
            {
                showNormalLogs = !showNormalLogs;
                RefreshButtonImages();
            });
            warningButton.onClick.AddListener(() =>
            {
                showWarnings = !showWarnings;
                RefreshButtonImages();
            });
            errorButton.onClick.AddListener(() =>
            {
                showErrors = !showErrors;
                RefreshButtonImages();
            });
            successButton.onClick.AddListener(() =>
            {
                showSuccesses = !showSuccesses;
                RefreshButtonImages();
            });

            RefreshButtonImages();
            return;

            void RefreshButtonImages()
            {
                _messageImage.color = showNormalLogs ? OPAQUE_COLOR : TRANSPARENT_COLOR;
                _warningImage.color = showWarnings ? OPAQUE_COLOR : TRANSPARENT_COLOR;
                _errorImage.color = showErrors ? OPAQUE_COLOR : TRANSPARENT_COLOR;
                _successImage.color = showSuccesses ? OPAQUE_COLOR : TRANSPARENT_COLOR;
            }
        }

        private void CreatePool()
        {
            _scrollRect = GetComponentInChildren<ScrollRect>();
            _scrollTransform = _scrollRect.GetComponent<RectTransform>();
            _contentTransform = GetComponentInChildren<VerticalLayoutGroup>().GetComponent<RectTransform>();

            _texts = new Dictionary<Transform, Text>();
            _images = new Dictionary<Transform, Image>();
            for (int i = 0; i < maxMessages; i++)
            {
                var obj = Instantiate(textTemplate.gameObject, _contentTransform);
                obj.SetActive(false);
                _texts[obj.transform] = obj.GetComponent<Text>();
                _images[obj.transform] = obj.GetComponentInChildren<Image>();
            }
            DestroyImmediate(textTemplate.gameObject);
            this.LogSuccess("Registered");
        }

        private void Start()
        {
            if (printLogsOnStart)
            {
                this.LogSuccess("This is a success log");
                this.LogWarning("This is a warning log");
                this.LogError("This is an error log");
                this.Log("This is a normal log");
            }
        }
    }
}