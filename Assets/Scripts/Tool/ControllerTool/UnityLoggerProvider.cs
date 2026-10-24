using System;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Kun.Tool
{
    // 1. 實作自訂的 Unity Logger Provider
    public class UnityLoggerProvider
    {
        public static ILogger<T> CreateLogger<T> (string categoryName)
        {
            return new UnityLogger<T> (categoryName);
        }

        private class UnityLogger<T> : ILogger<T>
        {
            private readonly string _categoryName;

            public UnityLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;

                string message = $"[{_categoryName}] {formatter (state, exception)}";

                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        Debug.Log(message);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(message);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        if (exception != null)
                            Debug.LogError($"{message}\n{exception}");
                        else
                            Debug.LogError(message);
                        break;
                }
            }
        }
    }
}