using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adjusts the cell size of the Grid Layout Group component
/// to ensure all grid cells fit on screen, as well as determining
/// the number of rows to best fit the UI element's size.
/// Sets the spheres on each cell to be that cell size
/// </summary>
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
        // cache the sphere transforms in each cell for performance when resizing
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
        // get usable canvas size for grid
        var rectSize = _rectTransform.rect.size;
        var padding = _gridLayoutGroup.padding;
        rectSize.x -= padding.left - padding.right;
        rectSize.y -= padding.bottom - padding.top;

        // get roughly how many spheres would fit on each side
        // of the grid
        float rectAspectRatio = rectSize.x / rectSize.y;
        float roughNumColumns = Mathf.Sqrt(_numCells*rectAspectRatio);
        float roughtNumRows = Mathf.Sqrt(_numCells/rectAspectRatio);
        var numColumns = Mathf.FloorToInt(roughNumColumns);
        var numRows = Mathf.FloorToInt(roughtNumRows);

        // product of the estimated numbers of rows and columns
        // may not be enough to cover all required cells
        while (numColumns * numRows < _numCells)
        {
            // if not, increase the lower of the two numbers,
            // to increase their product as much as possible
            if (numColumns > numRows)
            {
                numRows++;
            }
            else
            {
                numColumns++;
            }
        }

        // set the number of rows for the grid layout -
        // Unity calculates number of columns from this
        _gridLayoutGroup.constraintCount = numRows;

        // get the new cell size in pixels for the canvas
        // using the smaller of the two available options
        var cellSizeFromWidth = rectSize.x / numColumns;
        var cellSizeFromHeight = rectSize.y / numRows;
        var cellSize = Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);
        // make it smaller so some space between the cells is possible
        cellSize -= _gridLayoutGroup.spacing.x;
        _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

        // update the visual sphere meshes to be the correct size
        // avoid bad scale values
        if (float.IsInfinity(cellSize))
            return;
        var sphereScale = new Vector3(cellSize, cellSize, cellSize);
        foreach (var sphereTransform in _sphereTransforms)
        {
            sphereTransform.localScale = sphereScale;
        }
    }

    void OnDestroy()
    {
        // AdjustCellSize can be called after OnDestroy,
        // in which case trying to scale the spheres causes
        // a null reference error. This fixes the problem.
        _sphereTransforms = null;
    }
    
    // Store an array of the sphere's transforms, so that resizing the window
    // does not have to spend the time getting the sphere mesh
    private void StoreSphereTransforms()
    {
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
