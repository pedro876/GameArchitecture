using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture.Logging
{
    public interface ILogDisplay
    {
        void Hide();
        void Show();
        void Show(float width);
        void ChangeWidth(float width);
        void Clear();
    }
}


