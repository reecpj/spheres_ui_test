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
    public Transform SpheresParent;
    // The first slider for controlling all spheres is present in the scene
    public GameObject SliderPanel;
    public GameObject SpherePrefab;
    // This opaque material is shared across all opaque spheres to reduce draw calls
    public Material OpaqueSphereMaterial;
    private Material _previousOpaqueMaterial;
    // This shader is used on each material instance of each transparent sphere.
    // Changing the OpaqueSphereMaterial will update the transparent material
    // instances, using this shader.
    public Shader FadeSphereShader;

    private SliderOpacityPanel[] _opacityPanels;
    private Transform[] _sphereTransforms;
    void Start()
    {
        // set up spheres with sliders
        var sliderParent = SliderPanel.transform.parent;
        _opacityPanels = new SliderOpacityPanel[Constants.NumSpheres];
        _sphereTransforms = new Transform[Constants.NumSpheres];
        for (int i = 0; i < Constants.NumSpheres; ++i)
        {
            var sphereGameObject = GameObject.Instantiate(SpherePrefab, SpheresParent);
            sphereGameObject.name = "Sphere " + (i + 1);
            var sliderPanel = GameObject.Instantiate(SliderPanel, sliderParent);
            var sliderOpacityPanel = sliderPanel.GetComponent<SliderOpacityPanel>();
            sliderOpacityPanel.Setup(sphereGameObject, sphereGameObject.name);
            sliderOpacityPanel.ChangeMaterial(OpaqueSphereMaterial, FadeSphereShader);
            _opacityPanels[i] = sliderOpacityPanel;
            _sphereTransforms[i] = sphereGameObject.transform;
        }
        // set up master slider
        var masterSlider = SliderPanel.AddComponent<MasterSlider>();
        masterSlider.Setup(_opacityPanels);
        
        ViewportSizeChange.OnViewportChanged += ReflowSpheresToFitScreen;
    }

    /// <summary>
    /// When the screen window changes, create a grid of vectors in 2D space
    /// on the right side of the screen, and project from the camera onto a
    /// plane to find the sphere's 3d positions
    /// </summary>
    /// <param name="screenRes"></param>
    private void ReflowSpheresToFitScreen(Vector2 screenRes)
    {
        var camera = Camera.main;
        float pixelScaleFactor = HelperFunctions.GetUiScaleFactor(screenRes, 
            Constants.ReferenceScreenResolution, Constants.SphereScaleMatchWidthOrHeight);
        float padding = pixelScaleFactor * Constants.SpherePixelsPadding;
        float sphereWorldScale = Constants.SphereScale;
        // fit more spheres on screen when in vertical aspect ratio
        if (screenRes.y / screenRes.x > 1.3f)
            sphereWorldScale *= 0.8f;
        Plane gridPlane = new Plane(Vector3.forward, 0);
        float pixelOffset = (sphereWorldScale * pixelScaleFactor * Constants.PixelsPerSphere) + padding;
        var startPixelCoord = new Vector2(
            screenRes.x * Constants.HorizontalScreenPanelWidth + padding + pixelOffset * 0.5f, 
            screenRes.y - padding - pixelOffset);
        Vector3 positionScreenSpace = new Vector3(startPixelCoord.x, startPixelCoord.y, 0);
        foreach (var sphereTransform in _sphereTransforms)
        {
            // get 3D position from screen space grid coordinate
            Ray fromCamera = camera.ScreenPointToRay(positionScreenSpace);
            gridPlane.Raycast(fromCamera, out var t);
            sphereTransform.position = fromCamera.origin + fromCamera.direction * t;
            sphereTransform.localScale = new Vector3(sphereWorldScale, sphereWorldScale, sphereWorldScale);
            positionScreenSpace.x += pixelOffset;
            // if close to the edge of the screen, move down a row
            if (positionScreenSpace.x > screenRes.x - padding - pixelOffset * 0.5f)
            {
                positionScreenSpace.x = startPixelCoord.x;
                positionScreenSpace.y -= pixelOffset;
            }
        }
    }

    void Update()
    {
        if (OpaqueSphereMaterial != _previousOpaqueMaterial)
        {
            ChangeMaterial();
        }
    }

    [ContextMenu("Update Opaque Material or Fade shader")]
    private void ChangeMaterial()
    {
        foreach (var opacityPanel in _opacityPanels)
        {
            opacityPanel.ChangeMaterial(OpaqueSphereMaterial, FadeSphereShader);
        }

        _previousOpaqueMaterial = OpaqueSphereMaterial;
    }
}
