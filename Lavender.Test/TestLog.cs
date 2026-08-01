using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.Test
{
    internal class TestLog
    {
        public static void Log(string message)
        {
            Debug.Log($"[<color=#9585f1>Lavender.Test</color>] {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"[<color=#9585f1>Lavender.Test</color>] {message}");
        }
    }
}
