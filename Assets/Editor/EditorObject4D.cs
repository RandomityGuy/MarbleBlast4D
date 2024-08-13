using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Object4D))]
class EditorObject4D : Editor
{

    public void OnSceneGUI()
    {
        if (Tools.current == Tool.Move)
        {
            Tools.hidden = true;

            var tgo = (target as Object4D).gameObject;
            var t = target as Object4D;
            EditorGUI.BeginChangeCheck();

            var pos = tgo.transform.position;
            
            if (EditorVolume.isVolume)
            {
                pos.y = -t.positionW;
            }
            else if (EditorVolume.isVolume5D)
            {
                pos.x = -t.positionW;
            }

            var newPos = Handles.PositionHandle(pos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tgo, "Move Object4D");
                if (EditorVolume.isVolume)
                {
                    t.transform.position = new Vector3(newPos.x, t.localPosition4D.y, newPos.z);
                    t.localPosition4D = new Vector4(newPos.x, t.localPosition4D.y, newPos.z, -newPos.y);
                    t.positionW = -newPos.y;
                }
                else if (EditorVolume.isVolume5D)
                {
                    t.transform.position = new Vector3(t.localPosition4D.x, newPos.y, newPos.z);
                    t.localPosition4D = new Vector4(t.localPosition4D.x, newPos.y, newPos.z, -newPos.x);
                    t.positionW = -newPos.x;
                }
                else
                {
                    tgo.transform.position = newPos;
                }
            }

        }
        else
        {
            Tools.hidden = false;
        }
    }
}

