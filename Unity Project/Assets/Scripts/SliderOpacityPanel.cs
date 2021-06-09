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
    private Material _opaqueMaterial; // shared reference to reduce draw calls
    private Material _fadeMaterial; // unique reference generated from above
    public Slider Slider { get; private set; }
    private Coroutine _transitionRoutine;

    public void Setup(GameObject sphere, string sphereName)
    {
        // get sphere mesh renderer and game object
        _meshRenderer = sphere.GetComponentInChildren<MeshRenderer>();
        _sphereGameObject = _meshRenderer.gameObject;

        // setup slider with opacity
        Slider = GetComponentInChildren<Slider>();
        Slider.onValueChanged.AddListener(ChangeSphereOpacity);

        // set up name label
        name = $"Slider ({sphereName})";
        GetComponentInChildren<Text>().text = sphereName;

        // setup transition button
        var transitionButton = GetComponentInChildren<Button>();
        transitionButton.onClick.AddListener((() => HelperFunctions.StartSliderTransition
            (ref _transitionRoutine, this, Slider)));
    }

    public void ChangeMaterial(Material opaqueMaterial, Shader fadeShader)
    {
        _opaqueMaterial = opaqueMaterial;
        // create a new fade material from the opaque material
        _fadeMaterial = new Material(opaqueMaterial);
        _fadeMaterial.shader = fadeShader;
        // update the material on the object
        ChangeSphereOpacity(Slider.value);
    }

    public void ChangeSphereOpacity(float o)
    {
        o = Mathf.Clamp01(o);
        bool opacityNotZero = Mathf.Approximately(o, 0) == false;
        _sphereGameObject.SetActive(opacityNotZero);
        if (opacityNotZero)
        {
            if (Mathf.Approximately(o, 1))
            {
                // switch to shared opaque material
                _meshRenderer.sharedMaterial = _opaqueMaterial;
            }
            else
            {
                // switch to sphere-specific fade material and set opacity
                var c = _fadeMaterial.color;
                _fadeMaterial.color = new Color(c.r, c.g, c.b, o);
                _meshRenderer.material = _fadeMaterial;
            }
        }
    }
}
