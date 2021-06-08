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

    private SliderOpacityPanel[] _opacityPanels;

    void Start()
    {
        _slider = transform.GetChild(0).GetComponent<Slider>();
        _slider.onValueChanged.AddListener(ChangeChildSliders);
        var button = transform.GetChild(1).GetComponent<Button>();
        button.onClick.AddListener((() => HelperFunctions.StartSliderTransition
            (ref _transitionRoutine, this, _slider)));
    }

    public void ChangeChildSliders(float t)
    {
        foreach (var child in _opacityPanels)
        {
            child.Slider.value = t;
        }
    }

    public void Setup(SliderOpacityPanel[] opacityPanels)
    {
        _opacityPanels = opacityPanels;
    }
}
