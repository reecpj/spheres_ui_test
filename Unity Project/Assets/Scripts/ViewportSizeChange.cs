using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Send an event when the viewport size changes
/// </summary>
public class ViewportSizeChange : MonoBehaviour
{
    public delegate void ViewportChanged(Vector2 newViewportResolution);

    public static event ViewportChanged OnViewportChanged;
    private Vector2 lastScreenResolution;

    void Update()
    {
        var screenRes = new Vector2(Screen.width, Screen.height);
        if (screenRes != lastScreenResolution)
        {
            lastScreenResolution = screenRes;
            if(OnViewportChanged != null)
                OnViewportChanged(screenRes);
        }
    }
}
