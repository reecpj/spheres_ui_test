using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maintain an absolute 30 pixel offset for the left, bottom and top edges
/// of this UI element. The right edge is kept at 30% of the screen width
/// </summary>
public class AbsoluteUIOffsets : MonoBehaviour
{
    private Vector2 lastScreenResolution;
    private RectTransform rectTransform;
    private CanvasScaler canvasScaler;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
        ViewportSizeChange.OnViewportChanged += ViewportChanged;
    }

    void OnDestroy()
    {
        ViewportSizeChange.OnViewportChanged -= ViewportChanged;
    }

    /*
    void OnGUI()
    {
        GUI.color = new Color(1,1,0,0.2f);
        // draw the 30% line
        GUI.DrawTexture(new Rect(Screen.width*0.3f, 0, 2, Screen.height), 
            Texture2D.whiteTexture, ScaleMode.StretchToFill);
        // draw the 30 pixel rectangle
        GUI.DrawTexture(new Rect(0, 0, 30, Screen.height),
            Texture2D.whiteTexture, ScaleMode.StretchToFill);
    }
    */

    void ViewportChanged(Vector2 screenRes)
    {
        Vector2 referenceScreenResolution = new Vector2(900, 600);
        float referenceCanvasScale = HelperFunctions.GetUiScaleFactor(
            screenRes, referenceScreenResolution, canvasScaler.matchWidthOrHeight);
        float relativeOffset = Constants.AbsoluteOffsetPixels / referenceCanvasScale;
        rectTransform.anchoredPosition = new Vector2(relativeOffset, relativeOffset);
        // calculate what the canvas scale would be if we were just using the screen's width
        float screenWidthCanvasScale = HelperFunctions.GetUiScaleFactor(
            screenRes, referenceScreenResolution, 0);
        float relativeWidthConversion = screenWidthCanvasScale / referenceCanvasScale;
        // the right edge of the panel should reach 30% of the screen's width
        rectTransform.sizeDelta = new Vector2(
            (referenceScreenResolution.x * Constants.HorizontalScreenPanelWidth
                                         * relativeWidthConversion) - relativeOffset, 
            relativeOffset * -2.0f);
    }
}
