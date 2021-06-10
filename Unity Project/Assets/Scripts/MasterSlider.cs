using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control all the other sliders
/// </summary>
public class MasterSlider : MonoBehaviour
{
    private Slider _masterSlider;
    private Coroutine _transitionRoutine;

    // keep a reference to the main list of opacity sliders to control
    private List<SliderOpacityPanel> _childSliderPanels;

    private void Start()
    {

        _masterSlider = GetComponentInChildren<Slider>();
        _masterSlider.onValueChanged.AddListener(ChangeChildSliders);

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
            AllowChildSliderInteraction(false);
            HelperFunctions.StartSliderTransition(ref _transitionRoutine, _masterSlider, transitionButton, 
                () => { AllowChildSliderInteraction(true); });
        });
    }

    private void AllowChildSliderInteraction(bool allow)
    {
        foreach (var panel in _childSliderPanels)
        {
            panel.Slider.enabled = allow;
            panel.TransitionButton.enabled = allow;
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
