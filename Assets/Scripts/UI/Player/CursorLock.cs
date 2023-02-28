using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // TODO: handle setting to confined to deal with menus and such. Also find a better place to put this
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
