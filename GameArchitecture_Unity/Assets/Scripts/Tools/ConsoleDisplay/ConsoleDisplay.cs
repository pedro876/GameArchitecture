using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architecture.Services;

//<a href="https://www.flaticon.com/free-icons/attention" title="attention icons">Attention icons created by Andrean Prabowo - Flaticon</a>
//<a href="https://www.flaticon.com/free-icons/error" title="error icons">Error icons created by dmitri13 - Flaticon</a>
//< a href = "https://www.flaticon.com/free-icons/error" title = "error icons" > Error icons created by Freepik - Flaticon</a>
//<a href="https://www.flaticon.com/free-icons/tick" title="tick icons">Tick icons created by Kiranshastry - Flaticon</a>
//<a href="https://www.flaticon.com/free-icons/document" title="document icons">Document icons created by Freepik - Flaticon</a>
namespace Architecture.Logging
{
    internal class ConsoleDisplay : MonoBehaviour, /*Observer<LogEvents>, */IConsoleDisplay
    {
        private const string MESSAGE_LOG_COLOR = "#FFFFFF";
        private const string WARNING_LOG_COLOR = "#DDD54F";
        private const string ERROR_LOG_COLOR = "#E43539";
        private const string SUCCESS_LOG_COLOR = "#3BB54A";
        private static readonly Color TRANSPARENT_BUTTON_COLOR = Color.white * 0.7f;
        private static readonly Color OPAQUE_BUTTON_COLOR = Color.white;

        [Header("DEBUG")]
        [SerializeField] bool printLogsOnStart = false;

        [Header("Visibility")]
        [SerializeField] bool showNormalLogs = true;
        [SerializeField] bool showErrors = true;
        [SerializeField] bool showWarnings = true;
        [SerializeField] bool showSuccesses = true;

        [Header("Configuration")]
        [SerializeField] uint maxLogEntriesPerType = 100;
        [SerializeField] bool timestamps = true;
        [SerializeField] bool autoScroll = true;

        [Header("References")]
        [SerializeField] Button expandHorizontalButton;
        [SerializeField] Button expandVerticalButton;
        private bool _isExpandedHorizontally = false;
        private bool _isExpandedVertically = true;
        private Text _expandHorizontalText;
        private Text _expandVerticalText;
        [SerializeField] Button clearButton; 
        [SerializeField] Button messageButton;
        [SerializeField] Button warningButton;
        [SerializeField] Button errorButton;
        [SerializeField] Button successButton;
        [SerializeField] Text textTemplate;
        private ScrollRect _scrollRect;
        private RectTransform _scrollTransform;
        private RectTransform _contentTransform;
        private RectTransform _canvasTransform;
        //private CanvasScaler _canvasScaler;
        private float _originalWidth;
        private float _originalHeight;
        private const float _contractedVerticalSize = 42;

        //private Dictionary<Transform, ConsoleDisplayEntry> _entries;
        private ConsoleDisplayEntry[] _logEntries;
        private ConsoleDisplayEntry[] _warningEntries;
        private ConsoleDisplayEntry[] _errorEntries;
        private ConsoleDisplayEntry[] _successEntries;
        private int _logCount = 0;
        private int _warningCount = 0;
        private int _errorCount = 0;
        private int _successCount = 0;
        private bool _scrollingDown = false;

        private Image _messageImage;
        private Image _warningImage;
        private Image _errorImage;
        private Image _successImage;

        private bool _configured = false;
        

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);
        public void Clear()
        {
            _logCount = 0;
            _warningCount = 0;
            _errorCount = 0;
            _successCount = 0;
            for(int i = 0; i < maxLogEntriesPerType; i++)
            {
                _logEntries[i].gameObject.SetActive(false);
                _warningEntries[i].gameObject.SetActive(false);
                _errorEntries[i].gameObject.SetActive(false);
                _successEntries[i].gameObject.SetActive(false);
            }
        }

        private void ChangeWidth(float width)
        {
            var size = _scrollTransform.sizeDelta;
            size.x = width;
            _scrollTransform.sizeDelta = size;
        }

        private void ChangeHeight(float height)
        {
            var size = _scrollTransform.sizeDelta;
            size.y = height;
            _scrollTransform.sizeDelta = size;
        }

        private void OnLog(string log, string stackTrace, LogType logType)
        {
            switch (logType)
            {
                default:
                case LogType.Log:
                    bool isSuccess = log.HasSuccessPrefix();
                    if(isSuccess)
                        LogOnDisplay(log.RemoveSuccessPrefix().ColorWrap(SUCCESS_LOG_COLOR), logType, true);
                    else
                        LogOnDisplay(log.ColorWrap(MESSAGE_LOG_COLOR), logType);
                    break;
                case LogType.Warning:
                    LogOnDisplay(log.ColorWrap(WARNING_LOG_COLOR), logType);
                    break;
                case LogType.Error:
                case LogType.Exception:
                    LogOnDisplay(log.ColorWrap(ERROR_LOG_COLOR), logType);
                    break;
            }
        }

        private void LogOnDisplay(string msg, LogType logType, bool isSuccess = false)
        {
            ConsoleDisplayEntry entry = GetLastEntry(logType, isSuccess);
            entry.SetText(timestamps ? msg.AddTimestampPrefix() : msg);
            entry.MoveToBottom();
            entry.gameObject.SetActive(true);

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

            ConsoleDisplayEntry GetLastEntry(LogType logType, bool isSuccess)
            {
                ConsoleDisplayEntry lastEntry = null;
                ConsoleDisplayEntry[] entriesToUpdate;

                switch (logType)
                {
                    default:
                    case LogType.Log:
                        if (isSuccess)
                        {
                            if(_successCount < maxLogEntriesPerType)
                                lastEntry = _successEntries[_successCount];
                            _successCount++;
                            entriesToUpdate = _successEntries;
                        }
                        else
                        {
                            if(_logCount < maxLogEntriesPerType)
                                lastEntry = _logEntries[_logCount];
                            _logCount++;
                            entriesToUpdate = _logEntries;
                        }
                        break;
                    case LogType.Warning:
                        if (_warningCount < maxLogEntriesPerType)
                            lastEntry = _warningEntries[_warningCount];
                        _warningCount++;
                        entriesToUpdate = _warningEntries;
                        break;
                    case LogType.Exception:
                    case LogType.Error:
                        if (_errorCount < maxLogEntriesPerType)
                            lastEntry = _errorEntries[_errorCount];
                        _errorCount++;
                        entriesToUpdate = _errorEntries;
                        break;
                }

                
                if (lastEntry == null) // was not avaible, must use oldest
                {
                    lastEntry = entriesToUpdate[0];
                    for(int i = 0; i < maxLogEntriesPerType-1; i++)
                    {
                        entriesToUpdate[i] = entriesToUpdate[i + 1];
                    }
                    entriesToUpdate[maxLogEntriesPerType - 1] = lastEntry;
                }
                return lastEntry;
            }
        }

        private void Awake()
        {
            _scrollRect = GetComponentInChildren<ScrollRect>();
            _scrollTransform = _scrollRect.GetComponent<RectTransform>();
            _contentTransform = GetComponentInChildren<VerticalLayoutGroup>().GetComponent<RectTransform>();
            _canvasTransform = GetComponent<RectTransform>();
            //_canvasScaler = GetComponent<CanvasScaler>();
            //Debug.Log(_canvasScaler.referenceResolution.y);
            //_contractedVerticalSize = _contentTransform.rect.height;// textTemplate.GetComponent<RectTransform>().sizeDelta.y;
            //ChangeHeight(_canvasScaler.referenceResolution.y + _scrollTransform.anchoredPosition.y * 2f);
            _originalWidth = _scrollTransform.sizeDelta.x;
            _originalHeight = _scrollTransform.sizeDelta.y;
            _messageImage = messageButton.GetComponent<Image>();
            _warningImage = warningButton.GetComponent<Image>();
            _errorImage = errorButton.GetComponent<Image>();
            _successImage = successButton.GetComponent<Image>();
            _expandHorizontalText = expandHorizontalButton.GetComponentInChildren<Text>();
            _expandVerticalText = expandVerticalButton.GetComponentInChildren<Text>();
            CreatePools();
            BindButtonActions();
            Application.logMessageReceived += OnLog;
            _configured = true;
            ServiceLocator.Set<IConsoleDisplay>(this);
        }

        private void OnEnable()
        {
            if (_configured) return;
            Application.logMessageReceived += OnLog;
            _configured = true;
        }

        private void OnDisable()
        {
            if (!_configured) return;
            Application.logMessageReceived -= OnLog;
            _configured = false;
        }

        private void BindButtonActions()
        {
            expandHorizontalButton.onClick.AddListener(OnExpandHorizontal);
            expandVerticalButton.onClick.AddListener(OnExpandVertical);
            clearButton.onClick.AddListener(Clear);
            messageButton.onClick.AddListener(() =>
            {
                showNormalLogs = !showNormalLogs;
                RefreshButtonImages();
                ChangeEntriesVisibility(_logEntries, _logCount, showNormalLogs);
            });
            warningButton.onClick.AddListener(() =>
            {
                showWarnings = !showWarnings;
                RefreshButtonImages();
                ChangeEntriesVisibility(_warningEntries, _warningCount, showWarnings);
            });
            errorButton.onClick.AddListener(() =>
            {
                showErrors = !showErrors;
                RefreshButtonImages();
                ChangeEntriesVisibility(_errorEntries, _errorCount, showErrors);
            });
            successButton.onClick.AddListener(() =>
            {
                showSuccesses = !showSuccesses;
                RefreshButtonImages();
                ChangeEntriesVisibility(_successEntries, _successCount, showSuccesses);
            });

            RefreshButtonImages();
            ChangeEntriesVisibility(_logEntries, _logCount, showNormalLogs);
            ChangeEntriesVisibility(_warningEntries, _warningCount, showWarnings);
            ChangeEntriesVisibility(_errorEntries, _errorCount, showErrors);
            ChangeEntriesVisibility(_successEntries, _successCount, showSuccesses);
            return;

            void RefreshButtonImages()
            {
                _messageImage.color = showNormalLogs ? OPAQUE_BUTTON_COLOR : TRANSPARENT_BUTTON_COLOR;
                _warningImage.color = showWarnings ? OPAQUE_BUTTON_COLOR : TRANSPARENT_BUTTON_COLOR;
                _errorImage.color = showErrors ? OPAQUE_BUTTON_COLOR : TRANSPARENT_BUTTON_COLOR;
                _successImage.color = showSuccesses ? OPAQUE_BUTTON_COLOR : TRANSPARENT_BUTTON_COLOR;
            }

            void ChangeEntriesVisibility(ConsoleDisplayEntry[] entries, int count, bool visible)
            {
                for(int i = 0; i < entries.Length; i++)
                {
                    entries[i].gameObject.SetActive(visible && i < count);
                }
            }

            void OnExpandHorizontal()
            {
                _isExpandedHorizontally = !_isExpandedHorizontally;
                _expandHorizontalText.transform.Rotate(Vector3.forward, 180f);
                ChangeWidth(_isExpandedHorizontally ? _canvasTransform.sizeDelta.x - _scrollTransform.anchoredPosition.x*2f : _originalWidth);

                //if(_isExpandedHorizontally)
            }

            void OnExpandVertical()
            {
                _isExpandedVertically = !_isExpandedVertically;
                _expandVerticalText.transform.Rotate(Vector3.forward, 180f);
                ChangeHeight(_isExpandedVertically ? _originalHeight : _contractedVerticalSize);
            }
        }

        private void CreatePools()
        {
            _logEntries = new ConsoleDisplayEntry[maxLogEntriesPerType];
            _warningEntries = new ConsoleDisplayEntry[maxLogEntriesPerType];
            _errorEntries = new ConsoleDisplayEntry[maxLogEntriesPerType];
            _successEntries = new ConsoleDisplayEntry[maxLogEntriesPerType];
            for (int i = 0; i < maxLogEntriesPerType; i++)
                _logEntries[i] = CreateEntry(LogType.Log, _messageImage.sprite, isSuccess:false);
            for (int i = 0; i < maxLogEntriesPerType; i++)
                _warningEntries[i] = CreateEntry(LogType.Warning, _warningImage.sprite, isSuccess: false);
            for (int i = 0; i < maxLogEntriesPerType; i++)
                _errorEntries[i] = CreateEntry(LogType.Error, _errorImage.sprite, isSuccess: false);
            for (int i = 0; i < maxLogEntriesPerType; i++)
                _successEntries[i] = CreateEntry(LogType.Log, _successImage.sprite, isSuccess: true);

            DestroyImmediate(textTemplate.gameObject);
            return;

            ConsoleDisplayEntry CreateEntry(LogType type, Sprite icon, bool isSuccess = false)
            {
                var obj = Instantiate(textTemplate.gameObject, _contentTransform);
                var entry = obj.GetComponent<ConsoleDisplayEntry>();
                entry.SetType(type, icon, isSuccess);
                obj.SetActive(false);
                return entry;
            }
        }

        private void Start()
        {
            if (printLogsOnStart)
            {
                Debug.Log("This is a normal log");
                Debug.LogWarning("This is a warning log");
                Debug.LogError("This is an error log");
            }
        }
    }
}