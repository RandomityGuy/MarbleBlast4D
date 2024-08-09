#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorSlicer : EditorWindow {
    public static float sliceW = 0.0f;
    public static float sliceV = 0.0f;
    public static bool is5D = false;
    public static float prevSliceW = 0.0f;
    public static float prevSliceV = 0.0f;
    public static bool prevIs5D = false;

    [MenuItem("4D/Slicer Window...")]
    public static void Init() {
        EditorWindow window = GetWindow(typeof(EditorSlicer));
        window.Show();
    }

    void OnGUI() {
        var currentWAxis = "W";
        if (EditorVolume.isVolume)
            currentWAxis = "Y";
        if (EditorVolume.isVolume5D && !is5D)
            currentWAxis = "X";
        sliceW = EditorGUILayout.FloatField(currentWAxis, sliceW);
        sliceV = EditorGUILayout.FloatField("V", sliceV);
        is5D = EditorGUILayout.Toggle(new GUIContent("Use 5D"), is5D);
        if (GUILayout.Button("Slice To Selection"))
        {
            var sel = Selection.activeGameObject;
            if (sel != null)
            {
                var o4d = sel.gameObject.GetComponent<Object4D>();
                if (o4d != null)
                {
                    if (EditorVolume.isVolume)
                    {
                        sliceW = o4d.localPosition4D.y;
                    }
                    else if (EditorVolume.isVolume5D && !is5D)
                    {
                        sliceW = o4d.localPosition4D.x;
                    }
                    else
                    {
                        sliceW = o4d.localPosition4D.w;
                    }
                }
            }
        }
    }

    void OnInspectorUpdate() {
        if (sliceW != prevSliceW || sliceV != prevSliceV) {
            Shader.SetGlobalFloat("_EditorSliceW", sliceW);
            Shader.SetGlobalFloat("_EditorSliceV", sliceV);
            SceneView.RepaintAll();
            prevSliceW = sliceW;
            prevSliceV = sliceV;
        }
        if (is5D != prevIs5D) {
            EditorVolume.UpdateShaders();
            SceneView.RepaintAll();
            prevIs5D = is5D;
        }
    }
}
#endif
