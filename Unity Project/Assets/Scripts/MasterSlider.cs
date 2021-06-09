using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control all the other sliders
/// </summary>
public class MasterSlider : MonoBehaviour
{
    private Slider _slider;
    private Coroutine _transitionRoutine;

    private List<SliderOpacityPanel> _opacityPanels;

    void Start()
    {
        _slider = GetComponentInChildren<Slider>();
        _slider.onValueChanged.AddListener(ChangeChildSliders);
        var button = GetComponentInChildren<Button>();
        button.onClick.AddListener((() => HelperFunctions.StartSliderTransition
            (ref _transitionRoutine, this, _slider)));
    }

    void OnDestroy()
    {
        _slider.onValueChanged.RemoveAllListeners();
        var button = GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
    }

    public void ChangeChildSliders(float t)
    {

        foreach (var child in _opacityPanels)
        {
            child.Slider.value = t;
        }
    }

    public void Setup(List<SliderOpacityPanel> opacityPanels)
    {
        _opacityPanels = opacityPanels;
    }
}
