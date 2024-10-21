using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using Common.Unity.Drawing;

using System.Linq;
using UnityEditor.UI;
using UnityEngine.Rendering.Universal;
using UnityEditor.VersionControl;

using MarchingCubesProject;

public class MeshGenerator : MonoBehaviour
{
    private int pointCount;
    private Vector3[] pointDataArray;
    private float[] boundingBox = new float[6];

    public UniversalRendererData UniversalRendererData;
    public Material material;
    public MARCHING_MODE mode = MARCHING_MODE.CUBES;
    public bool smoothNormals = false;
    public bool drawNormals = false;
    private List<GameObject> meshes = new List<GameObject>();
    private NormalRenderer normalRenderer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (UniversalRendererData != null)
            {
                // 遍历 renderer features，找到 ShadowMaskRenderFeature
                foreach (var feature in UniversalRendererData.rendererFeatures)
                {
                    if (feature is ShadowMaskRenderFeature shadowMaskFeature)
                    {
                        shadowMaskFeature.EnableShadowCatching();
                    }
                }
            }
        }
    }

    public void ToGenerateMeshHandler(int pointCount, Vector3[] pointDataArray)
    {
        this.pointCount = pointCount;
        this.pointDataArray = pointDataArray;

        CalculateBoundingBox();

        //Set the mode used to create the mesh.
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        Marching marching = null;
        if(mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingTertrahedron();
        else
            marching = new MarchingCubes();

        //Surface is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        //The target value does not have to be the mid point it can be any value with in the range.
        marching.Surface = 0.0f;

        //The size of voxel array.
        int width = 128;
        int height = 128;
        int depth = 128;

        var voxels = new VoxelArray(width, height, depth);

        foreach (Vector3 point in this.pointDataArray)
        {
            // 将点坐标映射到体素网格的索引
            int x = Mathf.FloorToInt((point.x - this.boundingBox[0]) / (this.boundingBox[3] - this.boundingBox[0]) * (width - 1));
            int y = Mathf.FloorToInt((point.y - this.boundingBox[1]) / (this.boundingBox[4] - this.boundingBox[1]) * (height - 1));
            int z = Mathf.FloorToInt((point.z - this.boundingBox[2]) / (this.boundingBox[5] - this.boundingBox[2]) * (depth - 1));

            // 将该体素设为1，表示这个位置有数据
            voxels[x, y, z] = 1.0f;
        }

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels.Voxels, verts, indices);

        //Create the normals from the voxel.

        if (smoothNormals)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                //Presumes the vertex is in local space where
                //the min value is 0 and max is width/height/depth.
                Vector3 p = verts[i];

                float u = p.x / (width - 1.0f);
                float v = p.y / (height - 1.0f);
                float w = p.z / (depth - 1.0f);

                Vector3 n = voxels.GetNormal(u, v, w);

                normals.Add(n);
            }

            normalRenderer = new NormalRenderer();
            normalRenderer.DefaultColor = Color.red;
            normalRenderer.Length = 0.25f;
            normalRenderer.Load(verts, normals);
        }

        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 v = verts[i];
            v.x = Mathf.Lerp(this.boundingBox[0], this.boundingBox[3], v.x / (float)(width - 1));
            v.y = Mathf.Lerp(this.boundingBox[1], this.boundingBox[4], v.y / (float)(height - 1));
            v.z = Mathf.Lerp(this.boundingBox[2], this.boundingBox[5], v.z / (float)(depth - 1));
            verts[i] = v;
        }

        var position = new Vector3(-width / 2, -height / 2, -depth / 2);

        CreateMesh32(verts, normals, indices, position);

    }

    private void CalculateBoundingBox()
    {
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = this.pointDataArray[i];
            this.boundingBox[0] = worldPos.x < this.boundingBox[0] ? worldPos.x : this.boundingBox[0];
            this.boundingBox[1] = worldPos.y < this.boundingBox[1] ? worldPos.y : this.boundingBox[1];
            this.boundingBox[2] = worldPos.z < this.boundingBox[2] ? worldPos.z : this.boundingBox[2];
            this.boundingBox[3] = worldPos.x > this.boundingBox[3] ? worldPos.x : this.boundingBox[3];
            this.boundingBox[4] = worldPos.y > this.boundingBox[4] ? worldPos.y : this.boundingBox[4];
            this.boundingBox[5] = worldPos.z > this.boundingBox[5] ? worldPos.z : this.boundingBox[5];
        }

        //float offset = (float)0.25;

        //this.boundingBox[0] -= offset;
        //this.boundingBox[1] -= offset;
        //this.boundingBox[2] -= offset;
        //this.boundingBox[3] += offset;
        //this.boundingBox[4] += offset;
        //this.boundingBox[5] += offset;
    }

    private void CreateMesh32(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);

        //if (normals.Count > 0)
        //    mesh.SetNormals(normals);
        //else
        //    mesh.RecalculateNormals();

        mesh.RecalculateBounds();

        // 创建 GameObject 并应用生成的 Mesh
        GameObject obj = new GameObject("GeneratedMesh");
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        Material mat = new Material(Shader.Find("Shader Graphs/GeneratedMesh"));
        meshRenderer.material = mat;
    }

    //private void OnRenderObject()
    //{
    //    if(normalRenderer != null && meshes.Count > 0 && drawNormals)
    //    {
    //        var m = meshes[0].transform.localToWorldMatrix;

    //        normalRenderer.LocalToWorld = m;
    //        normalRenderer.Draw();
    //    }
            
    //}

}
