using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control all the other sliders, using a master slider.
/// A button transitions the master slider
/// </summary>
public class MasterSlider : MonoBehaviour
{
    private Slider _masterSlider;
    private Coroutine _transitionRoutine;

    // keep a reference to the main list of opacity sliders to control
    private List<SliderOpacityPanel> _childSliderPanels;

    private void Start()
    {
        // set up master slider
        _masterSlider = GetComponentInChildren<Slider>();
        _masterSlider.onValueChanged.AddListener(ChangeChildSliders);

        // set up transition button to transition master slider
        var transitionButton = GetComponentInChildren<Button>();
        transitionButton.onClick.AddListener(() =>
        {
            // stop all existing transitions to avoid glitchy animation
            foreach (var childSlider in _childSliderPanels)
            {
                var currentTransitionRoutine = childSlider.TransitionRoutine;
                if (currentTransitionRoutine != null)
                {
                    childSlider.Slider.StopCoroutine(currentTransitionRoutine);
                }
            }

            // avoid child sliders changing values while the master is in control
            AllowChildSliderInteraction(false);
            HelperFunctions.StartSliderTransition(ref _transitionRoutine, _masterSlider, 
                // re-enable slider interaction after transition has ended
                () => { AllowChildSliderInteraction(true); });
        });
    }

    private void AllowChildSliderInteraction(bool allow)
    {
        foreach (var child in _childSliderPanels)
        {
            child.Slider.enabled = allow;
            child.TransitionButton.enabled = allow;
        }
    }

    private void ChangeChildSliders(float newSliderValue)
    {
        foreach (var child in _childSliderPanels)
        {
            child.Slider.value = newSliderValue;
        }
    }

    public void Setup(List<SliderOpacityPanel> opacityPanels)
    {
        _childSliderPanels = opacityPanels;
    }

    private void OnDestroy()
    {
        _masterSlider.onValueChanged.RemoveAllListeners();
        var button = GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
    }
}
