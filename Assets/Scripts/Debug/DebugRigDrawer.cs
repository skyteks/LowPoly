using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif
public class DebugRigDrawer : MonoBehaviour
{
    [Range(0.1f, 20f)]
    public float lineThickness = 10f;
    public Color lineColor = Color.red;

    void OnDrawGizmosSelected()
    {
        DrawLinesToChildren(transform);
    }

    private void DrawLinesToChildren(Transform trans)
    {
#if UNITY_EDITOR
        Handles.color = lineColor;
        int count = trans.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = trans.GetChild(i);
            Handles.DrawLine(trans.position, child.position, lineThickness);
            DrawLinesToChildren(child);
        }
#endif
    }
}
