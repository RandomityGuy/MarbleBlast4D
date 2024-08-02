﻿using System;
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
    bool capFront;
    bool capBack;
    bool capLeft;
    bool capRight;
    bool capAnth;
    bool capKenth;

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
            GenerateTileFlat("TileFlat", tileSize.x, tileSize.y, tileSize.z, edgeFront, edgeBack, edgeLeft, edgeRight, edgeAnth, edgeKenth, capFront, capBack, capLeft, capRight, capAnth, capKenth, edgeHeightUnits * 0.25f);
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

    public static void GenerateTileFlat(string name, int xSize, int zSize, int wSize, bool edgeFront, bool edgeBack, bool edgeLeft, bool edgeRight, bool edgeAnth, bool edgeKenth, bool capFront, bool capBack, bool capLeft, bool capRight, bool capAnth, bool capKenth, float edgeHeight = 0.25f)
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

