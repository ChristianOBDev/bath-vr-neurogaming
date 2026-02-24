using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SyntyWater_customGUI : ShaderGUI
{

    #region TabProperties
    private int selectedTab = 0;
    private string[] tabNames = { "Base", "Shore Foam", "Global Foam", "Waves", "Advanced" };


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
                material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
            }
            else // Opaque
            {
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_BUILTIN_ZWrite", 1);
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ZWRITE_ON");
                material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
            }

            // Render Face Type
            int renderFace = (int)material.GetFloat("_BUILTIN_CullMode");
            renderFace = EditorGUILayout.Popup("Render Face", renderFace, new string[] { "Both", "Back", "Front" });
            material.SetFloat("_BUILTIN_CullMode", renderFace);

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
                //Make transparent
                material.SetFloat("_BUILTIN_Surface", 1);
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_BUILTIN_ZWrite", 0);
                material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ZWRITE_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                //alpha clip off
                material.SetFloat("_BUILTIN_AlphaClip", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                //Render both faces
                material.SetFloat("_BUILTIN_CullMode", 2);
            }

            //Set URP
            else if (renderPipeType == 1)
            {
                //Make transparent
                material.SetFloat("_Surface", 1);
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ZWRITE_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                //alpha clip off
                material.SetFloat("_AlphaClip", 0);
                material.SetFloat("_AlphaToMask", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                //Render both faces
                material.SetFloat("_Cull", 2);
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
                DrawShoreFoamSettings(materialEditor, properties);
                break;
            case 2:
                DrawGlobalFoamSettings(materialEditor, properties);
                break;
            case 3:
                DrawWaveSettings(materialEditor, properties);
                break;
            case 4:
                DrawAdvancedSettings(materialEditor, properties);
                break;
        }
    }
    #endregion

    #region Vector Custom GUI Properties
    //dictionary that contains the properties to override GUI for Vector2 + Vector3 elements
    Dictionary<string, string> vecGuiProperties = new Dictionary<string, string>
        {
            { "_Normal_Offset", "Vector2" },
            { "_Scrolling_Texture_Direction", "Vector2" },
            { "_Scrolling_Texture_Tiling", "Vector2" },
            { "_Distortion_Direction", "Vector2" }
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

    //Tab 1
    #region Base Properties
    bool showNormalSettings = true;
    private void DrawBaseSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();
        EditorGUILayout.BeginVertical(backdropStyle);
        #region Base Properties
        GUILayout.Label("Base Properties", EditorStyles.boldLabel);
        // Add properties here
        string[] shaderProperties =
        {
            "_Smoothness",
            "_Metallic",
            "_Base_Opacity",
            "_Shallows_Opacity",
            "_Shallow_Color",
            "_Deep_Color",
            "_Very_Deep_Color",
            "_Deep_Height",
            "_Very_Deep_Height"

        };
        setProperties(materialEditor, properties, shaderProperties, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Normals
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableNormals = FindProperty("_Enable_Normals", properties);
        materialEditor.ShaderProperty(enableNormals, "Enable Normals");

        if (enableNormals.floatValue == 1)
        {
            showNormalSettings = EditorGUILayout.Foldout(showNormalSettings, "Normal Settings");
            if (showNormalSettings)
            {
                string[] normalProperties =
                {
                    "_Normal_Texture",
                    "_Normal_Offset",
                    "_Normal_Tiling",
                    "_Normal_Intensity",
                    "_Normal_Pan_Speed",
                    "_Normal_Noise_Tiling",
                    "_Normal_Noise_Intensity"

                };
                setProperties(materialEditor, properties, normalProperties, vecGuiProperties);
                EditorGUILayout.Separator();
                GUILayout.Label("Fade Distant Normals", EditorStyles.boldLabel);
                string[] fresnelMasking =
                {
                    "_Enable_Fresnel_Fade",
                    "_Fade_Distance",
                    "_Fade_Power"

                };
                setProperties(materialEditor, properties, fresnelMasking, vecGuiProperties);
            }

        }
        EditorGUILayout.EndVertical();
        #endregion

    }
    #endregion

    //Tab 2
    #region Shore Foam
    bool showShoreWaveFoamSettings = true;
    bool showShoreFoamSettings = true;
    private void DrawShoreFoamSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {

        GUIStyle backdropStyle = makeBackdrop();

        //GUILayout.Label("Shore Foam Properties", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableShoreWaveFoam = FindProperty("_Enable_Shore_Wave_Foam", properties);
        materialEditor.ShaderProperty(enableShoreWaveFoam, "Enable Shore Wave Foam");
        if (enableShoreWaveFoam.floatValue == 1)
        {
            showShoreWaveFoamSettings = EditorGUILayout.Foldout(showShoreWaveFoamSettings, "Shore Wave Foam Settings");
            if (showShoreWaveFoamSettings)
            {
                string[] shoreAnimProperties =
                {
                    "_Enable_Shore_Animation",
                    "_Animation_Offset"
                };
                setProperties(materialEditor, properties, shoreAnimProperties, vecGuiProperties);
                EditorGUILayout.Separator();

                string[] shoreWaveProperties =
                {
                    "_Shore_Wave_Speed",
                    "_Shore_Wave_Return_Amount",
                    "_Shore_Wave_Thickness"
                };
                setProperties(materialEditor, properties, shoreWaveProperties, vecGuiProperties);
                EditorGUILayout.Separator();

                string[] shoreEdgeProperties =
                {
                    "_Shore_Edge_Opacity",
                    "_Shore_Wave_Color_Tint",
                    "_Shore_Edge_Thickness",
                    "_Shore_Edge_Noise_Scale"
                };
                setProperties(materialEditor, properties, shoreEdgeProperties, vecGuiProperties);
                EditorGUILayout.Separator();

                string[] shoreFoamProperties =
                {
                    "_Shore_Foam_Noise_Scale",
                    "_Shore_Foam_Noise_Texture"


                };
                setProperties(materialEditor, properties, shoreFoamProperties, vecGuiProperties);
                EditorGUILayout.Separator();

            }

        }
        EditorGUILayout.EndVertical();


        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableShoreFoam = FindProperty("_Enable_Shore_Foam", properties);
        materialEditor.ShaderProperty(enableShoreFoam, "Enable Shore Foam");

        if (enableShoreFoam.floatValue == 1)
        {
            showShoreFoamSettings = EditorGUILayout.Foldout(showShoreFoamSettings, "Shore Foam Settings");
            if (showShoreFoamSettings)
            {

                string[] shoreFoamProperties =
                {
                    "_Shore_Small_Foam_Opacity",
                    "_Shore_Small_Foam_Tiling",
                    "_Shore_Foam_Color_Tint"


                };
                setProperties(materialEditor, properties, shoreFoamProperties, vecGuiProperties);
                EditorGUILayout.Separator();

            }
        }
        EditorGUILayout.EndVertical();

    }
    #endregion

    //Tab 3
    #region Global Foam
    bool showGlobalFoamSettings = true;
    bool showTopScrollingFoamSettings = true;
    private void DrawGlobalFoamSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();

        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableGlobalFoam = FindProperty("_Enable_Global_Foam", properties);
        materialEditor.ShaderProperty(enableGlobalFoam, "Enable Global Foam");

        if (enableGlobalFoam.floatValue == 1)
        {
            showGlobalFoamSettings = EditorGUILayout.Foldout(showGlobalFoamSettings, "Global Foam Settings");
            if (showGlobalFoamSettings)
            {
                string[] globalFoamProperties =
                {
                    "_Noise_Texture"

                };
                setProperties(materialEditor, properties, globalFoamProperties, vecGuiProperties);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableTopScrollingFoam = FindProperty("_Enable_Top_Scrolling_Texture", properties);
        materialEditor.ShaderProperty(enableTopScrollingFoam, "Enable Top Scrolling Texture");

        if (enableTopScrollingFoam.floatValue == 1)
        {
            showTopScrollingFoamSettings = EditorGUILayout.Foldout(showTopScrollingFoamSettings, "Top Scrolling Foam Settings");
            if (showTopScrollingFoamSettings)
            {
                string[] topScrollProperties =
                {
                    "_Scrolling_Texture_Direction",
                    "_Scrolling_Texture",
                    "_Scrolling_Texture_Tiling",
                    "_Scrolling_Texture_Tint",
                    "_Scrolling_Texture_Opacity"

                };
                setProperties(materialEditor, properties, topScrollProperties, vecGuiProperties);
            }
        }
        EditorGUILayout.EndVertical();

    }
    #endregion

    //Tab 4
    #region Wave Settings
    private void DrawWaveSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableOceanWaves = FindProperty("_Enable_Ocean_Wave", properties);
        materialEditor.ShaderProperty(enableOceanWaves, "Enable Ocean Waves");

        if (enableOceanWaves.floatValue == 1)
        {
            string[] globalFoamProperties =
            {
                    "_Ocean_Wave_Height",
                    "_Ocean_Wave_Speed",
                    "_Ocean_Foam_Amount",
                    "_Ocean_Foam_Opacity",
                    "_Ocean_Foam_Breakup_Tiling"

                };
            setProperties(materialEditor, properties, globalFoamProperties, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    //Tab 5
    #region Advanced
    private void DrawAdvancedSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableCaustics = FindProperty("_Enable_Caustics", properties);
        materialEditor.ShaderProperty(enableCaustics, "Enable Caustics");

        if (enableCaustics.floatValue == 1)
        {
            string[] globalCausticProperties =
            {
                    "_Caustics_Scale",
                    "_Caustics_Use_Voronoi_Noise",
                    "_Caustics_Color",
                    "_Caustics_Intensity",
                    "_Caustics_Speed",

                };

            setProperties(materialEditor, properties, globalCausticProperties, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(backdropStyle);
        GUILayout.Label("This feature is only supported in URP/HDRP", EditorStyles.boldLabel);
        MaterialProperty enableDistortion = FindProperty("_Enable_Distortion", properties);
        materialEditor.ShaderProperty(enableDistortion, "Enable Distortion");

        if (enableDistortion.floatValue == 1)
        {
            string[] globalDistortionProperties =
            {
                "_Distortion_Direction",
                "_Distortion_Speed",
                "_Distortion_Strength",
                "_Distortion_Size"

            };

            setProperties(materialEditor, properties, globalDistortionProperties, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();

    }

    #endregion

    // Build Properties
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