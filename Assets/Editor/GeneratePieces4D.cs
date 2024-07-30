using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GeneratePieces4D : MonoBehaviour
{
    [MenuItem("4D/Generate 4D Level Pieces")]
    public static void Generate4DPiecesMenu()
    {
        GenerateTileFlat("TileFlat", 3, 6, 9, true, true, true, true, true, true);
    }
    
    public static void GenerateTileFlat(string name, int xSize, int zSize, int wSize, bool edgeFront, bool edgeBack, bool edgeLeft, bool edgeRight, bool edgeAnth, bool edgeKenth, float edgeHeight = 0.25f)
    {
        var flatPart = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f, zSize, wSize);
        var mesh = new Mesh4D(2);
        mesh.AddRawIndices(flatPart.mesh4D.vIndices[0].ToArray(), flatPart.mesh4D.sIndices[0].ToArray(), flatPart.mesh4D.wIndices[0].ToArray(), 0);
        mesh.AddRawVerts(flatPart.mesh4D.vArray, flatPart.mesh4D.sArray, flatPart.mesh4D.wArray, Transform4D.identity);
        mesh.NextSubmesh();
        if (edgeFront)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize + 0.375f, 0.25f + edgeHeight, 0.25f, wSize + 0.375f).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.125f, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeBack)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize + 0.375f, 0.25f + edgeHeight, 0.25f, wSize + 0.375f).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.125f, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeLeft)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize + 0.375f, wSize + 0.375f).Translate(-xSize - 0.125f, (0.25f + edgeHeight) / 2, 0, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeRight)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize + 0.375f, wSize + 0.375f).Translate(xSize + 0.125f, (0.25f + edgeHeight) / 2, 0, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeAnth)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize + 0.375f, 0.25f + edgeHeight, zSize + 0.375f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, 0, -wSize - 0.125f);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeKenth)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize + 0.375f, 0.25f + edgeHeight, zSize + 0.375f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, 0, wSize + 0.125f);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }

        var obj = CreateObject4D(mesh);

        var mr = obj.GetComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Default.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Edge.mat") };

        // Now for the colliders
        var flatCollider = obj.AddComponent<BoxCollider4D>();
        flatCollider.pos = Vector4.zero;
        flatCollider.size = new Vector4(xSize, 0.25f, zSize, wSize);

        if (edgeFront)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, zSize + 0.125f, 0);
            edgeCollider.size = new Vector4(xSize + 0.375f, 0.25f + edgeHeight, 0.25f, wSize + 0.375f);
        }
        if (edgeBack)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, -zSize - 0.125f, 0);
            edgeCollider.size = new Vector4(xSize + 0.375f, 0.25f + edgeHeight, 0.25f, wSize + 0.375f);
        }
        if (edgeLeft)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.125f, (0.25f + edgeHeight) / 2, 0, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize + 0.375f, wSize + 0.375f);
        }
        if (edgeRight)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.125f, (0.25f + edgeHeight) / 2, 0, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize + 0.375f, wSize + 0.375f);
        }
        if (edgeAnth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, 0, -wSize - 0.125f);
            edgeCollider.size = new Vector4(xSize + 0.375f, 0.25f + edgeHeight, zSize + 0.375f, 0.25f);
        }
        if (edgeKenth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, 0, wSize + 0.125f);
            edgeCollider.size = new Vector4(xSize + 0.375f, 0.25f + edgeHeight, zSize + 0.375f, 0.25f);
        }

        PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/" + name + ".prefab");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/Prefabs/" + name + ".prefab", ImportAssetOptions.ForceUpdate);
    }

    static GameObject CreateObject4D(Mesh4D mesh)
    {
        GameObject obj = new GameObject("Object4D");

        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<ShadowFilter>();
        obj.AddComponent<Object4D>();


        var mesh3d = new Mesh();
        var mesh3dShadow = new Mesh();
        var mesh3dWire = new Mesh();

        mesh.GenerateMesh(mesh3d);
        mesh.GenerateShadowMesh(mesh3dShadow);
        mesh.GenerateWireMesh(mesh3dWire);

        var mf = obj.GetComponent<MeshFilter>();
        mf.mesh = mesh3d;

        var sf = obj.GetComponent<ShadowFilter>();
        sf.shadowMesh = mesh3dShadow;
        sf.wireMesh = mesh3dWire;

        return obj;
    }
}

