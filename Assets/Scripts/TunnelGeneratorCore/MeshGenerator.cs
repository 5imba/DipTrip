using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathGenerator), typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private float wallSize = 12.0f;

    private PathGenerator pg;
    private Mesh mesh;
    private MeshFilter meshFilter;

    List<Vector3> verts;
    List<Vector2> uvs;
    List<int>[] tris;

    float tunnelWidth, tunnelHeight;
    bool initialize = false;    


    public void CreateMesh()
    {
        AssignComponents();
        CalculateMesh(0);

        //UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/tunel_test.asset");

        /*
        for (int i = 0; i < pg.path.Count; i++)
        {
            Vector3 up = pg.path[i].up * tunnelHeight;
            Vector3 right = pg.path[i].right * tunnelWidth;
            Vector3 point = pg.path[i].pos;
            Vector3 vert1 = new Vector3();
            Vector3 vert2 = new Vector3();

            for (int side = 0; side < tris.Length; side++)
            {          
                switch(side)
                {
                    case 0:
                        vert1 = (-up + -right) + point;
                        vert2 = (up + -right) + point;
                        break;
                    case 1:
                        vert1 = (up + -right) + point;
                        vert2 = (up + right) + point;
                        break;
                    case 2:
                        vert1 = (up + right) + point;
                        vert2 = (-up + right) + point;
                        break;
                    case 3:
                        vert1 = (-up + right) + point;
                        vert2 = (-up + -right) + point;
                        break;
                }

                verts.Add(vert1);
                verts.Add(vert2);

                int v = verts.Count;
                if (v > 8)
                {
                    tris[side].Add(v - 10);
                    tris[side].Add(v - 9);
                    tris[side].Add(v - 2);
                    tris[side].Add(v - 9);
                    tris[side].Add(v - 1);
                    tris[side].Add(v - 2);
                }
            }
        }

        ApplyMesh();
        */
    }

    public void AddToMesh(int amount)
    {
        int startIndex = pg.VertexMap(amount) + 1;

        CalculateMesh(startIndex);
    }

    private void CalculateMesh(int startIndex)
    {
        for (int i = startIndex; i < pg.path.Count; i++)
        {
            Vector3 up = pg.path[i].up * tunnelHeight;
            Vector3 right = pg.path[i].right * tunnelWidth;
            Vector3 point = pg.path[i].pos;
            Vector3 vert1 = new Vector3();
            Vector3 vert2 = new Vector3();
            Vector2 uv1 = new Vector2();
            Vector2 uv2 = new Vector2();

            for (int side = 0; side < tris.Length; side++)
            {
                switch (side)
                {
                    case 0:
                        vert1 = (-up + -right) + point;
                        vert2 = (up + -right) + point;
                        uv1 = new Vector2(0, vert1.z);//new Vector2(vert1.y, vert1.z);
                        uv2 = new Vector2(vert2.y- vert1.y, vert2.z);//new Vector2(vert2.y, vert2.z);
                        break;
                    case 1:
                        vert1 = (up + -right) + point;
                        vert2 = (up + right) + point;
                        uv1 = new Vector2(0, vert1.z);//new Vector2(vert1.x, vert1.z);
                        uv2 = new Vector2(vert2.x - vert1.x, vert2.z);//new Vector2(vert2.x, vert2.z);
                        break;
                    case 2:
                        vert1 = (up + right) + point;
                        vert2 = (-up + right) + point;
                        uv1 = new Vector2(0, vert1.z);//new Vector2(vert1.y, vert1.z);
                        uv2 = new Vector2(vert2.y - vert1.y, vert2.z);//new Vector2(vert2.y, vert2.z);
                        break;
                    case 3:
                        vert1 = (-up + right) + point;
                        vert2 = (-up + -right) + point;
                        uv1 = new Vector2(0, vert1.z);//new Vector2(vert1.x, vert1.z);
                        uv2 = new Vector2(vert2.x - vert1.x, vert2.z);//new Vector2(vert2.x, vert2.z);
                        break;
                }

                verts.Add(vert1);
                verts.Add(vert2);
                uvs.Add(uv1);
                uvs.Add(uv2);

                int v = verts.Count;
                if (v > 8)
                {
                    tris[side].Add(v - 10);
                    tris[side].Add(v - 9);
                    tris[side].Add(v - 2);
                    tris[side].Add(v - 9);
                    tris[side].Add(v - 1);
                    tris[side].Add(v - 2);
                }
            }
        }

        ApplyMesh();
    }

    public void RemoveFromMesh(int amount)
    {
        //verts.RemoveRange(0, amount * 8);
        //
        //int trisAmount = amount * 6;
        //for (int i = 0; i < tris.Length; i++)
        //    tris[i].RemoveRange(tris[i].Count - trisAmount - 1, trisAmount);


        for (int i = 0; i < amount; i++)
        {
            for (int v = 0; v < 8; v++)
            {
                verts.RemoveAt(0);
                uvs.RemoveAt(0);
            }

            for (int t = 0; t < 6; t++)
            {
                tris[0].RemoveAt(tris[0].Count - 1);
                tris[1].RemoveAt(tris[1].Count - 1);
                tris[2].RemoveAt(tris[2].Count - 1);
                tris[3].RemoveAt(tris[3].Count - 1);
            }
        }
    }

    private void AssignComponents()
    {
        if (!initialize)
        {
            pg = GetComponent<PathGenerator>();
            meshFilter = GetComponent<MeshFilter>();

            //Set tunnel size by screen params
            Camera cam = Camera.main;
            tunnelWidth = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0f, wallSize)).x;
            tunnelHeight = cam.ScreenToWorldPoint(new Vector3(0f, Screen.height, wallSize)).y;

            //Mesh configs
            mesh = new Mesh();
            mesh.name = "Tunnel";

            initialize = true;
        }

        verts = new List<Vector3>();
        uvs = new List<Vector2>();
        tris = new List<int>[4];

        for (int i = 0; i < tris.Length; i++) // assign tris lists
            tris[i] = new List<int>();
    }

    private void ApplyMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.subMeshCount = tris.Length;
        for (int i = 0; i < tris.Length; i++)
            mesh.SetTriangles(tris[i].ToArray(), i);
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }


    public float TunnelWidth
    {
        get
        {
            return tunnelWidth;
        }
    }
    public float TunnelHeight
    {
        get
        {
            return tunnelHeight;
        }
    }
}
