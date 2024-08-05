#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Unity.Collections;
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
        if (GUILayout.Button("Extrude Truncated Pyramid"))
        {
            var res = GenerateMeshes4D.Generate4DTruncatedPyramid(selectedMesh, extrudeLength, extrudeTruncateRatio, null, extrudeCapBottom, extrudeCapTop, extrudeVertAO, 0, extrudeCentered, extrudeCapBottom, extrudeCapTop).mesh4D;
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

        EditorGUILayout.BeginFoldoutHeaderGroup(true, "Misc Operations");
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
        if (GUILayout.Button("Merge Selected"))
        {
            MergeMeshes4D(Selection.gameObjects.Where(x => x.GetComponent<Object4D>() != null).Select(x => x.GetComponent<Object4D>()).ToArray());
        }
        if (GUILayout.Button("Save Mesh"))
        {
            var meshFileName = EditorUtility.SaveFilePanel("Save Mesh as", "Assets/Meshes4D", "", "mesh");
            if (meshFileName != "" && meshFileName != null)
            {
                meshFileName = Path.GetRelativePath(".", meshFileName);
                var mf = Selection.activeGameObject.GetComponent<MeshFilter>();
                var sf = Selection.activeGameObject.GetComponent<ShadowFilter>();

                Mesh tempMesh = (Mesh)UnityEngine.Object.Instantiate(mf.mesh);
                Mesh tempShadowMesh = (Mesh)UnityEngine.Object.Instantiate(sf.shadowMesh);
                Mesh tempWireMesh = (Mesh)UnityEngine.Object.Instantiate(sf.wireMesh);


                AssetDatabase.CreateAsset(tempMesh, meshFileName);
                AssetDatabase.CreateAsset(tempShadowMesh, meshFileName.Replace(".mesh", "_s.mesh"));
                AssetDatabase.CreateAsset(tempWireMesh, meshFileName.Replace(".mesh", "_w.mesh"));

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(meshFileName, ImportAssetOptions.ForceUpdate);
            }
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

    public static void MergeMeshes4D(Object4D[] objs4D)
    {
        //Get the primitive mesh from a GameObject.
        // Debug.Log("Generating " + name + "...");
        //GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Modeling/" + name + ".prefab");
        //Debug.Assert(model != null, "Could not find model '" + name + "' in Modeling folder.");
        MeshRenderer[] renderers = objs4D.Select(x => x.GetComponent<MeshRenderer>()).ToArray();

        //Awaken all Object4D components
        foreach (Object4D obj4D in objs4D)
        {
            obj4D.Awake();
            obj4D.transform.hasChanged = true;
        }

        //Get a material count for all the meshes in the object
        Dictionary<Material, int> matMap = new();
        Dictionary<int, Material> matMapRev = new();
        foreach (MeshRenderer renderer in renderers)
        {
            if (!renderer.enabled) { continue; }
            Material[] sharedMaterials = renderer.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; ++i)
            {
                Material sharedMaterial = sharedMaterials[i];
                if (!matMap.ContainsKey(sharedMaterial))
                {
                    var cnt = matMap.Count;
                    matMap[sharedMaterial] = matMap.Count;
                    matMapRev[cnt] = sharedMaterial;
                }
            }
        }

        //Create a new Mesh4D
        Mesh4D mesh4D = new Mesh4D(matMap.Count);

        //Add meshes for each materials
        foreach (MeshRenderer renderer in renderers)
        {
            //Get meshes
            if (!renderer.enabled) { continue; }
            Mesh mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
            Mesh mesh_s = renderer.GetComponent<ShadowFilter>()?.shadowMesh;
            Mesh mesh_w = renderer.GetComponent<ShadowFilter>()?.wireMesh;
            Debug.Assert(mesh != null, "Mesh renderer '" + renderer.name + "' did not have a mesh");
            Object4D obj4D = renderer.GetComponent<Object4D>();
            Debug.Assert(obj4D != null, "Mesh renderer '" + renderer.name + "' did not have an Object4D");
            Material[] sharedMaterials = renderer.sharedMaterials;

            //Merge all indices
            for (int i = 0; i < sharedMaterials.Length; ++i)
            {
                Material sharedMaterial = sharedMaterials[i];
                int[] vIndices = mesh.GetIndices(i);
                int[] sIndices = (mesh_s ? mesh_s.GetIndices(i) : new int[0]);
                int[] wIndices = (mesh_w ? mesh_w.GetIndices(i) : new int[0]);
                int subMesh = matMap[sharedMaterial];
                mesh4D.AddRawIndices(vIndices, sIndices, wIndices, subMesh);
            }

            //Merge all vertices
            Mesh.MeshDataArray meshData = Mesh.AcquireReadOnlyMeshData(mesh);
            NativeArray<Mesh4D.Vertex4D> vVerts = meshData[0].GetVertexData<Mesh4D.Vertex4D>(0);
            Debug.Assert(vVerts.Length % 4 == 0, "Invalid number of vertices");
            if (mesh_s)
            {
                Mesh.MeshDataArray meshData_s = Mesh.AcquireReadOnlyMeshData(mesh_s);
                Mesh.MeshDataArray meshData_w = Mesh.AcquireReadOnlyMeshData(mesh_w);
                NativeArray<Mesh4D.Shadow4D> sVerts = meshData_s[0].GetVertexData<Mesh4D.Shadow4D>(0);
                NativeArray<Mesh4D.Shadow4D> wVerts = meshData_w[0].GetVertexData<Mesh4D.Shadow4D>(0);
                Debug.Assert(sVerts.Length % 3 == 0, "Invalid number of vertices");
                Debug.Assert(mesh.subMeshCount == mesh_s.subMeshCount);
                mesh4D.AddRawVerts(vVerts, sVerts, wVerts, obj4D.WorldTransform4D());
                meshData_s.Dispose();
            }
            else
            {
                mesh4D.AddRawVerts(vVerts, obj4D.WorldTransform4D());
            }

            //Dispose the mesh data correctly
            meshData.Dispose();
        }
        var mb = new Mesh4DBuilder(mesh4D);

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

        mb.MergeVerts(0.001f);

        mb.mesh4D.GenerateMesh(mesh3d);
        mb.mesh4D.GenerateShadowMesh(mesh3dShadow);
        mb.mesh4D.GenerateWireMesh(mesh3dWire);

        var mf = obj.GetComponent<MeshFilter>();
        mf.mesh = mesh3d;

        var sf = obj.GetComponent<ShadowFilter>();
        sf.shadowMesh = mesh3dShadow;
        sf.wireMesh = mesh3dWire;

        var mr = obj.GetComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[matMapRev.Count];
        for (int i = 0; i < matMapRev.Count; i++)
        {
            mr.sharedMaterials[i] = matMapRev[i];
        }

        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;
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