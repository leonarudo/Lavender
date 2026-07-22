using System;
using UnityEngine;

namespace Lavender
{
    static internal class LavenderLog
    {
        public static void Log(string message)
        {
            if (BepinexPlugin.Settings.UseBepinexLog.Value)
            {
                BepinexPlugin.Log.LogInfo($"[<color=#9585f1>Lavender</color>] {message}");
            }
            else
            {
                Debug.Log($"[<color=#9585f1>Lavender</color>] {message}");
            }
        }

        public static void Detailed(string message)
        {
            if (BepinexPlugin.Settings.DetailedLog.Value)
            {
                if (BepinexPlugin.Settings.UseBepinexLog.Value)
                {
                    BepinexPlugin.Log.LogInfo($"[<color=#9585f1>Lavender</color>][Detailed] {message}");
                }
                else
                {
                    Debug.Log($"[<color=#9585f1>Lavender</color>][Detailed] {message}");
                }
            }
        }

        public static void Error(string message)
        {
            if (BepinexPlugin.Settings.UseBepinexLog.Value)
            {
                BepinexPlugin.Log.LogError($"[<color=#9585f1>Lavender</color>] {message}");
            }
            else
            {
                Debug.LogError($"[<color=#9585f1>Lavender</color>] {message}");
            }
        }
    }
}
