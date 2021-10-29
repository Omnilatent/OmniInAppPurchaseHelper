using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IAPProductData)), CanEditMultipleObjects]
public class IAPProductDataEditor : Editor
{
    IAPProductData m_Script;

    private void Awake()
    {
        m_Script = target as IAPProductData;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
