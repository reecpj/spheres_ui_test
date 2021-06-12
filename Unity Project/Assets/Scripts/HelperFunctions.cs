using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Functions shared between multiple classes
/// </summary>
public class HelperFunctions
{
    /// <summary>
    /// Start a coroutine to transition the slider to one end or the other
    /// based on the current value. At the end, an action may be run.
    /// The coroutine runs on the slider MonoBehaviour.
    /// </summary>
    /// <param name="transitionRoutine">a reference to the same transition,
    /// if started previously. Will be stopped before the new one starts to
    /// avoid both setting the slider value at once. This reference is set
    /// to the new transition coroutine.</param>
    /// <param name="slider">slider with arbitrary min and max values</param>
    /// <param name="onTransitionFinished">Optional action to run after transition finishes</param>
    public static void StartSliderTransition(ref Coroutine transitionRoutine, 
        Slider slider, Action onTransitionFinished = null)
    {
        if(transitionRoutine != null)
            slider.StopCoroutine(transitionRoutine);
        transitionRoutine = slider.StartCoroutine(
            SmoothTransitionSlider(slider, SphereOpacitySystem.TransitionSeconds, onTransitionFinished));
    }

    public static IEnumerator SmoothTransitionSlider(Slider slider, float transitionSeconds, Action onTransitionFinished)
    {
        // if the slider is closer to the start, transition to the end,
        // otherwise transition to the start
        var initValue = slider.value;
        var fractionSlid = Mathf.InverseLerp(slider.minValue, slider.maxValue, initValue);
        const float halfwayPoint = 0.5f;
        var goingUp = fractionSlid < halfwayPoint;
        float finalValue = goingUp ? slider.maxValue : slider.minValue;

        // disable interactivity to avoid fighting with the user
        slider.enabled = false;
        // avoid dividing by zero (or negative durations)
        transitionSeconds = Mathf.Max(transitionSeconds, float.Epsilon);

        // transition from initValue to finalValue over transitionSeconds
        // how much between 0 and 1 to transition each frame
        float deltaTransition;
        for (float transitionAmount = 0; transitionAmount <= 1; transitionAmount += deltaTransition)
        {
            // avoid overshooting past the finalValue, if transitionSeconds is very small
            transitionAmount = Mathf.Min(transitionAmount, 1);

            // interpolate linearly
            slider.value = Mathf.Lerp(initValue, finalValue, transitionAmount);
            // wait a frame
            yield return null;
            deltaTransition = Time.deltaTime / transitionSeconds;
        }

        // don't leave t hanging just short of its finalValue
        slider.value = finalValue;
        slider.enabled = true;

        // allow a final action when the slider reaches its destination
        onTransitionFinished?.Invoke();
    }
}
