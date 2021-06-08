using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Functions shared between multiple classes
/// </summary>
public class HelperFunctions
{
    // Function found in https://forum.unity.com/threads/canvasscaler-current-scale.285134/#post-3591418
    // Presumably what Unity uses in its canvas scaler with Match Width and Height.
    public static float GetUiScaleFactor(Vector2 screenSize, Vector2 m_ReferenceResolution, float m_MatchWidthOrHeight)
    {
        // We take the log of the relative width and height before taking the average.
        // Then we transform it back in the original space.
        // the reason to transform in and out of logarithmic space is to have better behavior.
        // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
        // In normal space the average would be (0.5 + 2) / 2 = 1.25
        // In logarithmic space the average is (-1 + 1) / 2 = 0
        const int kLogBase = 2;
        float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
        float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
        float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
        float scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
        return scaleFactor;
    }

    public static void StartSliderTransition(ref Coroutine transitionRoutine, 
        MonoBehaviour monoBehaviour, Slider slider)
    {
        if (transitionRoutine != null)
            monoBehaviour.StopCoroutine(transitionRoutine);
        transitionRoutine = monoBehaviour.StartCoroutine(SmoothTransitionSlider(slider));
    }

    public static IEnumerator SmoothTransitionSlider(Slider slider)
    {
        var initValue = slider.value;
        var goingUp = initValue < 0.5f;
        // disable to avoid fighting with the user
        slider.enabled = false;
        float finalValue = goingUp ? 1 : 0;
        // transition over 1 second to the final value
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            slider.value = Mathf.Lerp(initValue, finalValue, t);
            yield return null;
        }
        // don't leave t hanging just short of its finalValue
        slider.value = finalValue;
        slider.enabled = true;
    }
}
