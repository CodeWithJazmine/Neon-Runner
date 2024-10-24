using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    public float fadeSpeed;
    public float fadeAmount;
    float orignalOpacity;
    Material mat;
    public bool DoFade = false;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        orignalOpacity = mat.color.a;

    }
    void Update()
    {
        if(DoFade)
        {
            Fade();
        }
        else
        {
            ResetFade();
        }
    }

    void Fade()
    {
        Color currentColor = mat.color;
        Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(currentColor.a, fadeAmount, fadeSpeed));
        mat.color = smoothColor;
    }

    void ResetFade()
    {
        Color currentColor = mat.color;
        Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(currentColor.a, orignalOpacity, fadeSpeed));
        mat.color = smoothColor;
    }
}
