using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Set up all the spheres, connect the sliders, control the
/// opaque material changing.
/// Cache faded materials to avoid duplicates
/// </summary>
public class SphereOpacitySystem : MonoBehaviour
{
    // Scene hierarchy fields to fill in

    [Header("Scene hierarchy fields")]
    // Parent for all spheres in the hierarchy
    [SerializeField] private Transform _spheresParent;
    // The master slider for controlling all spheres is present in the scene
    [SerializeField] private MasterSlider _masterSlider;

    // Fields to be filled in from the Project's assets

    [Header("Project asset fields")]
    [SerializeField] private GameObject _sliderPrefab;
    [SerializeField] private GameObject _spherePrefab;

    // Materials/ shaders

    [Header("Swappable material/ shader", order = 1)]
    // header instructions for designers
    // avoid taking up too much space in UI
    [Space(-10, order = 2)]
    [Header("right-click Sphere Opacity System to", order = 2)]
    [Space(-10, order = 2)]
    [Header("update material colours/ values/ shaders", order = 3)]
    // This opaque material is shared across all opaque spheres to reduce draw calls
    [SerializeField] private Material _opaqueSphereMaterial;

    // This shader is used on each material instance of each transparent sphere.
    // Changing the OpaqueSphereMaterial will update the transparent
    // material instances, using this shader
    [Tooltip("This shader is used on each material instance of each transparent sphere. Changing the OpaqueSphereMaterial will update the transparent material instances, using this shader.")]
    [SerializeField] private Shader _fadeSphereShader;

    // only create new fade materials if the opaque material changes
    private Material _previousOpaqueMaterial;
    // cache fade materials according to their opacity level from 1-254, to
    // reduce numbers of materials and draw calls. 255 is opaque; 0 is invisible
    private Dictionary<byte, Material> _fadeMaterials;
    // avoid creating and maintaining materials that are not in use
    private Dictionary<byte, int> _refCountOfMaterialWithOpacity;

    // References to scene components

    private Transform _sliderParent;
    private AdjustGridCellSize _adjustGridCellSize;

    // Tweakable parameters

    [Header("Tweakable at runtime")]
    // Number of spheres can be changed from UI
    [SerializeField] private int _numSpheres = 50;
    // Record previous number to refresh only when the number of spheres changes
    private int _previousNumSpheres;

    // Allow changing the duration the slider transitions take,
    // after clicking the UI buttons with text on
    [Tooltip("Change the duration the slider transitions take, after clicking the UI buttons with text on")]
    [SerializeField] private float _transitionSeconds = 1;
    public static float TransitionSeconds;

    private List<SliderOpacityPanel> _opacityPanels;

    void Start()
    {
        _opacityPanels = new List<SliderOpacityPanel>(_numSpheres);
        // 256 possible alpha values, minus opaque and invisible
        const int possibleFadeAlphas = 254;
        _fadeMaterials = new Dictionary<byte, Material>(possibleFadeAlphas);
        _refCountOfMaterialWithOpacity = new Dictionary<byte, int>(possibleFadeAlphas);

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
                sliderOpacityPanel.Setup(sphereGameObject, sphereLabel, this);

                _opacityPanels.Add(sliderOpacityPanel);
            }
        }

        _adjustGridCellSize.UpdateNumCells(_numSpheres);

        _previousNumSpheres = _numSpheres;
    }

    // at the moment, only the unity editor UI can control any of these values.
    // Turn off Update for player builds to save performance
#if UNITY_EDITOR
    void Update()
    {
        // Handle user changing number of spheres
        if (_previousNumSpheres != _numSpheres)
        {
            GeneratedNeededSpheres();
        }

        // Handle opaque material changing
        if (_opaqueSphereMaterial != _previousOpaqueMaterial)
        {
            ChangeMaterial();
        }

        // Handle user changing slider transition seconds
        TransitionSeconds = _transitionSeconds;
    }
#endif

    [ContextMenu("Update Opaque Material or Fade shader")]
    private void ChangeMaterial()
    {
        // cache the keys due to modifying the collection in the loop
        var opacityByteValues = _fadeMaterials.Keys.ToList();
        // create new materials based on all used opacity values
        foreach (var opacityByteValue in opacityByteValues)
        {
            _fadeMaterials[opacityByteValue] = CreateFadeMaterial(opacityByteValue);
        }

        // switch the Material reference on the spheres over to the new Material
        foreach (var opacityPanel in _opacityPanels)
        {
            opacityPanel.UpdateMaterial();
        }

        _previousOpaqueMaterial = _opaqueSphereMaterial;
    }

    /// <summary>
    /// Quickly return the material with a certain
    /// opacity level if known it is present in the
    /// _fadeMaterials Dictionary
    /// </summary>
    /// <param name="opacity"></param>
    /// <returns></returns>
    public Material GetSphereMaterial(byte opacity)
    {
        if (opacity == byte.MaxValue)
            return _opaqueSphereMaterial;
        return _fadeMaterials[opacity];
    }

    /// <summary>
    /// Switch from an old opacity value to a new opacity value.
    /// 
    /// Since there are so many possible spheres, cache the material
    /// at each used opacity value, rather than giving each sphere a new
    /// material.
    ///
    /// Count the references to each material based on its opacity level
    /// to allow cleaning up unused transparent materials.
    /// </summary>
    /// <param name="oldOpacity">the previous opacity byte value</param>
    /// <param name="newOpacity">the desired opacity byte value</param>
    /// <returns>The new or existing material</returns>
    public Material SwitchToMaterialUsingOpacity(byte oldOpacity, byte newOpacity)
    {
        // byte.MaxValue means the old material is opaque. The opaque material is
        // always held in memory, thus not reference counted like the fade materials
        if (oldOpacity != byte.MaxValue)
        {
            // update reference counts of materials with old opacity
            if (_refCountOfMaterialWithOpacity.ContainsKey(oldOpacity))
            {
                _refCountOfMaterialWithOpacity[oldOpacity]--;

                // if no more references of materials with this opacity exist,
                // destroy the material
                if (_refCountOfMaterialWithOpacity[oldOpacity] == 0)
                {
                    _refCountOfMaterialWithOpacity.Remove(oldOpacity);
                    _fadeMaterials.Remove(oldOpacity);
                }
            }
        }

        // with opaque requests, just return the opaque material; no need to reference count
        if (newOpacity == byte.MaxValue)
        {
            return _opaqueSphereMaterial;
        }

        // if there is no ref to a material with this opacity,
        // create a new material
        if (!_refCountOfMaterialWithOpacity.ContainsKey(newOpacity))
        {
            // create slot for reference counting this opacity
            _refCountOfMaterialWithOpacity[newOpacity] = 0;
            _fadeMaterials[newOpacity] = CreateFadeMaterial(newOpacity);
        }

        // increment the reference count of the number of times this material is used
        _refCountOfMaterialWithOpacity[newOpacity]++;
        // return the reference of the material with the desired opacity
        return _fadeMaterials[newOpacity];
    }

    private Material CreateFadeMaterial(byte opacityByteValue)
    {
        // create a new material based on the opaque material, fade shader, and opacity value
        var newColor = _opaqueSphereMaterial.color;
        // convert from 0-255 to 0 to 1 for Color's float value
        newColor.a = opacityByteValue / Constants.MaxOpacityByteValue;
        return new Material(_opaqueSphereMaterial)
        {
            shader = _fadeSphereShader,
            color = newColor
        };
    }
}
