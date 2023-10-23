using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridGenerator : MonoBehaviour
{
    public Vector2Int GridSize = Vector2Int.one;
    public float QuadSize = 1f;
    public GameObject QuadObject;
    public float HeightMultiplier = 1f;
    public float DebugSize = 0.1f;
    public int Resolution = 1;
    public Color HighestColor;
    public Color LowestColor;
    public float SnowHeight;
    private List<List<Quad>> _quadsMatrix;
    private List<List<float>> _quadsHeight;
    private bool _canDebug;
    private GUIStyle _style;
    private Vector2Int _convertedGridSize;
    
    private void Start()
    {
        QuadSize /= Resolution;
        _convertedGridSize = new Vector2Int(GridSize.x * Resolution, GridSize.y * Resolution);
        Debug.Log(_convertedGridSize);
        
        CalculateHeight();
        InstantiateQuads();
        
        _style = new GUIStyle();
        _style.fontSize = 50;
        _style.alignment = TextAnchor.MiddleCenter;
    }

    private void OnDrawGizmos()
    {
        if (!_canDebug) return;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < _quadsHeight.Count; i++)
        {
            for (int j = 0; j < _quadsHeight[i].Count; j++)
            {
                Gizmos.DrawWireCube(new Vector3(i * QuadSize,_quadsHeight[i][j],j * QuadSize), Vector3.one * DebugSize);
                //Handles.Label(new Vector3(i * QuadSize,_quadsHeight[i][j],j * QuadSize), $"{_quadsHeight[i][j]}",_style);
            }
        }
    }

    private void InstantiateQuads()
    {
        _quadsMatrix = new List<List<Quad>>();
        
        for (int i = 0; i < _convertedGridSize.y - 1; i++)
        {
            _quadsMatrix.Add(new List<Quad>());
            
            for (int j = 0; j < _convertedGridSize.x - 1; j++)
            {
                var quad = Instantiate(QuadObject, transform, false);
                var quadComponent = quad.GetComponent<Quad>();
                
                quad.GetComponent<Quad>().ColorMode = Quad.QuadColorMode.PerCorner;
                
                if (i <= 0 && j <= 0)
                    quadComponent.A = new Vector3(i * QuadSize,_quadsHeight[i][j+1],j * QuadSize);
                
                else
                    quadComponent.A = new Vector3(i * QuadSize,_quadsHeight[i][j],j * QuadSize);
                
                quadComponent.B = new Vector3((i + 1) * QuadSize,_quadsHeight[i+1][j],j * QuadSize);
                quadComponent.C = new Vector3((i + 1) * QuadSize,_quadsHeight[i+1][j+1],(j + 1) * QuadSize);
                quadComponent.D = new Vector3(i * QuadSize,_quadsHeight[i][j+1],(j + 1) * QuadSize);
                
                quadComponent.ColorA = DefineColorPerHeight(quadComponent.A.y);
                quadComponent.ColorB = DefineColorPerHeight(quadComponent.B.y);
                quadComponent.ColorC = DefineColorPerHeight(quadComponent.C.y);
                quadComponent.ColorD = DefineColorPerHeight(quadComponent.D.y);

                _quadsMatrix[i].Add(quad.GetComponent<Quad>());
            }
        }

        _canDebug = true;
    }

    private Color DefineColorPerHeight(float height)
    {
        if (height > SnowHeight)
        {
            return HighestColor;
        }
        
        return LowestColor;
    }
    
    private void CalculateHeight()
    {
        _quadsHeight = new List<List<float>>();

        for (int i = 0; i < _convertedGridSize.y; i++)
        {
            _quadsHeight.Add(new List<float>());
            
            for (int j = 0; j < _convertedGridSize.x; j++)
            {
                float randomHeight = Mathf.PerlinNoise((j + 0.1f)/Resolution, (i + 0.1f)/Resolution) * HeightMultiplier;
                Debug.Log(Mathf.PerlinNoise(j + 0.2f,i + 0.2f));
                _quadsHeight[i].Add(randomHeight);
            }
        }
    }

}
