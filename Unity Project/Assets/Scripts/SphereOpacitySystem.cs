using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Set up all the spheres, connect the sliders, control the
/// opaque material changing.
/// Move the spheres around if the window size changes
/// </summary>
public class SphereOpacitySystem : MonoBehaviour
{

    // Parent for all spheres in the hierarchy
    [SerializeField] private Transform _spheresParent;
    // The master slider for controlling all spheres is present in the scene
    [SerializeField] private MasterSlider _masterSlider;

    // Fields to be filled in from the Project's assets
    [SerializeField] private GameObject _sliderPrefab;
    [SerializeField] private GameObject _spherePrefab;
    // This opaque material is shared across all opaque spheres to reduce draw calls
    [SerializeField] private Material _opaqueSphereMaterial;
    // This shader is used on each material instance of each transparent sphere.
    // Changing the OpaqueSphereMaterial will update the transparent material
    // instances, using this shader.
    [SerializeField] private Shader _fadeSphereShader;

    // references to scene components
    private Transform _sliderParent;
    private AdjustGridCellSize _adjustGridCellSize;

    private Material _previousOpaqueMaterial;

    // Number of spheres can be changed from UI
    [SerializeField] private int _numSpheres = 50;
    // Record previous number to refresh only when the number of spheres changes
    private int _previousNumSpheres;

    private List<SliderOpacityPanel> _opacityPanels;

    void Start()
    {
        _opacityPanels = new List<SliderOpacityPanel>(_numSpheres);

        // set up refs from hierarchy
        _sliderParent = _masterSlider.transform.parent;
        _adjustGridCellSize = _spheresParent.GetComponentInParent<AdjustGridCellSize>();

        // generate spheres and sliders
        _previousNumSpheres = 0;
        GeneratedNeededSpheres();

        // set up master slider
        _masterSlider.Setup(_opacityPanels);
    }

    /// <summary>
    /// Generate new spheres or delete unnecessary spheres
    /// if the number of spheres changes at runtime.
    /// </summary>
    private void GeneratedNeededSpheres()
    {
        if (_numSpheres < 0)
            _numSpheres = 0;
        var numRequiredSpheres = _numSpheres - _previousNumSpheres;

        if (numRequiredSpheres < 0)
        {
            // remove unnecessary spheres and sliders, starting from the end and working backwards
            var numSpheresToRemove = -numRequiredSpheres;
            var finalSphereIndex = _spheresParent.childCount - 1;
            var finalSliderIndex = _previousNumSpheres - 1;

            for (int removalIndex = 0; removalIndex < numSpheresToRemove; ++removalIndex)
            {
                // destroy slider
                var listRemovalIndex = finalSliderIndex - removalIndex;
                var sliderPanelToDestroy = _opacityPanels[listRemovalIndex].gameObject;
                Destroy(sliderPanelToDestroy);

                // destroy sphere
                var parentChildRemovalIndex = finalSphereIndex - removalIndex;
                var createdSphere = _spheresParent.GetChild(parentChildRemovalIndex).gameObject;
                Destroy(createdSphere);
            }

            // update slider panels list
            _opacityPanels.RemoveRange(_numSpheres, numSpheresToRemove);
        }
        else
        {
            // generate needed spheres
            for (int creationIndex = 0; creationIndex < numRequiredSpheres; ++creationIndex)
            {
                // create spheres
                var sphereGameObject = GameObject.Instantiate(_spherePrefab, _spheresParent);
                var absoluteSphereIndex = _previousNumSpheres + creationIndex;
                var sphereLabel = "Sphere " + (absoluteSphereIndex + 1);
                sphereGameObject.name = "UI " + sphereLabel;

                // create sliders
                var sliderPanel = GameObject.Instantiate(_sliderPrefab, _sliderParent);
                var sliderOpacityPanel = sliderPanel.GetComponent<SliderOpacityPanel>();
                sliderOpacityPanel.Setup(sphereGameObject, sphereLabel);
                sliderOpacityPanel.ChangeMaterial(_opaqueSphereMaterial, _fadeSphereShader);

                _opacityPanels.Add(sliderOpacityPanel);
            }
        }

        _adjustGridCellSize.UpdateNumCells(_numSpheres);

        _previousNumSpheres = _numSpheres;
    }

    void Update()
    {
        if (_previousNumSpheres != _numSpheres)
        {
            GeneratedNeededSpheres();
        }

        if (_opaqueSphereMaterial != _previousOpaqueMaterial)
        {
            ChangeMaterial();
        }
    }

    [ContextMenu("Update Opaque Material or Fade shader")]
    private void ChangeMaterial()
    {
        foreach (var opacityPanel in _opacityPanels)
        {
            opacityPanel.ChangeMaterial(_opaqueSphereMaterial, _fadeSphereShader);
        }

        _previousOpaqueMaterial = _opaqueSphereMaterial;
    }
}
