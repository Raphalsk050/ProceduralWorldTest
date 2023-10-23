using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;
        
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, meshHeightCurve.Evaluate(heightMap[x,z]) * heightMultiplier, topLeftZ - z);
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, z/(float)height);

                if(x < width-1 && z < height-1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex+width+1, vertexIndex+width);
                    meshData.AddTriangle(vertexIndex+width+1, vertexIndex, vertexIndex+1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    private int _triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {   
        triangles [_triangleIndex] = a;
        triangles [_triangleIndex + 1] = b;
        triangles [_triangleIndex + 2] = c;
        _triangleIndex += 3;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uvs,
            triangles = triangles,
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}