using UnityEngine;
using UnityEngine.InputSystem;

public class ESC : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame) 
        {
            Application.Quit();
        }
    }
}
