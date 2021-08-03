using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class RenderableContainer : MonoBehaviour
{
    public List<Renderable> renderables = new List<Renderable>();

    private void Awake()
    {
        renderables = GetComponentsInChildren<Renderable>().ToList();
        renderables.Sort ((a, b) => a.operation.CompareTo (b.operation));
    }
}
