using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AdjustGridCellSize : MonoBehaviour
{
    private RectTransform _rectTransform;
    private GridLayoutGroup _gridLayoutGroup;
    private int _numCells;
    private Transform[] _sphereTransforms;

    void OnRectTransformDimensionsChange()
    {
        AdjustCellSize();
    }

    public void UpdateNumCells(int numCells)
    {
        _numCells = numCells;
        StoreSphereTransforms();
        AdjustCellSize();
    }

    public void AdjustCellSize()
    {
        if (_rectTransform == null)
        { // initialise variables from hierarchy
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
            StoreSphereTransforms();
        }
        var rectSize = _rectTransform.rect.size;
        float rectArea = rectSize.x * rectSize.y;
        float rectAspectRatio = rectSize.x / rectSize.y;


        // get roughly how many spheres would fit on each side
        // of the grid
        float squareRootNumCells = Mathf.Sqrt(_numCells);
        // take the average of the aspect ratio and 1, so the following
        // numbers of cells are split fairly evenly between the two axes
        float rectAspectRatioSplit = (rectAspectRatio + 1) * 0.5f;
        var numHorizontalCells = Mathf.RoundToInt(squareRootNumCells * rectAspectRatioSplit);
        var numVerticalCells = Mathf.RoundToInt(squareRootNumCells / rectAspectRatioSplit);

        // product of width and height may not cover all required cells
        if (numHorizontalCells * numVerticalCells < _numCells)
        {
            // if not, increase the lower of the two numbers,
            // to increase their product as much as possible
            if (rectSize.x > rectSize.y)
            {
                numVerticalCells++;
            }
            else
            {
                numHorizontalCells++;
            }
        }

        // set the number of rows for the grid layout
        _gridLayoutGroup.constraintCount = numVerticalCells;

        // get the new cell size in pixels for the canvas
        var cellSizeFromWidth = rectSize.x / numHorizontalCells;
        var cellSizeFromHeight = rectSize.y / numVerticalCells;
        var cellSize = Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);
        cellSize -= _gridLayoutGroup.spacing.x;
        _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

        // update the visual mesh
        var sphereScale = new Vector3(cellSize, cellSize, cellSize);
        foreach (var sphereTransform in _sphereTransforms)
        {
            sphereTransform.localScale = sphereScale;
        }
    }
    private void StoreSphereTransforms()
    {
        // Store an array of the sphere's transforms, so that resizing the window
        // does not have to spend the time getting the sphere mesh
        var sphereMeshes = GetComponentsInChildren<MeshRenderer>();

        var numSpheres = sphereMeshes.Length;
        _sphereTransforms = new Transform[numSpheres];
        for (var sphereIndex = 0; sphereIndex < numSpheres; sphereIndex++)
        {
            var sphereMesh = sphereMeshes[sphereIndex];
            _sphereTransforms[sphereIndex] = sphereMesh.transform;
        }
    }
}
