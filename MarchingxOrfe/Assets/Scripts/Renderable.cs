using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderable : MonoBehaviour
{
    public Shape shape;
    public Operation operation;
    public Color color = Color.white;
    [Range(0,10)] public float blendStrength;
    
    public Vector3 Position {
        get {
            return transform.position;
        }
    }

    public Vector3 Scale {
        get {
            Vector3 parentScale = Vector3.one;
            if (transform.parent != null && transform.parent.GetComponent<Renderable>() != null) {
                parentScale = transform.parent.GetComponent<Renderable>().Scale;
            }
            return Vector3.Scale(transform.localScale, parentScale);
        }
    }


    public enum Shape
    {
        Sphere,
        Cube,
        Torus
    };
    public enum Operation 
    {
        None,
        Blend,
        Cut,
        Mask
    }

}
