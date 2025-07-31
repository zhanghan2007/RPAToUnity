using UnityEditor;
using System.Net;
using System.Threading;
using UnityEngine;

[InitializeOnLoad]
public class RPAServer
{
    private static HttpListener listener;
    private static Thread listenerThread;
    private static bool isRunning;

    static RPAServer()
    {
        EditorApplication.quitting += StopServer;
    }

    [MenuItem("RPA/StartServer")]
    public static void StartServer()
    {
        if (isRunning) return;
        
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/rpa/");
        listener.Start();
        
        isRunning = true;
        listenerThread = new Thread(Listen) { IsBackground = true };
        listenerThread.Start();
        
        EditorApplication.delayCall += () => {
            EditorWindow.GetWindow<UnityEditor.EditorWindow>().ShowNotification(
                new GUIContent("RPA服务已启动 (端口:8080)"));
        };
    }

    [MenuItem("RPA/StopServer")]
    public static void StopServer()
    {
        isRunning = false;
        listener?.Stop();
        listenerThread?.Join(1000);
    }

    private static void Listen()
    {
        while (isRunning)
        {
            try
            {
                var context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (HttpListenerException) { /* 正常关闭 */ }
        }
    }

    private static void ProcessRequest(HttpListenerContext context)
    {
        // 验证安全令牌
        string token = context.Request.QueryString["token"];
        if (token != "HttpBridgeMethods")
        {
            SendResponse(context, 403, "Invalid token");
            return;
        }

        // 解析命令
        string methodName = context.Request.QueryString["method-name"];
        var args = context.Request.QueryString["args"];
        
        // 主线程执行编辑器方法
        EditorApplication.delayCall += () => {
            var r = HttpBridgeMethods.ExecuteMethond(methodName, args);
            SendResponse(context, r.Item1, r.Item2);
            StopServer();
        };
    }

    private static void SendResponse(HttpListenerContext context, int status, string content)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
        context.Response.StatusCode = status;
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.Close();
    }
}