using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GeneratePieces4D : EditorWindow
{
    Vector3Int tileSize = Vector3Int.one;
    int edgeHeightUnits = 1;
    bool edgeFront;
    bool edgeBack;
    bool edgeLeft;
    bool edgeRight;
    bool edgeAnth;
    bool edgeKenth;

    [MenuItem("4D/Level Builder 4D")]
    public static void Init()
    {
        EditorWindow window = GetWindow(typeof(GeneratePieces4D));
        window.titleContent = new GUIContent("Level Builder 4D");
        window.Show();
    }

    private void OnGUI()
    {
        tileSize = EditorGUILayout.Vector3IntField("Tile Size", tileSize);
        edgeHeightUnits = EditorGUILayout.IntField("Edge Height", edgeHeightUnits);
        EditorGUILayout.BeginHorizontal();
        edgeFront = EditorGUILayout.Toggle(new GUIContent("Edge Front"), edgeFront);
        edgeBack = EditorGUILayout.Toggle(new GUIContent("Edge Back"), edgeBack);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        edgeLeft = EditorGUILayout.Toggle(new GUIContent("Edge Left"), edgeLeft);
        edgeRight = EditorGUILayout.Toggle(new GUIContent("Edge Right"), edgeRight);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        edgeAnth = EditorGUILayout.Toggle(new GUIContent("Edge Anth"), edgeAnth);
        edgeKenth = EditorGUILayout.Toggle(new GUIContent("Edge Kenth"), edgeKenth);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Flat Tile"))
        {
            GenerateTileFlat("TileFlat", tileSize.x, tileSize.y, tileSize.z, edgeFront, edgeBack, edgeLeft, edgeRight, edgeAnth, edgeKenth, edgeHeightUnits * 0.25f);
        }
    }

    [MenuItem("4D/Generate 4D Level Pieces")]
    public static void Generate4DPiecesMenu()
    {
        GenerateTileFlat("TileFlat", 3, 3, 3, true, true, true, true, true, true);
    }
    
    public static void GenerateTileFlat(string name, int xSize, int zSize, int wSize, bool edgeFront, bool edgeBack, bool edgeLeft, bool edgeRight, bool edgeAnth, bool edgeKenth, float edgeHeight = 0.25f)
    {
        var flatPart = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f, zSize, wSize);
        var mesh = new Mesh4D(2);
        mesh.AddRawIndices(flatPart.mesh4D.vIndices[0].ToArray(), flatPart.mesh4D.sIndices[0].ToArray(), flatPart.mesh4D.wIndices[0].ToArray(), 0);
        mesh.AddRawVerts(flatPart.mesh4D.vArray, flatPart.mesh4D.sArray, flatPart.mesh4D.wArray, Transform4D.identity);
        mesh.NextSubmesh();
        var cornerFrontLeft = edgeFront && edgeLeft;
        var cornerFrontRight = edgeFront && edgeRight;
        var cornerBackLeft = edgeBack && edgeLeft;
        var cornerBackRight = edgeBack && edgeRight;
        var cornerAnthLeft = edgeAnth && edgeLeft;
        var cornerAnthRight = edgeAnth && edgeRight;
        var cornerKenthLeft = edgeKenth && edgeLeft;
        var cornerKenthRight = edgeKenth && edgeRight;
        var cornerAnthFront = edgeAnth && edgeFront;
        var cornerAnthBack = edgeAnth && edgeBack;
        var cornerKenthFront = edgeKenth && edgeFront;
        var cornerKenthBack = edgeKenth && edgeBack;
        var ccFrontLeftAnth = edgeFront && edgeLeft && edgeAnth;
        var ccFrontLeftKenth = edgeFront && edgeLeft && edgeKenth;
        var ccFrontRightAnth = edgeFront && edgeRight && edgeAnth;
        var ccFrontRightKenth = edgeFront && edgeRight && edgeKenth;
        var ccBackLeftAnth = edgeBack && edgeLeft && edgeAnth;
        var ccBackLeftKenth = edgeBack && edgeLeft && edgeKenth;
        var ccBackRightAnth = edgeBack && edgeRight && edgeAnth;
        var ccBackRightKenth = edgeBack && edgeRight && edgeKenth;

        // Edge pieces
        if (edgeFront)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, 0.25f, wSize).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeBack)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, 0.25f, wSize).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeLeft)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize, wSize).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeRight)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize, wSize).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeAnth)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, zSize, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeKenth)
        {
            var edge = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, zSize, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        #region Corners
        if (cornerFrontLeft)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerFrontRight)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerBackLeft)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerBackRight)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthLeft)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthRight)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(+xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthLeft)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthRight)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(+xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthFront)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthBack)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthFront)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthBack)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        #endregion
        // The tiny 4d corner pieces
        if (ccFrontLeftAnth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, - wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccFrontLeftKenth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccFrontRightAnth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccFrontRightKenth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackLeftAnth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackLeftKenth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackRightAnth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackRightKenth)
        {
            var corner = GenerateMeshes4D.GenerateHyperCube().Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }

        var obj = CreateObject4D(mesh, name);

        var mr = obj.GetComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tile.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Edge.mat") };

        // Now for the colliders
        var flatCollider = obj.AddComponent<BoxCollider4D>();
        flatCollider.pos = Vector4.zero;
        flatCollider.size = new Vector4(xSize, 0.25f, zSize, wSize);

        if (edgeFront)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, 0.25f, wSize);
        }
        if (edgeBack)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, 0.25f, wSize);
        }
        if (edgeLeft)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize, wSize);
        }
        if (edgeRight)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize, wSize);
        }
        if (edgeAnth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, zSize, 0.25f);
        }
        if (edgeKenth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, zSize, 0.25f);
        }
        if (cornerFrontLeft)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, wSize);
        }
        if (cornerFrontRight)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, wSize);
        }
        if (cornerBackLeft)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, wSize);
        }
        if (cornerBackRight)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, wSize);
        }
        if (cornerAnthLeft)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize, 0.25f);
        }
        if (cornerAnthRight)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize, 0.25f);
        }
        if (cornerKenthLeft)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize, 0.25f);
        }
        if (cornerKenthRight)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, zSize, 0.25f);
        }
        if (cornerAnthFront)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, -wSize - 0.25f);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (cornerAnthBack)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (cornerKenthFront)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, wSize + 0.25f);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (cornerKenthBack)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, +wSize + 0.25f);
            edgeCollider.size = new Vector4(xSize, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccFrontLeftAnth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, -wSize - 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccFrontLeftKenth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, wSize + 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccFrontRightAnth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, -wSize - 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccFrontRightKenth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, wSize + 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccBackLeftAnth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccBackLeftKenth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccBackRightAnth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
        if (ccBackRightKenth)
        {
            var edgeCollider = obj.AddComponent<BoxCollider4D>();
            edgeCollider.pos = new Vector4(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            edgeCollider.size = new Vector4(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f);
        }
    }

    static GameObject CreateObject4D(Mesh4D mesh, string name)
    {
        GameObject obj = new GameObject(name);

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

