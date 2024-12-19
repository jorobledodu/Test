using UnityEngine;

public class Debug_Test : MonoBehaviour
{
    public void DebugLog()
    {
        Debug.Log("This is a Debug.Log");
    }

    public void DebugLogWarning()
    {
        Debug.LogWarning("This is a Debug.LogWarning");
    }

    public void DebugLogError()
    {
        Debug.LogError("ERROR ERROR");
    }
}
