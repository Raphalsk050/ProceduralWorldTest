using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridGeneratorWithProceduralMesh : MonoBehaviour
{
    public Vector2Int GridSize = Vector2Int.one;
    public float QuadSize = 1f;
    public Material MeshMaterial;
    public Vector3[] MeshVertices = new Vector3[4];
    public GameObject ProceduralMeshObject;
    public float HeightMultiplier = 1f;
    public float DebugSize = 0.1f;
    public int Resolution = 1;
    public Color HighestColor;
    public Color LowestColor;
    public float SnowHeight;
    private List<List<SimpleProceduralMesh>> _meshMatrix;
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

    /*private void OnDrawGizmos()
    {
        if (!_canDebug) return;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < _quadsHeight.Count; i++)
        {
            for (int j = 0; j < _quadsHeight[i].Count; j++)
            {
                Gizmos.DrawWireCube(new Vector3(i * QuadSize,_quadsHeight[i][j],j * QuadSize), Vector3.one * DebugSize);
                Handles.Label(new Vector3(i * QuadSize,_quadsHeight[i][j],j * QuadSize), $"{_quadsHeight[i][j]}",_style);
            }
        }
    }*/

    private void InstantiateQuads()
    {
        _meshMatrix = new List<List<SimpleProceduralMesh>>();
        
        for (int i = 0; i < _convertedGridSize.y - 1; i++)
        {
            _meshMatrix.Add(new List<SimpleProceduralMesh>());
            
            for (int j = 0; j < _convertedGridSize.x - 1; j++)
            {
                var mesh = Instantiate(ProceduralMeshObject, transform, false);
                
                var meshComponent = mesh.GetComponent<SimpleProceduralMesh>();

                meshComponent.MeshVertices[0] = new Vector3(i * QuadSize,_quadsHeight[i][j],j * QuadSize);
                meshComponent.MeshVertices[1] = new Vector3((i + 1) * QuadSize,_quadsHeight[i+1][j],j * QuadSize);
                meshComponent.MeshVertices[2] = new Vector3((i) * QuadSize,_quadsHeight[i][j+1],(j + 1) * QuadSize);
                meshComponent.MeshVertices[3] = new Vector3((i + 1) * QuadSize,_quadsHeight[i+1][j+1],(j + 1) * QuadSize);
                
                mesh.GetComponent<MeshRenderer>().material.SetFloat("CurrentHeight", mesh.transform.localPosition.y);

                meshComponent.Initialize();
                _meshMatrix[i].Add(mesh.GetComponent<SimpleProceduralMesh>());
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
    
    public void Initialize () {
        
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                
            }
        }
    }
}
