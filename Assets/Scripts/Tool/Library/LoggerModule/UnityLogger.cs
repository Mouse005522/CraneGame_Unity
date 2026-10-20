using System;
using UnityEngine;

namespace Kun.Tool
{
    public class UnityLogger : Loggerable
    {
        void Loggerable.Error (object msg)
        {
            Debug.LogError (msg);
        }

        void Loggerable.Log (object msg)
        {
            Debug.Log (msg);
        }

        void Loggerable.Warn (object msg)
        {
            Debug.Log (msg);
        }

        void Loggerable.Exception (Exception e)
        {
            Debug.LogException (e);
        }
    }

}