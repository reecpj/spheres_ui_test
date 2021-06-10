using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A slider controls the opacity of an associated sphere.
/// A button moves the slider to 0 or 1 depending on which is
/// closer
/// </summary>
public class SliderOpacityPanel : MonoBehaviour
{
    private GameObject _sphereGameObject;
    private MeshRenderer _meshRenderer;

    private SphereOpacitySystem _sphereOpacitySystem;

    public Slider Slider { get; private set; }
    public Button TransitionButton { get; private set; }

    private Coroutine _transitionRoutine;
    public Coroutine TransitionRoutine => _transitionRoutine;

    private byte _oldOpacity;

    public void Setup(GameObject sphere, string sphereName, SphereOpacitySystem sphereOpacitySystem)
    {
        _sphereOpacitySystem = sphereOpacitySystem;

        // get sphere mesh renderer and game object
        _meshRenderer = sphere.GetComponentInChildren<MeshRenderer>();
        _sphereGameObject = _meshRenderer.gameObject;

        // setup slider with opacity
        Slider = GetComponentInChildren<Slider>();
        Slider.onValueChanged.AddListener(ChangeSphereOpacity);
        
        // ensure initial material
        ChangeSphereOpacity(Slider.value);

        // set up name label
        name = $"Slider ({sphereName})";
        GetComponentInChildren<Text>().text = sphereName;

        // setup transition button
        TransitionButton = GetComponentInChildren<Button>();
        TransitionButton.onClick.AddListener(() =>
        {
            HelperFunctions.StartSliderTransition
                (ref _transitionRoutine, Slider, TransitionButton);
        });
    }

    public void ChangeSphereOpacity(float newOpacity)
    {
        newOpacity = Mathf.Clamp01(newOpacity);
        bool opacityNotZero = Mathf.Approximately(newOpacity, 0) == false;
        _sphereGameObject.SetActive(opacityNotZero);
        byte newOpacityAsByte = (byte) (newOpacity * Constants.MaxOpacityByteValue);
        if (opacityNotZero)
        {
            // switch to shared material provided
            _meshRenderer.sharedMaterial = _sphereOpacitySystem.SwitchToMaterialUsingOpacity(_oldOpacity, newOpacityAsByte);
        }

        _oldOpacity = newOpacityAsByte;
    }

    public void UpdateMaterial()
    {
        _meshRenderer.sharedMaterial = _sphereOpacitySystem.GetSphereMaterial(_oldOpacity);
    }
}
