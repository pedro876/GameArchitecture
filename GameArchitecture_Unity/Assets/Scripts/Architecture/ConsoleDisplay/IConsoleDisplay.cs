using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture.Logging
{
    public interface IConsoleDisplay
    {
        void Hide();
        void Show();
        void Clear();
    }
}


