using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kun.Tool
{
    public class EasyHttp
    {
        readonly int listenerPort;
        readonly string listenPath;
        readonly Dictionary<string, Func<HttpListenerContext, UniTask>> methodHandlers;

        HttpListener httpListener;
        Func<HttpListenerContext, UniTask> headerHandler;

        public EasyHttp (int listenerPort, string listenPath = "/")
        {
            this.listenerPort = listenerPort;
            this.listenPath = NormalizeListenPath (listenPath);
            methodHandlers = new Dictionary<string, Func<HttpListenerContext, UniTask>> (StringComparer.OrdinalIgnoreCase);
        }

        public void SetHeaderHandler (Func<HttpListenerContext, UniTask> headerHandler)
        {
            this.headerHandler = headerHandler;
        }

        public void RegisterMethodHandler (string httpMethod, Func<HttpListenerContext, UniTask> methodHandler)
        {
            if (string.IsNullOrWhiteSpace (httpMethod))
            {
                Debug.LogError ("[EasyHttp] RegisterMethodHandler 失敗：httpMethod 不可為空。");
            }
            else if (methodHandler != null)
            {
                methodHandlers[httpMethod] = methodHandler;
            }
            else
            {
                Debug.LogError ($"[EasyHttp] RegisterMethodHandler 失敗：{httpMethod} handler 不可為 null。");
            }
        }

        public async UniTask Start ()
        {
            try
            {
                httpListener = new HttpListener ();
                var prefix = $"http://*:{listenerPort}{listenPath}";
                Debug.Log ($"[EasyHttp] 嘗試監聽: {prefix}");
                httpListener.Prefixes.Add (prefix);
                httpListener.Start ();

                Debug.Log ($"[EasyHttp] 正在監聽: http://localhost:{listenerPort}{listenPath}");

                while (httpListener != null && httpListener.IsListening)
                {
                    HttpListenerContext context = await httpListener.GetContextAsync ();

                    try
                    {
                        if (headerHandler != null)
                        {
                            await headerHandler (context);
                        }

                        if (methodHandlers.TryGetValue (context.Request.HttpMethod, out Func<HttpListenerContext, UniTask> methodHandler))
                        {
                            await methodHandler (context);
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                            Debug.LogError ($"[EasyHttp] 尚未註冊 HttpMethod: {context.Request.HttpMethod}");
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        Debug.LogWarning ($"[EasyHttp] 處理請求異常: {ex.Message}");
                    }
                    finally
                    {
                        try { context.Response.Close (); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException (ex);
            }
        }

        public void Stop ()
        {
            if (httpListener != null)
            {
                try { httpListener.Stop (); } catch { }
                httpListener = null;
            }
        }

        static string NormalizeListenPath (string path)
        {
            if (string.IsNullOrWhiteSpace (path))
            {
                return "/";
            }

            string normalizedPath = path;

            if (!normalizedPath.StartsWith ("/"))
            {
                normalizedPath = $"/{normalizedPath}";
            }

            if (!normalizedPath.EndsWith ("/"))
            {
                normalizedPath = $"{normalizedPath}/";
            }

            return normalizedPath;
        }
    }
}
