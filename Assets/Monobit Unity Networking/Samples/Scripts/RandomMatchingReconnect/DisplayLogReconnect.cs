using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

public class DisplayLogReconnect : MonoBehaviour {

    // デバッグログ
    public static List<string> logMessage = new List<string>();

    private static bool m_bCreated = false;

    void Awake()
    {
        if (!m_bCreated)
        {
            m_bCreated = true;
            Application.logMessageReceived += AddLog;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void OnGUI()
    {
        // デバッグログの表示
        foreach (string str in logMessage)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
            GUILayout.Label(str);
            GUILayout.EndHorizontal();
        }
    }

    void AddLog(string condition, string stackTrace, LogType type)
    {
        if(condition.Contains("[INFO]") || condition.Contains("[DEBUG]") || condition.Contains("[WARNING]"))
        {
            logMessage.Add(condition);
            if (logMessage.Count > 10)
            {
                logMessage.RemoveAt(0);
            }
        }
    }
}
