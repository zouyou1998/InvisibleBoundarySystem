using System.IO;
using UnityEngine;

public class LogToFile : MonoBehaviour
{
    private string logFilePath;
    private bool isLoggingEnabled = true; // 可通过 Inspector 或代码控制是否记录日志

    void Awake()
    {
        // 设置日志文件路径
        logFilePath = Application.persistentDataPath + "/game_log.txt";
        Debug.Log("Log file path: " + logFilePath);

        // 注册日志回调，捕获所有 Debug.Log 消息
        Application.logMessageReceived += HandleLog;

        // 可选：初始化时清空旧日志文件
        //if (File.Exists(logFilePath))
        //{
            //File.WriteAllText(logFilePath, $"[{System.DateTime.Now}] Log Initialized\n");
        //}
    }

    void OnDestroy()
    {
        // 取消注册，避免内存泄漏
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!isLoggingEnabled) return;

        // 格式化日志消息
        string logMessage = $"[{System.DateTime.Now}] {type}: {logString}\n";

        // 检查文件大小，防止过大（可选）
        if (File.Exists(logFilePath) && new FileInfo(logFilePath).Length > 1024 * 1024) // 限制1MB
        {
            File.WriteAllText(logFilePath, $"[{System.DateTime.Now}] Log Cleared (Size Limit)\n");
        }

        // 写入日志
        try
        {
            File.AppendAllText(logFilePath, logMessage);
            if (type == LogType.Error || type == LogType.Exception)
            {
                File.AppendAllText(logFilePath, stackTrace + "\n"); // 记录错误或异常的堆栈跟踪
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write log to file: {e.Message}");
        }
    }

    // 可选：提供方法动态启用/禁用日志
    public void SetLoggingEnabled(bool enabled)
    {
        isLoggingEnabled = enabled;
    }
}