using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject CellPlane;
    public GameObject CellHighlight;
    public Vector2 GridCoord;
    public float CoverPercent;
    public Color HighlightedColor;

    private bool ShouldBeActive;

    [SerializeField] private float CoverAccuracy = 3;
    [SerializeField] private GameObject DebugSphere;

    private Material highlightMat;

    private void Start()
    {
        highlightMat = CellHighlight.GetComponent<MeshRenderer>().material;
    }

    public void ColorHighlight(Color newColor, bool saveColor = true, bool mixColor = false)
    {
        if (mixColor)
        {
            if(HighlightedColor != Color.clear || !CellHighlight.activeInHierarchy) 
                newColor = Color.Lerp(HighlightedColor, newColor, 0.5f);
        }

        if (!CellHighlight.activeInHierarchy)
        {
            CellHighlight.SetActive(true);
            ShouldBeActive = true;
        }
        if (HighlightedColor == newColor)
            return;
        
        
        highlightMat.color = newColor;
        if (saveColor)
        {
            HighlightedColor = newColor;
        }
    }

    public void ClearCell(bool clearingGrid = false)
    {
        if (HighlightedColor == Color.clear || clearingGrid)
        {
            CellHighlight.SetActive(false);
            ShouldBeActive = false;
            HighlightedColor = Color.clear;
        }
        else
        {
            CellHighlight.SetActive(ShouldBeActive);
            highlightMat.color = HighlightedColor;
        }
    }

    private void CheckCoverIntersection(Collider c)
    {
        float rootAccuracy = Mathf.Sqrt(CoverAccuracy);
        // Cell plane is 10x10
        float increment = 10 / (CoverAccuracy-1);
        float TotalCasts = CoverAccuracy * CoverAccuracy;
        float CoverIntersects = 0f;
        for (float x = -5; x <= 5; x += increment)
        {
            for (float y = -5; y <= 5; y += increment)
            {
                Vector3 pos = new Vector3(x, 0.1f, y);
                GameObject t_spehere = Instantiate(DebugSphere, transform);
                t_spehere.transform.position = transform.position + (pos)*transform.localScale.x;

                if (c.bounds.Contains(transform.position + (pos*transform.localScale.x)))
                {
                    CoverIntersects += 1;
                }
            }
        }

        CoverPercent = CoverIntersects / TotalCasts;
    }
    
}
