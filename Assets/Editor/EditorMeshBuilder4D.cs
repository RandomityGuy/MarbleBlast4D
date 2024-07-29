#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorMeshBuilder4D : EditorWindow {

    Mesh selectedMesh;
    MeshFilter selectedMeshFilter;
    GameObject selectedObject;

    // Extrude
    float extrudeLength;
    bool extrudeCapTop;
    bool extrudeCapBottom;
    bool extrudeVertAO;
    bool extrudeCentered;
    float extrudeTruncateRatio;
    float extrudeHoleThickness;

    // Revolve
    int revolveSegments;
    float revolveAngle;
    Vector3 revolveOffset = Vector3.zero;

    // Flat
    float holeThickness;
    float holeHeight;

    [MenuItem("4D/Mesh Builder 4D")]
    public static void Init()
    {
        EditorWindow window = GetWindow(typeof(EditorMeshBuilder4D));
        window.Show();
    }
    
    void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Selection", selectedMesh, typeof(Mesh));
        EditorGUI.EndDisabledGroup();

        // Operations

        EditorGUILayout.BeginFoldoutHeaderGroup(true, "Extrude");
        
        extrudeLength = EditorGUILayout.FloatField("Length", extrudeLength);
        extrudeCapTop = EditorGUILayout.Toggle(new GUIContent("Cap Top"), extrudeCapTop);
        extrudeCapBottom = EditorGUILayout.Toggle(new GUIContent("Cap Bottom"), extrudeCapBottom);
        extrudeVertAO = EditorGUILayout.Toggle(new GUIContent("Vertex AO"), extrudeVertAO);
        extrudeCentered = EditorGUILayout.Toggle(new GUIContent("Centered"), extrudeCentered);
        extrudeTruncateRatio = EditorGUILayout.FloatField(new GUIContent("Truncate Ratio"), extrudeTruncateRatio);
        extrudeHoleThickness = EditorGUILayout.FloatField(new GUIContent("Hole Thickness"), extrudeHoleThickness);
        if (GUILayout.Button("Extrude"))
        {
            var res = GenerateMeshes4D.Generate4DExtrude(selectedMesh, extrudeLength, null, extrudeCapTop, extrudeCapBottom, extrudeVertAO, 0, extrudeCapTop, extrudeCapBottom).mesh4D;
            SetMeshToSelection(res);
        }
        if (GUILayout.Button("Bumper Extrude"))
        {
            var res = GenerateMeshes4D.Generate4DBumperExtrude(selectedMesh, extrudeLength, extrudeTruncateRatio).mesh4D;
            SetMeshToSelection(res);
        }
        if (GUILayout.Button("Extrude Flat"))
        {
            var res = GenerateMeshes4D.Generate4DExtrudeFlat(selectedMesh, extrudeLength).mesh4D;
            SetMeshToSelection(res);
        }
        if (GUILayout.Button("Extrude Hole"))
        {
            var res = GenerateMeshes4D.Generate4DHoleExtrude(selectedMesh, extrudeHoleThickness, extrudeLength, extrudeCapTop, extrudeCapBottom).mesh4D;
            SetMeshToSelection(res);
        }
        if (GUILayout.Button("Extrude Pyramid"))
        {
            var res = GenerateMeshes4D.Generate4DPyramid(selectedMesh, extrudeLength).mesh4D;
            SetMeshToSelection(res);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.BeginFoldoutHeaderGroup(true, "Revolve");

        revolveSegments = EditorGUILayout.IntSlider("Segments", revolveSegments, 1, 100);
        revolveOffset = EditorGUILayout.Vector3Field("Offset", revolveOffset);
        revolveAngle = EditorGUILayout.Slider("Angle", revolveAngle, 0.0f, 360.0f);

        if (GUILayout.Button("Revolve"))
        {
            var res = GenerateMeshes4D.GenerateRevolve(selectedMesh, revolveSegments, revolveOffset, revolveAngle).mesh4D;
            SetMeshToSelection(res);
        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();

        // Flat Operations

        EditorGUILayout.BeginFoldoutHeaderGroup(true, "Flat Operations");
        if (GUILayout.Button("Generate Flat"))
        {
            var res = GenerateMeshes4D.Generate4DFlat(selectedMesh).mesh4D;
            SetMeshToSelection(res);
        }
        holeThickness = EditorGUILayout.FloatField("Hole Thickness", holeThickness);
        holeHeight = EditorGUILayout.FloatField("Hole Height", holeHeight);
        if (GUILayout.Button("Generate Hole Flat"))
        {
            var res = GenerateMeshes4D.Generate4DHoleFlat(selectedMesh, holeThickness, holeHeight).mesh4D;
            SetMeshToSelection(res);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //// 4D Meshbuilder operations
        //EditorGUILayout.BeginFoldoutHeaderGroup(true, "4D Operations");
        
        //EditorGUILayout.EndFoldoutHeaderGroup();

        // Primitives
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "Primitives");
        if (GUILayout.Button("Flat Cube"))
        {
            var res = GenerateMeshes4D.GenerateFlatCube().mesh4D;
            CreateObject4D(res);
        }
        if (GUILayout.Button("Flat Tetrahedron"))
        {
            var res = GenerateMeshes4D.GenerateFlatTetrahedron().mesh4D;
            CreateObject4D(res);
        }
        if (GUILayout.Button("Hypercube"))
        {
            var res = GenerateMeshes4D.GenerateHyperCube().mesh4D;
            CreateObject4D(res);
        }
        if (GUILayout.Button("Ramp Prism"))
        {
            var res = GenerateMeshes4D.GenerateRampPrism().mesh4D;
            CreateObject4D(res);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();



        //sliceW = EditorGUILayout.FloatField(EditorVolume.isVolume ? "Y" : "W", sliceW);
        //sliceV = EditorGUILayout.FloatField("V", sliceV);
        //is5D = EditorGUILayout.Toggle(new GUIContent("Use 5D"), is5D);
    }

    void CreateObject4D(Mesh4D mesh)
    {
        GameObject obj = new GameObject("Object4D");
        GameObject prefabRoot = PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot;
        if (prefabRoot)
        {
            GameObjectUtility.SetParentAndAlign(obj, prefabRoot);
        }

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

        var mr = obj.GetComponent<MeshRenderer>();
        mr.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Default.mat");

        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;
    }

    void SetMeshToSelection(Mesh4D mesh)
    {
        if (selectedObject.GetComponent<Object4D>() == null)
            selectedObject.AddComponent<Object4D>();

        if (selectedObject.GetComponent<ShadowFilter>() == null)
            selectedObject.AddComponent<ShadowFilter>();

        var mesh3d = new Mesh();
        var mesh3dShadow = new Mesh();
        var mesh3dWire = new Mesh();

        mesh.GenerateMesh(mesh3d);
        mesh.GenerateShadowMesh(mesh3dShadow);
        mesh.GenerateWireMesh(mesh3dWire);

        var mf = selectedObject.GetComponent<MeshFilter>();
        mf.mesh = mesh3d;

        var sf = selectedObject.GetComponent<ShadowFilter>();
        sf.shadowMesh = mesh3dShadow;
        sf.wireMesh = mesh3dWire;

        var mr = selectedObject.GetComponent<MeshRenderer>();
        mr.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Default.mat");
    }

    private void OnSelectionChange()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            selectedMesh = null;
            selectedMeshFilter = null;
            selectedObject = null;
        }
        else
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                selectedMeshFilter = mf;
                selectedMesh = mf.sharedMesh;
                selectedObject = go;
            }
            else
            {
                selectedMesh = null;
                selectedMeshFilter = null;
                selectedObject = null;
            }
        }
        Repaint();
    }
}
#endif