using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class WeatherControl : MonoBehaviour
{
    [Range(0,1)]
    public float windIntensity;
    [Range(0,1)]
    public float weatherIntensity;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShaderGlobals();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawIcon(this.transform.position,"../Synty/PNB_Core/Textures/SyntyLogo.png", true);
        Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 4f);
        Gizmos.DrawLine(this.transform.position + this.transform.forward * 4f, this.transform.position + this.transform.forward * 3f + this.transform.right * 0.5f);
        Gizmos.DrawLine(this.transform.position + this.transform.forward * 4f, this.transform.position + this.transform.forward * 3f - this.transform.right * 0.5f);
        UpdateShaderGlobals();
    }

    private void UpdateShaderGlobals()
    {
        Shader.SetGlobalVector("_WindDirection", this.transform.forward);
        Shader.SetGlobalFloat("_GaleStrength", weatherIntensity);
        Shader.SetGlobalFloat("_WindIntensity", windIntensity);
    }
}