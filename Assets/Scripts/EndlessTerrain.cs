using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public Transform viewerTransform;
    public static Vector2 viewerPosition;
    public const int maxViewDistance = 1000;
    public Material mapMaterial;
    private int _chunkSize;
    private int _visibleChunksInViewDistance;
    internal static MapGenerator _mapGenerator;
    private Dictionary<Vector2,TerrainChunk> _terrainChunkDictionary = new ();
    private List<TerrainChunk> _terrainChunksVisibleLastUpdate = new();

    private void Start()
    {
        _mapGenerator = FindFirstObjectByType<MapGenerator>();
        _chunkSize = MapGenerator.mapChunkSize - 1;
        _visibleChunksInViewDistance = Mathf.RoundToInt(maxViewDistance / _chunkSize);
    }

    private void Update(){
        viewerPosition = new Vector2(viewerTransform.position.x, viewerTransform.position.z);
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        //convert the range of coordinates from (-240,0,240) to a range of (-1,0,1)
        int currentViewCoordX = Mathf.RoundToInt(viewerPosition.x / _chunkSize);
        int currentViewCoordZ = Mathf.RoundToInt(viewerPosition.y / _chunkSize);

        for (int i = 0; i < _terrainChunksVisibleLastUpdate.Count; i++)
        {
            _terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        _terrainChunksVisibleLastUpdate.Clear();

        //get the near visible chunks coordinates and verifies if already have a chunk instantiated, if not
        //instantiates a new chunk into this coord
        for(int zOffset = -_visibleChunksInViewDistance; zOffset <= _visibleChunksInViewDistance; zOffset++)
        {
            for(int xOffset = -_visibleChunksInViewDistance; xOffset <= _visibleChunksInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentViewCoordX + xOffset, currentViewCoordZ + zOffset);

                if(_terrainChunkDictionary.ContainsKey(viewedChunkCoord) && !_terrainChunksVisibleLastUpdate.Contains(_terrainChunkDictionary[viewedChunkCoord]))
                {
                    _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if(_terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        _terrainChunksVisibleLastUpdate.Add(_terrainChunkDictionary[viewedChunkCoord]);
                    }
                } 
                else{
                    _terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,_chunkSize,transform, mapMaterial));
                }
            }
        }
    }
}

public class TerrainChunk{
    private GameObject _meshObject;
    private Vector2 _position;
    private Bounds _bounds;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    public TerrainChunk(Vector2 coord, int size, Transform parent, Material material){
        _position = coord * size;
        _bounds = new Bounds(_position, Vector2.one * size);
        Vector3 positionV3 = new Vector3(_position.x,0,_position.y);

        _meshObject = new GameObject("TerrainChunk");
        _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
        _meshFilter = _meshObject.AddComponent<MeshFilter>();
        _meshRenderer.material = material;

        _meshObject.transform.position = positionV3;
        _meshObject.transform.SetParent(parent);
        EndlessTerrain._mapGenerator.RequestMapData(OnMapDataReceived);
        SetVisible(false);
    }

    private void OnMapDataReceived(MapData mapData)
    {
        EndlessTerrain._mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
    }

    private void OnMeshDataReceived(MeshData meshData)
    {
        _meshFilter.mesh = meshData.CreateMesh();
    }

    public void UpdateTerrainChunk(){
        float viewerDistFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(EndlessTerrain.viewerPosition));
        bool visible = viewerDistFromNearestEdge <= EndlessTerrain.maxViewDistance;
        SetVisible(visible);
    }
    public void SetVisible(bool visible){
        _meshObject.SetActive(visible);
    }

    public bool IsVisible(){
        return _meshObject.activeSelf;
    }
    
    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int _lod;

        public LODMesh(int lod)
        {
            _lod = lod;
        }
        

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            EndlessTerrain._mapGenerator.RequestMeshData(mapData,);
        }
    }
}

