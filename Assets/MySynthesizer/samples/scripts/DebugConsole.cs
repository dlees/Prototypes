
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Text;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    public const int MaxNumLines = 100;
    public static event Action<string> Output = output;
    public static bool Disabled
    {
        get;
        set;
    }
    public static bool Timestamp
    {
        get;
        set;
    }
    [SerializeField, Tooltip("Screenに対する割合(0~1.0)")]
    private Rect rect = new Rect(0.03f, 0.03f, 0.94f, 0.94f);
    [SerializeField]
    private Color color = new Color(0.0f, 1.0f, 0.0f);
    [SerializeField]
    private int fontSize = 16;

    private static readonly StringBuilder builder = new StringBuilder();
    private static List<string> lines = new List<string>();
    public static UInt32 GetSystemTime()
    {
        return (UInt32)(DateTime.UtcNow.Ticks / 10000);  // 100nsec -> msec
    }
    private static void output(string str)
    {
        Debug.Log(str);
        lock (lines)
        {
            lines.Add(str);
            if (lines.Count > MaxNumLines)
            {
                lines.RemoveAt(0);
            }
        }
    }

    public static void WriteLine(string frm, params object[] args)
    {
        if (Disabled)
        {
            return;
        }
        if (frm == null)
        {
            return;
        }
        lock (builder)
        {
            if (Timestamp && (builder.Length == 0))
            {
                var t = GetSystemTime();
                uint min = ((uint)t / (60 * 1000)) % 60;
                uint sec = ((uint)t / (1000)) % 60;
                uint msec = (uint)t % 1000;
                builder.AppendFormat("{0}:{1}:{2} ", min.ToString("d2"), sec.ToString("d2"), msec.ToString("d3"));
            }
            builder.AppendFormat(frm, args);
#if false
                Output.Invoke(builder.ToString());
#else
            var str = builder.ToString().Split('\n');
            for (int i = 0; i < str.Length; i++)
            {
                Output.Invoke(str[i]);
            }
#endif
            builder.Remove(0, builder.Length);
        }
    }
    public static void Write(string frm, params object[] args)
    {
        if (Disabled)
        {
            return;
        }
        if (frm == null)
        {
            return;
        }
        lock (builder)
        {
            if (Timestamp && (builder.Length == 0))
            {
                var t = GetSystemTime();
                uint min = ((uint)t / (60 * 1000)) % 60;
                uint sec = ((uint)t / (1000)) % 60;
                uint msec = (uint)t % 1000;
                builder.Append(string.Format("{0}:{1}:{2} ", min.ToString("d2"), sec.ToString("d2"), msec.ToString("d3")));
            }
            builder.AppendFormat(frm, args);
            var str = builder.ToString().Split('\n');
            for (int i = 0; i < str.Length - 1; i++)
            {
                Output.Invoke(str[i]);
            }
            builder.Remove(0, builder.Length);
            builder.Append(str[str.Length - 1]);
        }
    }

    private readonly StringBuilder sb = new StringBuilder();
    void OnGUI()
    {
        GUIStyle style = GUI.skin.GetStyle("label");
        int oldFontSize = style.fontSize;
        style.fontSize = fontSize;
        int fh = (int)Mathf.Ceil(style.lineHeight);
        int h = ((int)(Screen.height * rect.height) - fh + 1) / fh;
        lock (lines)
        {
            int end = lines.Count;
            int begin = end - h;
            if (begin < 0)
            {
                begin = 0;
            }
            for (int i = begin; i < end; i++)
            {
                sb.AppendLine(lines[i]);
            }
        }
        Rect r = new Rect(Screen.width * rect.xMin, Screen.height * rect.yMin, Screen.width * rect.width, Screen.height * rect.height);
        Color prevColor = GUI.color;
        GUI.color = color;
        GUI.Label(r, sb.ToString());
        GUI.color = prevColor;
        style.fontSize = oldFontSize;
        sb.Remove(0, sb.Length);
    }
}
