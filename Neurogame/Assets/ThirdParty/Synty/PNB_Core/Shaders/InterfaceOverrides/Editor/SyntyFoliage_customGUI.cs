using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SyntyFoliage_customGUI : ShaderGUI
{

    bool scriptExists = false;


    #region TabProperties
    private int selectedTab = 0;
    private string[] tabNames = { "Leaves", "Trunk", "Frosting", "Emission", "Wind" };


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
    //Build Surface Type options BIRP
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
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_BUILTIN_ZWrite", 0);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ZWRITE_ON");
                material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
            }
            else // Opaque
            {
                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue = (int)RenderQueue.Geometry;
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
                material.EnableKeyword("_BUILTIN_ALPHATEST_ON");
                material.EnableKeyword("_BUILTIN_AlphaClip");
            }
            else
            {
                material.DisableKeyword("_BUILTIN_ALPHATEST_ON");
                material.DisableKeyword("_BUILTIN_AlphaClip");
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

        GUIStyle backdropStyle = makeBackdrop();

        #region Create Warning button
        foreach (var obj in Resources.FindObjectsOfTypeAll<WeatherControl>())
        {
            if (obj != null)
            {
                scriptExists = true;
                break;
            }

        }

        if (!scriptExists)
        {
            GUILayout.Label("WARNING", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(backdropStyle);
            GUI.contentColor = Color.yellow;
            GUILayout.Label("Global WeatherController missing in scene", EditorStyles.boldLabel);
            GUI.contentColor = Color.white;
            if (GUILayout.Button("Add WeatherController"))
            {
                GameObject weatherController = new GameObject("WeatherController");
                weatherController.AddComponent<WeatherControl>();

            }
            EditorGUILayout.EndVertical();
        }
        #endregion

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
                material.SetFloat("_BUILTIN_Surface", 0);
                material.SetFloat("_BUILTIN_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_BUILTIN_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_BUILTIN_ZWrite", 1);
                material.DisableKeyword("_ZWRITE_ON");
                material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
                //alpha clip on
                material.SetFloat("_BUILTIN_AlphaClip", 1);
                material.EnableKeyword("_BUILTIN_ALPHATEST_ON");
                material.EnableKeyword("_BUILTIN_AlphaClip");
                //Render both faces
                material.SetFloat("_BUILTIN_CullMode", 0);
            }
            //Set URP
            else if (renderPipeType == 1)
            {
                //Make opaque
                material.SetFloat("_Surface", 0);
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
                DrawLeafSettings(materialEditor, properties);
                break;
            case 1:
                DrawTrunkSettings(materialEditor, properties);
                break;
            case 2:
                DrawFrostingSettings(materialEditor, properties);
                break;
            case 3:
                DrawEmissionSettings(materialEditor, properties);
                break;
            case 4:
                DrawWindSettings(materialEditor, properties);
                break;
        }
    }
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

    #region Vector Custom GUI Properties
    //dictionary that contains the properties to override GUI for Vector2 + Vector3 elements
    Dictionary<string, string> vecGuiProperties = new Dictionary<string, string>
        {
            { "_Leaf_Tiling", "Vector2" },
            { "_Leaf_Offset", "Vector2" },
            { "_Leaf_Normal_Tiling", "Vector2" },
            { "_Leaf_Normal_Offset", "Vector2" },
            { "_Trunk_Tiling", "Vector2" },
            { "_Trunk_Offset", "Vector2" },
            { "_Trunk_Normal_Tiling", "Vector2" },
            { "_Trunk_Normal_Offset", "Vector2" },
            { "_Emissive_Mask_Tiling", "Vector2" },
            { "_Emissive_Mask_Offset", "Vector2" },
            { "_Emissive_Mask_2_Tiling", "Vector2" },
            { "_Emissive_Mask_2_Offset", "Vector2" }
        };

    #endregion

    //Tab 1
    #region Leaf Properties
    private void DrawLeafSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();

        #region Leaf & Trunk Color Noise
        GUILayout.Label("Leaf & Trunk Color Noise", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] generalProperties =
        {
                    "_Alpha_Clip_Threshold",
                    "_Use_Color_Noise",
                    "_Color_Noise_Small_Freq",
                    "_Color_Noise_Large_Freq"

        };
        setProperties(materialEditor, properties, generalProperties, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Base Leaf
        GUILayout.Label("Leaf Base", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] leafProperties =
        {
                    "_Leaf_Texture",
                    "_Leaf_Tiling",
                    "_Leaf_Offset",
                    "_Leaf_Metallic",
                    "_Leaf_Smoothness",
                    "_Leaf_Base_Color",
                    "_Leaf_Noise_Color",
                    "_Leaf_Noise_Large_Color",
                    "_Leaf_Flat_Color"
        };
        setProperties(materialEditor, properties, leafProperties, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Leaf Normals
        GUILayout.Label("Leaf Normals", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableLeafNormals = FindProperty("_Enable_Leaf_Normal", properties);
        materialEditor.ShaderProperty(enableLeafNormals, "Enable Leaf Normals");

        if (enableLeafNormals.floatValue == 1)
        {
            string[] leafNormals =
            {
                "_Leaf_Normal",
                "_Leaf_Normal_Tiling",
                "_Leaf_Normal_Offset",
                "_Leaf_Normal_Strength"
            };
            setProperties(materialEditor, properties, leafNormals, vecGuiProperties);
        }

        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Leaf AO
        GUILayout.Label("Leaf AO", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] leafAO =
        {
                    "_Leaf_Ambient_Occlusion",
                    "_Leaf_Ambient_Occlusion_Intensity"

        };
        setProperties(materialEditor, properties, leafAO, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion
    }
    #endregion

    //Tab 2
    #region Trunk Properties
    private void DrawTrunkSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();

        #region Leaf & Trunk Color Noise
        GUILayout.Label("Leaf & Trunk Color Noise", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] generalProperties =
        {
                    "_Use_Color_Noise",
                    "_Color_Noise_Small_Freq",
                    "_Color_Noise_Large_Freq"

        };
        setProperties(materialEditor, properties, generalProperties, vecGuiProperties);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        #endregion

        #region Trunk Base
        GUILayout.Label("Trunk Base", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] trunkBase =
        {
                    "_Trunk_Texture",
                    "_Trunk_Tiling",
                    "_Trunk_Offset",
                    "_Trunk_Metallic",
                    "_Trunk_Smoothness",
                    "_Trunk_Base_Color",
                    "_Trunk_Noise_Color",
                    "_Trunk_Flat_Color_Switch"

        };
        setProperties(materialEditor, properties, trunkBase, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion
        EditorGUILayout.Separator();

        #region Trunk Normals
        GUILayout.Label("Trunk Normals", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableTrunkNormals = FindProperty("_Enable_Trunk_Normal", properties);
        materialEditor.ShaderProperty(enableTrunkNormals, "Enable Trunk Normals");

        if (enableTrunkNormals.floatValue == 1)
        {
            string[] trunkNormals =
            {
                "_Trunk_Normal",
                "_Trunk_Normal_Tiling",
                "_Trunk_Normal_Offset",
                "_Trunk_Normal_Strength"
            };
            setProperties(materialEditor, properties, trunkNormals, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Trunk Emission
        GUILayout.Label("Trunk Emission", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] trunkEmmisive =
        {
                    "_Trunk_Emissive_Mask",
                    "_Trunk_Emissive_Color"
        };
        setProperties(materialEditor, properties, trunkEmmisive, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Trunk AO
        GUILayout.Label("Trunk AO", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(backdropStyle);
        string[] trunkAO =
        {
                    "_Trunk_Ambient_Occlusion",
                    "_Trunk_Ambient_Occlusion_Intensity"
        };
        setProperties(materialEditor, properties, trunkAO, vecGuiProperties);
        EditorGUILayout.EndVertical();
        #endregion
    }
    #endregion

    //Tab 3
    #region Frosting
    private void DrawFrostingSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableFrosting = FindProperty("_Enable_Frosting", properties);
        materialEditor.ShaderProperty(enableFrosting, "Enable Frosting");

        if (enableFrosting.floatValue == 1)
        {
            string[] frosting =
            {
                "_Frosting_Color",
                "_Frosting_Falloff",
                "_Frosting_Height",
                "_Frosting_Use_World_Normals"
            };
            setProperties(materialEditor, properties, frosting, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    //Tab 4
    #region Emission Properties
    private void DrawEmissionSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();

        #region Emission
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableEmission = FindProperty("_Enable_Emission", properties);
        materialEditor.ShaderProperty(enableEmission, "Enable Emission");
        if (enableEmission.floatValue == 1)
        {
            string[] emission =
            {
                "_Emissive_Color",
                "_Emissive_2_Color",
                "_Emissive_Mask",
                "_Emissive_Mask_Tiling",
                "_Emissive_Mask_Offset",
                "_Emissive_2_Mask",
                "_Emissive_Mask_2_Tiling",
                "_Emissive_Mask_2_Offset",
                "_Emissive_Amount"
            };
            setProperties(materialEditor, properties, emission, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Pulse

        if (enableEmission.floatValue == 1)
        {
            EditorGUILayout.BeginVertical(backdropStyle);
            MaterialProperty enablePulse = FindProperty("_Enable_Pulse", properties);
            materialEditor.ShaderProperty(enablePulse, "Enable Pulse");
            if (enablePulse.floatValue == 1)
            {
                string[] emissionPulse =
                {
                "_Emissive_Pulse_Map",
                "_Pulse_Tiling",
                "_Pulse_Speed"

            };
                setProperties(materialEditor, properties, emissionPulse, vecGuiProperties);
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

    }
    #endregion

    //Tab 5
    #region Wind
    private void DrawWindSettings(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUIStyle backdropStyle = makeBackdrop();

        string[] enableVertexCol =
        {
            "_Use_Vertex_Color_Wind",
        };

        EditorGUILayout.BeginVertical(backdropStyle);
        setProperties(materialEditor, properties, enableVertexCol, vecGuiProperties);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Separator();



        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty globalWeatherController = FindProperty("_Use_Global_Weather_Controller", properties);
        materialEditor.ShaderProperty(globalWeatherController, "Use Global Weather Controller");
        //Set if statement here

        if (scriptExists)
        {
            GUILayout.Label("WeatherController found in scene", EditorStyles.boldLabel);
        }
        else
        {
            GUI.contentColor = Color.yellow;
            GUILayout.Label("Cannot activate until WeatherController in scene", EditorStyles.boldLabel);
            GUI.contentColor = Color.white;
            globalWeatherController.floatValue = 0f;

        }


        EditorGUILayout.EndVertical();


        EditorGUILayout.Separator();

        #region Breeze
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableBreeze = FindProperty("_Enable_Breeze", properties);
        materialEditor.ShaderProperty(enableBreeze, "Enable Breeze");
        if (enableBreeze.floatValue == 1)
        {
            string[] enableBreezeProp =
            {
                "_Breeze_Strength",
            };
            setProperties(materialEditor, properties, enableBreezeProp, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Light Wind
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableLightWind = FindProperty("_Enable_Light_Wind", properties);
        materialEditor.ShaderProperty(enableLightWind, "Enable Light Wind");
        if (enableLightWind.floatValue == 1)
        {
            string[] enableLightWindProp =
            {
                "_Light_Wind_Strength",
                "_Light_Wind_Y_Strength",
                "_Light_Wind_Y_Offset",
                "_Light_Wind_Use_Leaf_Fade"

            };
            setProperties(materialEditor, properties, enableLightWindProp, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Strong Wind
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableStrongWind = FindProperty("_Enable_Strong_Wind", properties);
        materialEditor.ShaderProperty(enableStrongWind, "Enable Strong Wind");
        if (enableStrongWind.floatValue == 1)
        {
            string[] enableStrongWindProp =
            {
                "_Strong_Wind_Strength",
                "_Strong_Wind_Frequency"
            };
            setProperties(materialEditor, properties, enableStrongWindProp, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.Separator();

        #region Wind Twist
        EditorGUILayout.BeginVertical(backdropStyle);
        MaterialProperty enableWindTwist = FindProperty("_Enable_Wind_Twist", properties);
        materialEditor.ShaderProperty(enableWindTwist, "Enable Wind Twist");
        if (enableWindTwist.floatValue == 1)
        {
            string[] enableWindTwistProp =
            {
                "_Wind_Twist_Strength",
                "_Gale_Bend"
            };
            setProperties(materialEditor, properties, enableWindTwistProp, vecGuiProperties);
        }
        EditorGUILayout.EndVertical();
        #endregion
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
