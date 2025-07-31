using PSDUIImporter;
using UnityEngine;

public class HttpBridgeMethods
{
    public static (int, string) ExecuteMethond(string methodName, string args)
    {
        switch (methodName)
        {
            case "ExecutePSDConversion":
                var s = ExecutePSDConversion(args);
                return (200, s ? "转换psd到预制体成功" : "转换psd到预制体失败");
                break;
        }
        return (404, "Method not found");
    }
    
    public static bool ExecutePSDConversion(string args)
    {
        Debug.Log("进入方法");
        if (!string.IsNullOrEmpty(args))
        {
            // 调用目标方法
            PSDImportMenu.PSDGo(args);
            return true;
        }
        return false;
    }
}
