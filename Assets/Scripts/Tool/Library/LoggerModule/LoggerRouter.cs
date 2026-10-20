using System;
using System.Collections.Generic;

namespace Kun.Tool
{
    public static class LoggerRouter
    {
        static LoggerRouter ()
        {
            AddLogger (new UnityLogger ());
        }

        public static void AddLogger (Loggerable loggerable)
        {
            loggerables.Add (loggerable);
        }

        public static void RemoveLogger (Loggerable loggerable)
        {
            loggerables.Remove (loggerable);
        }

        static List<Loggerable> loggerables = new List<Loggerable> ();

        public static void Log (object msg)
        {
            foreach (var loggerable in loggerables)
            {
                loggerable.Log (msg);
            }
        }

        public static void Error (object msg)
        {
            foreach (var loggerable in loggerables)
            {
                loggerable.Error (msg);
            }
        }

        public static void Exception (Exception e)
        {
            foreach (var loggerable in loggerables)
            {
                loggerable.Exception (e);
            }
        }

        public static void Warn (object msg)
        {
            foreach (var loggerable in loggerables)
            {
                loggerable.Warn (msg);
            }
        }
    }
}