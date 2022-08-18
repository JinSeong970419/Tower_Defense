﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    Color m_OriginalColor;
    MeshRenderer m_Renderer;

    [SerializeField]
    string m_type = "terrain";

    public bool towerPlaced = false;

    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_OriginalColor = m_Renderer.material.color;
    }

    public void SetNodeType(string type) { m_type = type; }
    public string GetNodeType() { return m_type; }
    public void SetOriginalColor(Color color)
    {
        this.m_OriginalColor = color;
        m_Renderer.material.color = this.m_OriginalColor;
    }
}