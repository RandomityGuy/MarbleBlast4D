using log4net.Util;
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
    Vector4 skewX = new Vector4(1, 0, 0, 0);
    Vector4 skewY = new Vector4(0, 1, 0, 0);
    Vector4 skewZ = new Vector4(0, 0, 1, 0);
    Vector4 skewW = new Vector4(0, 0, 0, 1);
    int edgeHeightUnits = 1;
    bool edgeFront;
    bool edgeBack;
    bool edgeLeft;
    bool edgeRight;
    bool edgeAnth;
    bool edgeKenth;
    bool capFront;
    bool capBack;
    bool capLeft;
    bool capRight;
    bool capAnth;
    bool capKenth;
    // For the *regular* skew tiles
    Vector2Int xwWidth = new Vector2Int(1, 1);
    Vector4 endOffset = Vector4.zero;

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
        skewX = EditorGUILayout.Vector4Field("Skew X", skewX);
        skewY = EditorGUILayout.Vector4Field("Skew Y", skewY);
        skewZ = EditorGUILayout.Vector4Field("Skew Z", skewZ);
        skewW = EditorGUILayout.Vector4Field("Skew W", skewW);
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
        EditorGUILayout.BeginHorizontal();
        capFront = EditorGUILayout.Toggle(new GUIContent("Cap Front"), capFront);
        capBack = EditorGUILayout.Toggle(new GUIContent("Cap Back"), capBack);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        capLeft = EditorGUILayout.Toggle(new GUIContent("Cap Left"), capLeft);
        capRight = EditorGUILayout.Toggle(new GUIContent("Cap Right"), capRight);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        capAnth = EditorGUILayout.Toggle(new GUIContent("Cap Anth"), capAnth);
        capKenth = EditorGUILayout.Toggle(new GUIContent("Cap Kenth"), capKenth);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Flat Tile"))
        {
            GenerateTileFlat("TileFlat", tileSize.x, tileSize.y, tileSize.z, edgeFront, edgeBack, edgeLeft, edgeRight, edgeAnth, edgeKenth, capFront, capBack, capLeft, capRight, capAnth, capKenth, Matrix4x4.identity, edgeHeightUnits * 0.25f);
        }
        if (GUILayout.Button("Transformed Tile"))
        {
            var skewMatrix = new Matrix4x4(skewX, skewY, skewZ, skewW);
            GenerateTileFlat("TileTrnasform", tileSize.x, tileSize.y, tileSize.z, edgeFront, edgeBack, edgeLeft, edgeRight, edgeAnth, edgeKenth, capFront, capBack, capLeft, capRight, capAnth, capKenth, skewMatrix, edgeHeightUnits * 0.25f);
        }
        // For the *regular* skew tiles
        xwWidth = EditorGUILayout.Vector2IntField("XW Width", xwWidth);
        endOffset = EditorGUILayout.Vector4Field("End Offset", endOffset);
        if (GUILayout.Button("Skewed Tile (Regular)"))
        {
            var edgeFlags = ToFlags(true, true, edgeBack, edgeFront, edgeLeft, edgeRight, edgeAnth, edgeKenth);
            var capFlags = ToFlags(true, true, capBack, capFront, capLeft, capRight, capAnth, capKenth);
            GenerateTileSkew("TileSkew", endOffset, xwWidth, (byte)(edgeFlags), (byte)capFlags, edgeHeightUnits * 0.5f);
        }
    }

    [MenuItem("4D/Generate 4D Level Pieces")]
    public static void Generate4DPiecesMenu()
    {

    }


    private static void AddFlatCube(Mesh4D mesh4D, Matrix4x4 rotate, Vector4 offset, bool parity = false, byte shadowFlags = 0b111111)
    {
        /*
         *          B1--------B2
         *         / |       / |
         *        /  |      /  |
         *       A1--------A2  |
         *       |   |      |  |
         *       |   |      |  |
         *       |  b1------|--b2
         *       | /        | /
         *       a1---------a2
         * 
         */
        //Add a unit cube with rotation and translation
        Vector4 a1 = offset + rotate * new Vector4(-1, -1, -1, 0);
        Vector4 a2 = offset + rotate * new Vector4(1, -1, -1, 0);
        Vector4 A1 = offset + rotate * new Vector4(-1, 1, -1, 0);
        Vector4 A2 = offset + rotate * new Vector4(1, 1, -1, 0);
        Vector4 b1 = offset + rotate * new Vector4(-1, -1, 1, 0);
        Vector4 b2 = offset + rotate * new Vector4(1, -1, 1, 0);
        Vector4 B1 = offset + rotate * new Vector4(-1, 1, 1, 0);
        Vector4 B2 = offset + rotate * new Vector4(1, 1, 1, 0);
        mesh4D.AddCell(a1, a2, A1, A2, b1, b2, B1, B2, parity);
        if (shadowFlags != 0)
        {
            if ((shadowFlags & 0b10) > 0)
            {
                mesh4D.AddQuadShadow(a2, b2, a1, b1); // Bottom face
                mesh4D.AddQuadShadow(b2, a2, a1, a2); // Bottom face
            }
            if ((shadowFlags & 0b1) > 0) mesh4D.AddQuadShadow(A1, B1, A2, B2); // Top face
            if ((shadowFlags & 0b100000) > 0) mesh4D.AddQuadShadow(A1, a1, B1, b1); // Left face
            if ((shadowFlags & 0b10000) > 0) mesh4D.AddQuadShadow(a2, A2, b2, B2); // Right face
            if ((shadowFlags & 0b100) > 0) mesh4D.AddQuadShadow(a2, a1, A2, A1); // Front face
            if ((shadowFlags & 0b1000) > 0) mesh4D.AddQuadShadow(b1, b2, B1, B2); // Back face
        }
    }

    public static Mesh4DBuilder GenerateHyperCube(bool top = true, bool bottom = true, bool left = true, bool right = true,
        bool front = true, bool back = true, bool anth = true, bool kenth = true)
    {
        //Create a new mesh4D
        Mesh4D mesh4D = new Mesh4D();

        //Assemble the 8 cubic 'faces'
        if (right) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 0, 3), new Vector4(1, 0, 0, 0), false);
        if (left) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 0, 3), new Vector4(-1, 0, 0, 0), false);
        if (top) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 1, 3), new Vector4(0, 1, 0, 0), false, 0);
        if (bottom) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 1, 3), new Vector4(0, -1, 0, 0), false, 0);
        if (front) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 2, 3), new Vector4(0, 0, 1, 0), false);
        if (back) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 2, 3), new Vector4(0, 0, -1, 0), false);
        if (kenth) { AddFlatCube(mesh4D, Transform4D.PlaneRotation(0.0f, 0, 3), new Vector4(0, 0, 0, 1), true); }
        if (anth) { AddFlatCube(mesh4D, Transform4D.PlaneRotation(180.0f, 0, 3), new Vector4(0, 0, 0, -1), true); }
        return new Mesh4DBuilder(mesh4D);
    }

    public static Mesh4DBuilder GenerateHyperCubeNS(bool top = true, bool bottom = true, bool left = true, bool right = true,
    bool front = true, bool back = true, bool anth = true, bool kenth = true, bool leftShadow = false, bool rightShadow = false, bool frontShadow = false, bool backShadow = false, bool anthShadow = false, bool kenthShadow = false)
    {
        //Create a new mesh4D
        Mesh4D mesh4D = new Mesh4D();

        //Assemble the 8 cubic 'faces'
        //if (right) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 0, 3), new Vector4(1, 0, 0, 0), false, rightShadow);
        //if (left) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 0, 3), new Vector4(-1, 0, 0, 0), false, leftShadow);
        //if (top) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 1, 3), new Vector4(0, 1, 0, 0), false, false);
        //if (bottom) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 1, 3), new Vector4(0, -1, 0, 0), false, false);
        //if (front) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 2, 3), new Vector4(0, 0, 1, 0), false, frontShadow);
        //if (back) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 2, 3), new Vector4(0, 0, -1, 0), false, backShadow);
        //if (kenth) { AddFlatCube(mesh4D, Transform4D.PlaneRotation(0.0f, 0, 3), new Vector4(0, 0, 0, 1), true, kenthShadow); }
        //if (anth) { AddFlatCube(mesh4D, Transform4D.PlaneRotation(180.0f, 0, 3), new Vector4(0, 0, 0, -1), true, anthShadow); }
        if (right) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 0, 3), new Vector4(1, 0, 0, 0), false, 0b000001);
        if (left) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 0, 3), new Vector4(-1, 0, 0, 0), false, 0b000001);
        if (top) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 1, 3), new Vector4(0, 1, 0, 0), false, 0);
        if (bottom) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 1, 3), new Vector4(0, -1, 0, 0), false, 0);
        if (front) AddFlatCube(mesh4D, Transform4D.PlaneRotation(90.0f, 2, 3), new Vector4(0, 0, 1, 0), false, 0b000010);
        if (back) AddFlatCube(mesh4D, Transform4D.PlaneRotation(-90.0f, 2, 3), new Vector4(0, 0, -1, 0), false, 0b000010);
        if (kenth) { AddFlatCube(mesh4D, Transform4D.PlaneRotation(0.0f, 0, 3), new Vector4(0, 0, 0, 1), true, 0b000010); }
        if (anth) { AddFlatCube(mesh4D, Transform4D.PlaneRotation(180.0f, 0, 3), new Vector4(0, 0, 0, -1), true, 0b000010); }

        // mesh4D.AddQuadShadow(new Vector4(-1, -1, -1, 0), new Vector4(1, -1, -1, 0), new Vector4(-1, -1, 1, 0), new Vector4(1, -1, 1, 0));
        // mesh4D.AddQuadShadow(new Vector4(-1, 1, -1, 0), new Vector4(1, 1, -1, 0), new Vector4(-1, 1, 1, 0), new Vector4(1, 1, 1, 0));

        //if (left) mesh4D.AddQuadShadow(new Vector4(-1, -1, -1, 0), new Vector4(-1, -1, 1, 0), new Vector4(-1, 1, -1, 0), new Vector4(-1, 1, 1, 0));
        //if (right) mesh4D.AddQuadShadow(new Vector4(1, -1, -1, 0), new Vector4(1, -1, 1, 0), new Vector4(1, 1, -1, 0), new Vector4(1, 1, 1, 0));
        //if (back) mesh4D.AddQuadShadow(new Vector4(-1, -1, -1, 0), new Vector4(1, -1, -1, 0), new Vector4(-1, 1, -1, 0), new Vector4(1, 1, -1, 0));
        //if (front) mesh4D.AddQuadShadow(new Vector4(-1, -1, 1, 0), new Vector4(1, -1, 1, 0), new Vector4(-1, 1, 1, 0), new Vector4(1, 1, 1, 0));
        return new Mesh4DBuilder(mesh4D);
    }

    public static void GenerateTileFlat(string name, int xSize, int zSize, int wSize, bool edgeFront, bool edgeBack, bool edgeLeft, bool edgeRight, bool edgeAnth, bool edgeKenth, bool capFront, bool capBack, bool capLeft, bool capRight, bool capAnth, bool capKenth, Matrix4x4 transform, float edgeHeight = 0.25f)
    {
        var flatPart = GenerateHyperCube(true, true, !edgeLeft && capLeft, !edgeRight && capRight, !edgeFront && capFront, !edgeBack && capBack, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(xSize, 0.25f, zSize, wSize);
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
            var edge = GenerateHyperCubeNS(true, true, !edgeLeft && capLeft, !edgeRight && capRight, capFront, true, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(xSize, 0.25f + edgeHeight, 0.25f, wSize).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeBack)
        {
            var edge = GenerateHyperCubeNS(true, true, !edgeLeft && capLeft, !edgeRight && capRight, true, capBack, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(xSize, 0.25f + edgeHeight, 0.25f, wSize).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeLeft)
        {
            var edge = GenerateHyperCubeNS(true, true, capLeft, true, !edgeFront && capFront, !edgeBack && capBack, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(0.25f, 0.25f + edgeHeight, zSize, wSize).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeRight)
        {
            var edge = GenerateHyperCubeNS(true, true, true, capRight, !edgeFront && capFront, !edgeBack && capBack, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(0.25f, 0.25f + edgeHeight, zSize, wSize).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, 0);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeAnth)
        {
            var edge = GenerateHyperCubeNS(true, true, !edgeLeft && capLeft, !edgeRight && capRight, !edgeFront && capFront, !edgeBack && capBack, capAnth, anthShadow: true, kenthShadow: true).Scale(xSize, 0.25f + edgeHeight, zSize, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        if (edgeKenth)
        {
            var edge = GenerateHyperCubeNS(true, true, !edgeLeft && capLeft, !edgeRight && capRight, !edgeFront && capFront, !edgeBack && capBack, true, capKenth, anthShadow: true, kenthShadow: true).Scale(xSize, 0.25f + edgeHeight, zSize, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            mesh.AddRawIndices(edge.mesh4D.vIndices[0].ToArray(), edge.mesh4D.sIndices[0].ToArray(), edge.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(edge.mesh4D.vArray, edge.mesh4D.sArray, edge.mesh4D.wArray, Transform4D.identity);
        }
        #region Corners
        if (cornerFrontLeft)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, capFront, false, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerFrontRight)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, capFront, false, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, zSize + 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerBackLeft)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, false, capBack, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerBackRight)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, false, capBack, !edgeAnth && capAnth, !edgeKenth && capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, wSize).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, 0);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthLeft)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, !edgeFront && capFront, !edgeBack && capBack, capAnth, false).Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthRight)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, !edgeFront && capFront, !edgeBack && capBack, capAnth, false).Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(+xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthLeft)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, !edgeFront && capFront, !edgeBack && capBack, false, capKenth).Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthRight)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, !edgeFront && capFront, !edgeBack && capBack, false, capKenth).Scale(0.25f, 0.25f + edgeHeight, zSize, 0.25f).Translate(+xSize + 0.25f, (0.25f + edgeHeight) / 2, 0, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthFront)
        {
            var corner = GenerateHyperCube(true, true, !edgeLeft && capLeft, !edgeRight && capRight, capFront, false, capAnth, false).Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerAnthBack)
        {
            var corner = GenerateHyperCube(true, true, !edgeLeft && capLeft, !edgeRight && capRight, false, capBack, capAnth, false).Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthFront)
        {
            var corner = GenerateHyperCube(true, true, !edgeLeft && capLeft, !edgeRight && capRight, capFront, false, false, capKenth).Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, zSize + 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (cornerKenthBack)
        {
            var corner = GenerateHyperCube(true, true, !edgeLeft && capLeft, !edgeRight && capRight, false, capBack, false, capKenth).Scale(xSize, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(0, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        #endregion
        // The tiny 4d corner pieces
        #region 4D corners
        if (ccFrontLeftAnth)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, capFront, false, capAnth, false).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccFrontLeftKenth)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, capFront, false, false, capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccFrontRightAnth)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, capFront, false, capAnth, false).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccFrontRightKenth)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, capFront, false, false, capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, +zSize + 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackLeftAnth)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, false, capBack, capAnth, false).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackLeftKenth)
        {
            var corner = GenerateHyperCube(true, true, capLeft, false, false, capBack, false, capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(-xSize - 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackRightAnth)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, false, capBack, capAnth, false).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, -wSize - 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        if (ccBackRightKenth)
        {
            var corner = GenerateHyperCube(true, true, false, capRight, false, capBack, false, capKenth).Scale(0.25f, 0.25f + edgeHeight, 0.25f, 0.25f).Translate(xSize + 0.25f, (0.25f + edgeHeight) / 2, -zSize - 0.25f, wSize + 0.25f);
            mesh.AddRawIndices(corner.mesh4D.vIndices[0].ToArray(), corner.mesh4D.sIndices[0].ToArray(), corner.mesh4D.wIndices[0].ToArray(), 1);
            mesh.AddRawVerts(corner.mesh4D.vArray, corner.mesh4D.sArray, corner.mesh4D.wArray, Transform4D.identity);
        }
        #endregion
        var mb = new Mesh4DBuilder(mesh);
        mb.MergeVerts(0.0001f);
        mb.Affine(transform, Vector4.zero);

        var obj = CreateObject4D(mesh, name);

        var o4d = obj.GetComponent<Object4D>();
        o4d.uvOffset = new Vector4(1, 0, 1, 0);

        var mr = obj.GetComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GridCool2.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Edge.mat") };

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

    static void AddTesseract(Mesh4D mesh, Vector4[] s, Vector4[] e, bool addShadows = false, byte facetFlags = 0b11_11_11_11)
    {
        /* 
          facetFlags:
          top bottom back front left right anth kenth

         */

        // Top
        if ((facetFlags & 0b10_00_00_00) > 0)
        {
            mesh.AddCell(s[2], s[3], e[2], e[3], s[6], s[7], e[6], e[7]);
            if (addShadows) mesh.AddCellShadow(s[2], s[3], e[2], e[3], s[6], s[7], e[6], e[7]);
        }
        // Bottom
        if ((facetFlags & 0b01_00_00_00) > 0)
        {
            mesh.AddCell(s[1], s[0], e[1], e[0], s[5], s[4], e[5], e[4]);
            if (addShadows) mesh.AddCellShadow(s[1], s[0], e[1], e[0], s[5], s[4], e[5], e[4]);
        }
        // Back
        if ((facetFlags & 0b00_10_00_00) > 0)
        {
            mesh.AddCell(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7]);
            if (addShadows) mesh.AddCellShadow(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7]);
        }
        // Front
        if ((facetFlags & 0b00_01_00_00) > 0)
        {
            mesh.AddCell(e[1], e[0], e[3], e[2], e[5], e[4], e[7], e[6]);
            if (addShadows) mesh.AddCellShadow(e[1], e[0], e[3], e[2], e[5], e[4], e[7], e[6]);
        }
        // Left
        if ((facetFlags & 0b00_00_10_00) > 0)
        {
            mesh.AddCell(s[0], s[2], e[0], e[2], s[4], s[6], e[4], e[6]);
            if (addShadows) mesh.AddCellShadow(s[0], s[2], e[0], e[2], s[4], s[6], e[4], e[6]);
        }
        // Right
        if ((facetFlags & 0b00_00_01_00) > 0)
        {
            mesh.AddCell(s[3], s[1], e[3], e[1], s[7], s[5], e[7], e[5]);
            if (addShadows) mesh.AddCellShadow(s[3], s[1], e[3], e[1], s[7], s[5], e[7], e[5]);
        }
        // Anth
        if ((facetFlags & 0b00_00_00_10) > 0)
        {
            mesh.AddCell(s[1], s[0], s[3], s[2], e[1], e[0], e[3], e[2]);
            if (addShadows) mesh.AddCellShadow(s[1], s[0], s[3], s[2], e[1], e[0], e[3], e[2]);
        }
        // Kenth
        if ((facetFlags & 0b00_00_00_01) > 0)
        {
            mesh.AddCell(s[4], s[5], s[6], s[7], e[4], e[5], e[6], e[7]);
            if (addShadows) mesh.AddCellShadow(s[4], s[5], s[6], s[7], e[4], e[5], e[6], e[7]);
        }
    }

    static void AddCellShadow(Mesh4D mesh, Vector4 a1, Vector4 a2, Vector4 A1, Vector4 A2, Vector4 b1, Vector4 b2, Vector4 B1, Vector4 B2, byte shadowFlags = 0b111111)
    {
        if ((shadowFlags & 0b10) > 0) mesh.AddQuadShadow(a2, b2, a1, b1); // Bottom
        if ((shadowFlags & 0b1) > 0) mesh.AddQuadShadow(A1, B1, A2, B2); // Top
        if ((shadowFlags & 0b100000) > 0) mesh.AddQuadShadow(A1, a1, B1, b1); // Left
        if ((shadowFlags & 0b10000) > 0) mesh.AddQuadShadow(a2, A2, b2, B2); // Right
        if ((shadowFlags & 0b100) > 0) mesh.AddQuadShadow(a2, a1, A2, A1); // Front
        if ((shadowFlags & 0b1000) > 0) mesh.AddQuadShadow(b1, b2, B1, B2); // Back
    }

    static void AddTesseractShadows(Mesh4D mesh, Vector4[] s, Vector4[] e, byte facetFlags = 0b11_11_11_11)
    {
        /* 
          facetFlags:
          top bottom back front left right anth kenth

         */

        // Top
        if ((facetFlags & 0b10_00_00_00) > 0)
        {
            AddCellShadow(mesh,s[2], s[3], e[2], e[3], s[6], s[7], e[6], e[7], 0b000001);
        }
        // Bottom
        if ((facetFlags & 0b01_00_00_00) > 0)
        {
            AddCellShadow(mesh,s[1], s[0], e[1], e[0], s[5], s[4], e[5], e[4], 0b000001);
        }
        // Back
        if ((facetFlags & 0b00_10_00_00) > 0)
        {
            AddCellShadow(mesh,s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7], 0b000001);
        }
        // Front
        if ((facetFlags & 0b00_01_00_00) > 0)
        {
            AddCellShadow(mesh,e[1], e[0], e[3], e[2], e[5], e[4], e[7], e[6], 0b000001);
        }
        // Left
        if ((facetFlags & 0b00_00_10_00) > 0)
        {
            AddCellShadow(mesh,s[0], s[2], e[0], e[2], s[4], s[6], e[4], e[6], 0b000001);
        }
        // Right
        if ((facetFlags & 0b00_00_01_00) > 0)
        {
            AddCellShadow(mesh,s[3], s[1], e[3], e[1], s[7], s[5], e[7], e[5], 0b000001);
        }
        // Anth
        if ((facetFlags & 0b00_00_00_10) > 0)
        {
            AddCellShadow(mesh,s[1], s[0], s[3], s[2], e[1], e[0], e[3], e[2], 0b000001);
        }
        // Kenth
        if ((facetFlags & 0b00_00_00_01) > 0)
        {
            AddCellShadow(mesh,s[4], s[5], s[6], s[7], e[4], e[5], e[6], e[7], 0b000001);
        }
    }

    static void addSkewedColliders(GameObject obj, byte edgeFlags, byte capFlags, Vector4 offset, Vector4 ax1, Vector4 ax2, Vector4 ax3, Vector4 ax4)
    {
        /*
                    - - - - - - - - -
                    | \             | \
                    |   \           |   \
                    |     \         |     \
                    |       - - - - - - - - -
                    |       |       |       |
                    |       |   .   |       |
                    |     ax2/4     |       |
                    - - - - | - - - -       |
                      \     |         \     |
                      ax3   |           \   |
                          \ |             \ |
                            - - - - - - - - -
                         offset       ax1
         * 
         */

        FromFlags(capFlags, out bool capTop, out bool capBottom, out bool capBack, out bool capFront, out bool capLeft, out bool capRight, out bool capAnth, out bool capKenth);
        FromFlags(edgeFlags, out bool edgeTop, out bool edgeBottom, out bool edgeBack, out bool edgeFront, out bool edgeLeft, out bool edgeRight, out bool edgeAnth, out bool edgeKenth);

        // Top face
        var c1 = obj.AddComponent<ParallelotopeCollider4D>();
        c1.pos = offset + ax2 + ax3 / 2;
        c1.ax1 = ax1;
        c1.ax2 = ax4;
        c1.ax3 = ax3 / 2;

        //// Bottom face
        var c2 = obj.AddComponent<ParallelotopeCollider4D>();
        c2.pos = offset + ax3 / 2;
        c2.ax1 = ax1;
        c2.ax2 = ax4;
        c2.ax3 = ax3 / 2;

        // Right face
        if (!edgeRight && capRight)
        {
            var c3 = obj.AddComponent<ParallelotopeCollider4D>();
            c3.pos = offset + ax1 + ax3 / 2 + ax2 / 2;
            c3.ax1 = ax2 / 2;
            c3.ax2 = ax4;
            c3.ax3 = ax3 / 2;
        }

        // Left face
        if (!edgeLeft && capLeft)
        {
            var c3 = obj.AddComponent<ParallelotopeCollider4D>();
            c3.pos = offset - ax1 + ax3 / 2 + ax2 / 2;
            c3.ax1 = ax2 / 2;
            c3.ax2 = ax4;
            c3.ax3 = ax3 / 2;
        }

        // Kenth face
        if (!edgeKenth && capKenth)
        {
            var c5 = obj.AddComponent<ParallelotopeCollider4D>();
            c5.pos = offset + ax4 + ax2 / 2 + ax3 / 2;
            c5.ax1 = ax2 / 2;
            c5.ax2 = ax1;
            c5.ax3 = ax3 / 2;
        }

        // Anth face
        if (!edgeAnth && capAnth)
        {
            var c5 = obj.AddComponent<ParallelotopeCollider4D>();
            c5.pos = offset - ax4 + ax2 / 2 + ax3 / 2;
            c5.ax1 = ax2 / 2;
            c5.ax2 = ax1;
            c5.ax3 = ax3 / 2;
        }

        offset -= ax1 + ax4;

        ax1 *= 2;
        ax4 *= 2;

        // Start
        var s1 = offset;
        var s2 = offset + ax1;
        var s3 = offset + ax2;
        var s4 = offset + ax1 + ax2;
        var s5 = offset + ax4;
        var s6 = offset + ax1 + ax4;
        var s7 = offset + ax2 + ax4;
        var s8 = offset + ax1 + ax2 + ax4;
        // End
        var e1 = s1 + ax3;
        var e2 = s2 + ax3;
        var e3 = s3 + ax3;
        var e4 = s4 + ax3;
        var e5 = s5 + ax3;
        var e6 = s6 + ax3;
        var e7 = s7 + ax3;
        var e8 = s8 + ax3;

        // Back face
        if (!edgeBack && capBack)
        {
            var c7 = obj.AddComponent<CellCollider4D>();
            c7.MakeCell(s1, s2, s3, s4, s5, s6, s7, s8);
        }

        // Front face
        if (!edgeFront && capFront)
        {
            var c8 = obj.AddComponent<CellCollider4D>();
            c8.MakeCell(e1, e2, e3, e4, e5, e6, e7, e8);
        }
    }

    static byte ToFlags(bool top = false, bool bottom = false, bool back = false, bool front = false, bool left = false, bool right = false, bool anth = false, bool kenth = false)
    {
        return (byte)((top ? 1 << 7 : 0) | (bottom ? 1 << 6 : 0) | (back ? 1 << 5 : 0) | (front ? 1 << 4 : 0) | (left ? 1 << 3 : 0) | (right ? 1 << 2 : 0) | (anth ? 1 << 1 : 0) | (kenth ? 1 : 0));
    }

    static byte ToFlagsN(bool top = true, bool bottom = true, bool back = true, bool front = true, bool left = true, bool right = true, bool anth = true, bool kenth = true)
    {
        return (byte)((top ? 1 << 7 : 0) | (bottom ? 1 << 6 : 0) | (back ? 1 << 5 : 0) | (front ? 1 << 4 : 0) | (left ? 1 << 3 : 0) | (right ? 1 << 2 : 0) | (anth ? 1 << 1 : 0) | (kenth ? 1 : 0));
    }

    static void FromFlags(byte flags, out bool top, out bool bottom, out bool back, out bool front, out bool left, out bool right, out bool anth, out bool kenth)
    {
        top = ((1 << 7) & flags) > 0;
        bottom = ((1 << 6) & flags) > 0;
        back = ((1 << 5) & flags) > 0;
        front = ((1 << 4) & flags) > 0;
        left = ((1 << 3) & flags) > 0;
        right = ((1 << 2) & flags) > 0;
        anth = ((1 << 1) & flags) > 0;
        kenth = ((1 << 0) & flags) > 0;
    }

    public static void GenerateTileSkew(string name, Vector4 endOffset, Vector2Int xwWidth, byte edgeFlags, byte capFlags, float edgeHeight = 0.5f)
    {
        var mesh = new Mesh4D(2);


        /*
               e7 - - - - - - - - e8
                | \  ~            | \  ~
                |   \     ~       |   \     ~
                |     \        ~  |     \        ~
                |      e3 - - - - - - - -e4           ~
                |       |    ~    |      ~|    ~           ~
                |       |         |       |   ~     ~           ~
                |       |         |    ~  |        ~     ~           ~
                |       |         |       | ~          s7 - - - - - - - - s8
               e5 - - - | - - - -e6       |      ~      | \        ~      | \
                  \  ~  |           \  ~  |           ~ |   \           ~ |   \
                    \   | ~           \   | ~           |  ~  \           |  ~  \
                      \ |      ~        \ |      ~      |      s3 - - - - - - - - s4
                       e1 - - - - - - - -e2           ~ |       |         |       |
                             ~           ~     ~        |  ~    |         |       |
                                  ~           ~     ~   |       |         |       |
                                       ~           ~    |~      |    ~    |       |
                                            ~          s5 - - - | - - - -s6       |
                                                 ~        \     |  ~        \     |
                                                      ~     \   |       ~     \   |
                                                           ~  \ |            ~  \ |
                                                               s1 - - - - - - - - s2
         * 
         */

        var globalOff = new Vector4(0, 0, 0, 0);

        // Start
        var s1 = new Vector4(-xwWidth.x, 0, 0, -xwWidth.y) - globalOff;
        var s2 = new Vector4(xwWidth.x, 0, 0, -xwWidth.y) - globalOff;
        var s3 = new Vector4(-xwWidth.x, 0.5f, 0, -xwWidth.y) - globalOff;
        var s4 = new Vector4(xwWidth.x, 0.5f, 0, -xwWidth.y) - globalOff;
        var s5 = new Vector4(-xwWidth.x, 0, 0, xwWidth.y) - globalOff;
        var s6 = new Vector4(xwWidth.x, 0, 0, xwWidth.y) - globalOff;
        var s7 = new Vector4(-xwWidth.x, 0.5f, 0, xwWidth.y) - globalOff;
        var s8 = new Vector4(xwWidth.x, 0.5f, 0, xwWidth.y) - globalOff;
        // End
        var e1 = s1 + endOffset;
        var e2 = s2 + endOffset;
        var e3 = s3 + endOffset;
        var e4 = s4 + endOffset;
        var e5 = s5 + endOffset;
        var e6 = s6 + endOffset;
        var e7 = s7 + endOffset;
        var e8 = s8 + endOffset;

        FromFlags(capFlags, out bool capTop, out bool capBottom, out bool capBack, out bool capFront, out bool capLeft, out bool capRight, out bool capAnth, out bool capKenth);
        FromFlags(edgeFlags, out bool edgeTop, out bool edgeBottom, out bool edgeBack, out bool edgeFront, out bool edgeLeft, out bool edgeRight, out bool edgeAnth, out bool edgeKenth);


        // The base tile
        AddTesseract(mesh, new Vector4[] { s1, s2, s3, s4, s5, s6, s7, s8 }, new Vector4[] { e1, e2, e3, e4, e5, e6, e7, e8 }, false, ToFlags(true, true, !edgeBack && capBack, !edgeFront && capFront, !edgeLeft && capLeft, !edgeRight && capRight, !edgeAnth && capAnth, !edgeKenth && capKenth));
        mesh.NextSubmesh();

        // Now for the edges
        {
            var sp = new Vector4[] { s1, s2, s3, s4, s5, s6, s7, s8 };
            var ep = new Vector4[8];
            for (var i = 0; i < sp.Length; i++)
            {
                if (sp[i].x > 0)
                    sp[i].x += 0.5f;
                else
                    sp[i].x -= 0.5f;
                ep[i] = sp[i] + endOffset;
            }
            sp[2].y = sp[3].y = sp[6].y = sp[7].y = 0.5f + edgeHeight;
            ep[2].y = ep[3].y = ep[6].y = ep[7].y = endOffset.y + 0.5f + (edgeHeight);

            var yOff = new Vector4(0, edgeHeight, 0, 0);

            // now actually add them
            // Left
            if (edgeLeft)
            {
                AddTesseract(mesh, new Vector4[] { sp[0], s1, sp[2], s3 + yOff, sp[4], s5, sp[6], s7 + yOff }, new Vector4[] { ep[0], e1, ep[2], e3 + yOff, ep[4], e5, ep[6], e7 + yOff }, false, ToFlagsN(true, true, capBack, capFront));
                mesh.AddQuadShadow(s1, e1, s5, e5);
                mesh.AddQuadShadow(sp[0], ep[0], sp[4], ep[4]);
            }
            // Right
            if (edgeRight)
            {
                AddTesseract(mesh, new Vector4[] { s2, sp[1], s4 + yOff, sp[3], s6, sp[5], s8 + yOff, sp[7] }, new Vector4[] { e2, ep[1], e4 + yOff, ep[3], e6, ep[5], e8 + yOff, ep[7] }, false, ToFlagsN(true, true, capBack, capFront));
                mesh.AddQuadShadow(s2, e2, s6, e6);
                mesh.AddQuadShadow(sp[1], sp[5], ep[1], ep[5]);
            }

            sp = new Vector4[] { s1, s2, s3, s4, s5, s6, s7, s8 };
            ep = new Vector4[8];
            for (var i = 0; i < sp.Length; i++)
            {
                if (sp[i].w > 0)
                    sp[i].w += 0.5f;
                else
                    sp[i].w -= 0.5f;
                ep[i] = sp[i] + endOffset;
            }
            sp[2].y = sp[3].y = sp[6].y = sp[7].y = 0.5f + (edgeHeight);
            ep[2].y = ep[3].y = ep[6].y = ep[7].y = endOffset.y + 0.5f + (edgeHeight);

            // now actually add them
            // Kenth
            if (edgeKenth)
            {
                AddTesseract(mesh, new Vector4[] { s1, sp[0], s3 + yOff, sp[2], s2, sp[1], s4 + yOff, sp[3] }, new Vector4[] { e1, ep[0], e3 + yOff, ep[2], e2, ep[1], e4 + yOff, ep[3] }, false, ToFlagsN(true, true, capBack, capFront));
                mesh.AddQuadShadow(s1, e1, s2, e2);
                mesh.AddQuadShadow(sp[0], ep[0], sp[1], ep[1]);
            }
            // Anth
            if (edgeAnth)
            {
                AddTesseract(mesh, new Vector4[] { sp[4], s5, sp[6], s7 + yOff, sp[5], s6, sp[7], s8 + yOff }, new Vector4[] { ep[4], e5, ep[6], e7 + yOff, ep[5], e6, ep[7], e8 + yOff }, false, ToFlagsN(true, true, capBack, capFront));
                mesh.AddQuadShadow(s5, e5, s6, e6);
                mesh.AddQuadShadow(sp[4], ep[4], sp[5], ep[5]);
            }
        }
        // Edge corners
        {
            var yOff = new Vector4(0, edgeHeight, 0, 0);
            var sp = new Vector4[] { s1, s2, s3, s4, s5, s6, s7, s8 };
            var ep = new Vector4[8];
            for (var i = 0; i < sp.Length; i++)
            {
                if (sp[i].w > 0)
                    sp[i].w += 0.5f;
                else
                    sp[i].w -= 0.5f;
                ep[i] = sp[i] + endOffset;
            }
            sp[2].y = sp[3].y = sp[6].y = sp[7].y = 0.5f + (edgeHeight);
            ep[2].y = ep[3].y = ep[6].y = ep[7].y = endOffset.y + 0.5f + (edgeHeight);
            var xOff = new Vector4(0.5f, 0, 0, 0);
            var st = new Vector4[] { s1, sp[0], s3 + yOff, sp[2], s1 + xOff, sp[0] + xOff, s3 + xOff + yOff, sp[2] + xOff };
            var et = new Vector4[] { e1, ep[0], e3 + yOff, ep[2], e1 + xOff, ep[0] + xOff, e3 + xOff + yOff, ep[2] + xOff };
            for (var i = 0; i < sp.Length; i++)
            {
                st[i].x -= 0.5f;
                et[i].x -= 0.5f;
            }
            // Left Kenth
            if (edgeLeft && edgeKenth)
            {
                AddTesseract(mesh, st, et, false, ToFlagsN(true, true, capBack, capFront));
                // Volume view shadows
                mesh.AddQuadShadow(st[0], et[0], st[1], et[1]);
                mesh.AddQuadShadow(st[4], et[4], st[5], et[5]);
                mesh.AddQuadShadow(st[0], et[0], st[4], et[4]);
                mesh.AddQuadShadow(st[1], et[1], st[5], et[5]);
                if (capBack)
                    mesh.AddQuadShadow(st[0], st[4], st[1], st[5]);
                if (capFront)
                    mesh.AddQuadShadow(et[4], et[0], et[5], et[1]);
                // Normal view shadows
                // Side faces
                // mesh.AddQuadShadow(st[0], et[0], st[2], et[2]); // X = -1.5 W = -1.0
                mesh.AddQuadShadow(st[1], et[1], st[3], et[3]); // X = -1.5 W = -1.5
                mesh.AddQuadShadow(st[4], et[4], st[6], et[6]); // X = 1.0 W = 1.0
                // mesh.AddQuadShadow(st[4], et[4], st[6], et[6]); 
                // mesh.AddQuadShadow(st[5], et[5], st[7], et[7]);
                // Top faces
                mesh.AddQuadShadow(st[3], st[2], et[3], et[2]);
                mesh.AddQuadShadow(st[3], st[7], et[3], et[7]);
                // Left face
                // mesh.AddQuadShadow(et[2], st[2], et[0], st[0]);
                // Right face
                //mesh.AddQuadShadow(et[6], st[6], et[4], st[4]);
                // Top face
                // mesh.AddQuadShadow(et[2], st[2], et[6], st[6]);
                // Back face
                //mesh.AddQuadShadow(st[0], st[4], st[2], st[6]);
                // Front face
                //mesh.AddQuadShadow(et[0], et[4], et[2], et[6]);

                // W normal view
                //mesh.AddQuadShadow(st[7], et[7], st[5], et[5]);
                //mesh.AddQuadShadow(st[6], et[6], st[7], et[7]);
                // mesh.AddQuadShadow(st[0], et[0], st[7], et[7]);
                //if (capBack)
                //    mesh.AddQuadShadow(st[1], st[2], st[7], st[6]);
                //if (capFront)
                //    mesh.AddQuadShadow(et[2], et[1], et[6], et[7]);
                //// Right face
                //mesh.AddQuadShadow(et[6], st[6], et[4], st[4]);
                //// Top face
                //mesh.AddQuadShadow(et[2], st[2], et[6], st[6]);
                //// Back face
                //mesh.AddQuadShadow(st[1], st[4], st[2], st[6]);
                //// Front face
                //mesh.AddQuadShadow(et[1], et[4], et[2], et[6]);
            }
            for (var i = 0; i < sp.Length; i++)
            {
                st[i].x += 0.5f + 2 * xwWidth.x;
                et[i].x += 0.5f + 2 * xwWidth.x;
            }
            // Right Kenth
            if (edgeRight && edgeKenth)
            {
                AddTesseract(mesh, st, et, false, ToFlagsN(true, true, capBack, capFront));
            }
            for (var i = 0; i < sp.Length; i++)
            {
                st[i].w += 0.5f + 2 * xwWidth.y;
                et[i].w += 0.5f + 2 * xwWidth.y;
            }
            // Right Anth
            if (edgeRight && edgeAnth)
            {
                AddTesseract(mesh, st, et, false, ToFlagsN(true, true, capBack, capFront));
            }
            for (var i = 0; i < sp.Length; i++)
            {
                st[i].x -= 0.5f + 2 * xwWidth.x;
                et[i].x -= 0.5f + 2 * xwWidth.x;
            }
            // Left Anth
            if (edgeLeft && edgeAnth)
            {
                AddTesseract(mesh, st, et, false, ToFlagsN(true, true, capBack, capFront));
            }
        }

        var obj = CreateObject4D(mesh, name);

        var o4d = obj.GetComponent<Object4D>();
        o4d.uvOffset = new Vector4(xwWidth.x / 2, 0.25f, 1, xwWidth.y / 2 );
        o4d.uvTransform.m11 = 0; // Don't let Y affect anything else
        o4d.uvTransform.m20 = -endOffset.x / endOffset.z;
        o4d.uvTransform.m23 = -endOffset.w / endOffset.z;


        // Its time for colliders
        // Main collider
        addSkewedColliders(obj, edgeFlags, capFlags, Vector4.zero, new Vector4(xwWidth.x, 0, 0, 0), new Vector4(0, 0.5f, 0, 0), endOffset, new Vector4(0, 0, 0, xwWidth.y));
        // Edge piece colliders - suffering
        {
            // Left edge
            // Left
            if (edgeLeft)
                addSkewedColliders(obj, 0, 0b111111, new Vector4(-xwWidth.x - 0.25f, 0, 0, 0), new Vector4(0.25f, 0, 0, 0), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(0, 0, 0, xwWidth.y));
            // Right edge
            // Left
            if (edgeRight)
                addSkewedColliders(obj, 0, 0b111111, new Vector4(xwWidth.x + 0.25f, 0, 0, 0), new Vector4(0.25f, 0, 0, 0), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(0, 0, 0, xwWidth.y));
            // Anth edge
            // Left
            if (edgeAnth)
                addSkewedColliders(obj, 0, 0b111111, new Vector4(0, 0, 0, -xwWidth.y - 0.25f), new Vector4(0, 0, 0, 0.25f), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(xwWidth.x, 0, 0, 0));
            // Kenth edge
            // Left
            if (edgeKenth)
                addSkewedColliders(obj, 0, 0b111111, new Vector4(0, 0, 0, xwWidth.y + 0.25f), new Vector4(0, 0, 0, 0.25f), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(xwWidth.x, 0, 0, 0));
        }
        // Edge corner pieces
        {
            // Left Anth
            if (edgeLeft && edgeAnth)
                addSkewedColliders(obj, 0, 0b110101, new Vector4(-xwWidth.x - 0.25f, 0, 0, -xwWidth.y - 0.25f), new Vector4(0.25f, 0, 0, 0), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(0, 0, 0, 0.25f));
            // Right Anth
            if (edgeRight && edgeAnth)
                addSkewedColliders(obj, 0, 0b110110, new Vector4(xwWidth.x + 0.25f, 0, 0, -xwWidth.y - 0.25f), new Vector4(0.25f, 0, 0, 0), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(0, 0, 0, 0.25f));
            // Right Kenth
            if (edgeRight && edgeKenth)
                addSkewedColliders(obj, 0, 0b111010, new Vector4(xwWidth.x + 0.25f, 0, 0, xwWidth.y + 0.25f), new Vector4(0.25f, 0, 0, 0), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(0, 0, 0, 0.25f));
            // Left Kenth
            if (edgeLeft && edgeKenth)
                addSkewedColliders(obj, 0, 0b111001, new Vector4(-xwWidth.x - 0.25f, 0, 0, xwWidth.y + 0.25f), new Vector4(0.25f, 0, 0, 0), new Vector4(0, (0.5f + edgeHeight) * 2, 0, 0) / 2, endOffset, new Vector4(0, 0, 0, 0.25f));
        }


        //var bc = obj.AddComponent<BoxCollider4D>();
        //bc.pos = new Vector4(0, (0.25f + edgeHeight) / 2, endOffset.z / 2, 0f);
        //bc.size = new Vector4(xwWidth.x, (0.25f + edgeHeight) / 2, endOffset.z / 2, xwWidth.y / 2);
        //bc.basis.SetColumn(2, endOffset.normalized);

        var mr = obj.GetComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GridCool2.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Edge.mat") };
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

