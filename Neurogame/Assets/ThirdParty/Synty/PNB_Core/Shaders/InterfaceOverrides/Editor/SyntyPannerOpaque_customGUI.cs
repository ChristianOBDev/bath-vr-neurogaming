using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SyntyPannerOpaque_customGUI : ShaderGUI
{
    #region Vector Custom GUI Properties
    //dictionary that contains the properties to override GUI for Vector2 + Vector3 elements
    Dictionary<string, string> vecGuiProperties = new Dictionary<string, string>
        {
            { "_Tiling", "Vector2" },
            { "_Offset", "Vector2" }
        };

    #endregion
    #region Gui Backdrops
    private static GUIStyle makeBackdrop()
    {
        GUIStyle backdropStyle = new GUIStyle(GUI.skin.box);
        backdropStyle.padding = new RectOffset(10, 10, 10, 10);
        backdropStyle.margin = new RectOffset(5, 5, 5, 5);
        return backdropStyle;
    }
    #endregion
    #region TabProperties
    private int selectedTab = 0;
    private string[] tabNames = { "Base", "Advanced" };

    private bool showSurfaceType = false;
    private bool checkedRenderPipe = false;
    private int renderPipeType = 0;

    void CheckRenderPipeline()
    {
        var pipelineAsset = GraphicsSettings.defaultRenderPipeline;

        if (pipelineAsset == null)
        {
            Debug.Log("Using Built-In Render Pipeline (BIRP)");
            renderPipeType = 0;
        }
        else if (pipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
        {
            Debug.Log("Using Universal Render Pipeline (URP)");
            renderPipeType = 1;
        }
        else if (pipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
        {
            Debug.Log("Using High Definition Render Pipeline (HDRP)");
            renderPipeType = 1;
        }
        else
        {
            Debug.Log("Using custom render pipeline, falling back to BIRP settings");
            renderPipeType = 0;
        }
        checkedRenderPipe = true;
    }
    //Build Surface Type options URP
    public void SurfaceOptionsBIRP(MaterialEditor materialEditor)
    {
        GUIStyle backdropStyle = makeBackdrop();

        Material material = materialEditor.target as Material;
        if (material != null)
        {
            EditorGUILayout.LabelField("Surface Options", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(backdropStyle);

            // Surface Type Dropdown
            int surfaceType = (int)material.GetFloat("_BUILTIN_Surface");

            surfaceType = EditorGUILayout.Popup("Surface Type", surfaceType, new string[] { "Opaque", "Transparent" });
            material.SetFloat("_BUILTIN_Surface", surfaceType);

            // Transparent
            if (surfaceType == 1)
            {
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_BUILTIN_ZWrite", 0);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ZWRITE_ON");
            }
            else // Opaque
            {
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_BUILTIN_ZWrite", 1);
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ZWRITE_ON");
            }

            // Render Face Type
            int renderFace = (int)material.GetFloat("_BUILTIN_Cull");
            renderFace = EditorGUILayout.Popup("Render Face", renderFace, new string[] { "Both", "Back", "Front" });
            material.SetFloat("_BUILTIN_Cull", renderFace);

            // Alpha Clipping
            bool alphaClip = material.GetFloat("_BUILTIN_AlphaClip") == 1;
            alphaClip = EditorGUILayout.Toggle("Alpha Clipping", alphaClip);
            material.SetFloat("_BUILTIN_AlphaClip", alphaClip ? 1 : 0);

            if (alphaClip)
            {
                material.EnableKeyword("_ALPHATEST_ON");
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            material.shader = Shader.Find(material.shader.name);

        }
    }

    //Build Surface Type options URP
    public void SurfaceOptionsURP(MaterialEditor materialEditor)
    {
        GUIStyle backdropStyle = makeBackdrop();

        Material material = materialEditor.target as Material;
        if (material != null)
        {
            EditorGUILayout.LabelField("Surface Options", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(backdropStyle);

            // Surface Type Dropdown
            int surfaceType = (int)material.GetFloat("_Surface");

            surfaceType = EditorGUILayout.Popup("Surface Type", surfaceType, new string[] { "Opaque", "Transparent" });
            material.SetFloat("_Surface", surfaceType);

            // Transparent
            if (surfaceType == 1)
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ZWRITE_ON");
            }
            else // Opaque
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ZWRITE_ON");
            }

            // Render Face Type
            int renderFace = (int)material.GetFloat("_Cull");
            renderFace = EditorGUILayout.Popup("Render Face", renderFace, new string[] { "Both", "Back", "Front" });
            material.SetFloat("_Cull", renderFace);

            // Alpha Clipping
            bool alphaClip = material.GetFloat("_AlphaClip") == 1;
            alphaClip = EditorGUILayout.Toggle("Alpha Clipping", alphaClip);
            material.SetFloat("_AlphaClip", alphaClip ? 1 : 0);
            material.SetFloat("_AlphaToMask", alphaClip ? 1 : 0);
            if (alphaClip)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
            }


            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            material.shader = Shader.Find(material.shader.name);

        }
    }

    //used to setup the tabs and what settings to call
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        #region Surface Type Properties
        //Set Default RenderType's
        if (!checkedRenderPipe)
        {
            Material material = materialEditor.target as Material;
            CheckRenderPipeline();

            //Set BIRP
            if (renderPipeType == 0)
            {
                //Make opaque
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_BUILTIN_ZWrite", 1);
                material.DisableKeyword("_ZWRITE_ON");
                //alpha clip on
                material.SetFloat("_BUILTIN_AlphaClip", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                //Render both faces
                material.SetFloat("_BUILTIN_Cull", 0);
            }
            //Set URP
            else if (renderPipeType == 1)
            {
                //Make opaque
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ZWRITE_ON");
                //alpha clip on
                material.SetFloat("_AlphaClip", 1);
                material.SetFloat("_AlphaToMask", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                //Render both faces
                material.SetFloat("_Cull", 0);
            }

        }

        //Manual Options
        showSurfaceType = EditorGUILayout.Foldout(showSurfaceType, "Material Override properties");
        if (showSurfaceType)
        {
            //Set BIRP
            if (renderPipeType == 0)
            {
                SurfaceOptionsBIRP(materialEditor);
            }
            //Set URP
            else if (renderPipeType == 1)
            {
                SurfaceOptionsURP(materialEditor);
            }

        }
        #endregion

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        GUILayout.Space(10);

        switch (selectedTab)
        {
            case 0:
                DrawBaseSettings(materialEditor, properties);
                break;
            case 1:
                DrawAdvancedSettings(materialEditor, properties);
                break;

        }
    }
    #endregion


    #region Base Color
    private void DrawBaseSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();
        GUILayout.Label("Base Properties", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] baseProperties =
        {
            "_Albedo_Map",
            "_Albedo_Tint",
            "_Normal_Map",
            "_Normal_Intensity",
            "_Emission_Map",
            "_Emission_Tint",
            "_Emission_Intensity",
            "_Metallic",
            "_Smoothness",
            "_Tiling",
            "_Offset"

        };
        setProperties(materialEditor, properties, baseProperties, vecGuiProperties);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Separator();
        GUILayout.Label("Scrolling", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] scrollProperties =
        {
            "_Speed_X",
            "_Speed_Y"
        };
        setProperties(materialEditor, properties, scrollProperties, vecGuiProperties);
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region Advanced
    private void DrawAdvancedSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableUVDistortion = FindProperty("_Enable_UV_Distortion", properties);
        materialEditor.ShaderProperty(enableUVDistortion, "Enable UV Noise");

        if (enableUVDistortion.floatValue == 1)
        {
            string[] globalUVNoiseProperties =
            {
                "_Use_Texture",
                "_Distortion_Map",
                "_Distortion_Size",
                "_Distortion_Speed",
                "_Distortion_Strength"
            };

            setProperties(materialEditor, properties, globalUVNoiseProperties, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region Build Properties
    //Gathers list of properties to sort and display in inspector
    private static void setProperties(MaterialEditor materialEditor, MaterialProperty[] properties, string[] shaderProperties, Dictionary<string, string> vecGuiProperties)
    {
        foreach (string property in shaderProperties)
        {
            MaterialProperty propertyReference = FindProperty(property, properties);

            if (vecGuiProperties.ContainsKey(property))
            {
                string type = vecGuiProperties[property];

                if (type == "Vector2")
                {
                    Vector2 vec2Value = new Vector2(propertyReference.vectorValue.x, propertyReference.vectorValue.y);
                    vec2Value = EditorGUILayout.Vector2Field(propertyReference.displayName, vec2Value);
                    propertyReference.vectorValue = new Vector4(vec2Value.x, vec2Value.y, 0, 0);
                }
                else if (type == "Vector3")
                {
                    Vector3 vec3Value = new Vector3(propertyReference.vectorValue.x, propertyReference.vectorValue.y, propertyReference.vectorValue.z);
                    vec3Value = EditorGUILayout.Vector3Field(propertyReference.displayName, vec3Value);
                    propertyReference.vectorValue = new Vector4(vec3Value.x, vec3Value.y, vec3Value.z, 0);
                }

            }
            else
            {
                materialEditor.ShaderProperty(propertyReference, propertyReference.displayName);
            }


        }
    }
    #endregion

}
