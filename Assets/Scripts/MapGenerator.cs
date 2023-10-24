using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode{
        NoiseMap,
        ColorMap,
        Mesh
    }
    public DrawMode drawMode;
    public const int mapChunkSize = 241;
    [Range(0,6)] public int levelOfDetail;
    public float noiseScale;
    public AnimationCurve meshHeightCurve;

    [Range(0,17)] public int octaves;
    [Range(0,1)] public float persistence;
    [Range(0,1000)] public float heightMultiplier;
    
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool autoUpdate;
    public TerrainType[] regions;

    private Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new();
    private Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStartDelegate = delegate
        {
            MapDataThread(callback);
        };

        new Thread(threadStartDelegate).Start();
    }

    private void MapDataThread(Action<MapData> callback)
    {
        MapData mapData = GenerateMapData();
        lock(_mapDataThreadInfoQueue)
        {
            _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public static void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {

    }

    private void MeshDataThread(MapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, levelOfDetail);
        lock (_meshDataThreadInfoQueue)
        {
            _meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if(_mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = _mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (_meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = _meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    private MapData GenerateMapData(){
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize*mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height){
                        colorMap [y*mapChunkSize+x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }

    void OnValidate(){
        if(lacunarity < 1){
            lacunarity = 1;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public float width;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
