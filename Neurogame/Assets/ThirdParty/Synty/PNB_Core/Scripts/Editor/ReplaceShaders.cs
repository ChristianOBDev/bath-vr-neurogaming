using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Synty.PolygonVikings2.Scripts.Editor
{
    [InitializeOnLoad]
    public class ReplaceShaders : EditorWindow
    {
        private StyleSheet _editorStyle;
        
        private Shader _oldShader = null;
        private Shader _newShader = null;

        private Dictionary<string, string> _parameterMatchList = new Dictionary<string, string>();
        
        private List<string> _oldShaderProperties = new List<string>();
        private List<string> _newShaderProperties = new List<string>();
        
        private void Awake()
        {
            InitializeEditorWindow();
        }
        
        private void InitializeEditorWindow()
        {
            _editorStyle = Resources.Load<StyleSheet>("Styles/EditorStyles");
        }
        
        [MenuItem("Synty/Replace Shader")]
        public static void ShowWindow() {
            // Create the window instance
            ReplaceShaders window = GetWindow<ReplaceShaders>("Synty Replace Shaders");
        }

        private void DoReplaceShader()
        {
            string[] assets = Selection.assetGUIDs;
            foreach (string guid in assets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Material loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (loadedMaterial != null)
                {
                    Dictionary<string, object> storedProperties = new Dictionary<string, object>();
                    if (loadedMaterial.shader == _oldShader)
                    {
                        for (int i = 0; i < loadedMaterial.shader.GetPropertyCount(); i++)
                        {
                            switch (loadedMaterial.shader.GetPropertyType(i))
                            {
                                case ShaderPropertyType.Color:
                                    storedProperties.Add(loadedMaterial.shader.GetPropertyName(i), loadedMaterial.GetColor(loadedMaterial.shader.GetPropertyName(i)));
                                    break;
                                case ShaderPropertyType.Vector:
                                    storedProperties.Add(loadedMaterial.shader.GetPropertyName(i), loadedMaterial.GetVector(loadedMaterial.shader.GetPropertyName(i)));
                                    break;
                                case ShaderPropertyType.Float:
                                    storedProperties.Add(loadedMaterial.shader.GetPropertyName(i), loadedMaterial.GetFloat(loadedMaterial.shader.GetPropertyName(i)));
                                    break;
                                case ShaderPropertyType.Range:
                                    storedProperties.Add(loadedMaterial.shader.GetPropertyName(i), loadedMaterial.GetFloat(loadedMaterial.shader.GetPropertyName(i)));
                                    break;
                                case ShaderPropertyType.Texture:
                                    storedProperties.Add(loadedMaterial.shader.GetPropertyName(i), loadedMaterial.GetTexture(loadedMaterial.shader.GetPropertyName(i)));
                                    break;
                                case ShaderPropertyType.Int:
                                    storedProperties.Add(loadedMaterial.shader.GetPropertyName(i), loadedMaterial.GetInt(loadedMaterial.shader.GetPropertyName(i)));
                                    break;
                            }
                        }
                        
                        loadedMaterial.shader = _newShader;
                        foreach (var property in storedProperties)
                        {
                            // Don't map any properties that haven't been allocated matching sets
                            if (_parameterMatchList[property.Key] == "Don't Map")
                            {
                                continue;
                            }

                            // Map all defined properties
                            if (property.Value is Color)
                            {
                                loadedMaterial.SetColor(_parameterMatchList[property.Key], (Color)property.Value);
                            }
                            else if (property.Value is Vector4)
                            {
                                loadedMaterial.SetVector(_parameterMatchList[property.Key], (Vector4)property.Value);
                            }
                            else if (property.Value is Single)
                            {
                                loadedMaterial.SetFloat(_parameterMatchList[property.Key], (float)property.Value);
                            }
                            else if (property.Value is Texture2D)
                            {
                                loadedMaterial.SetTexture(_parameterMatchList[property.Key], (Texture2D)property.Value);
                            }
                            else if (property.Value is int)
                            {
                                loadedMaterial.SetInt(_parameterMatchList[property.Key], (int)property.Value);
                            }
                        }
                    }
                }
            }
        }
        
        public void CreateGUI()
        {
            FetchShaderOptions();
            
            VisualElement root = rootVisualElement;

            // Replace shader button click
            Action onButtonClick = DoReplaceShader;

            var replaceButton = new Button() { text = "Replace Shader on Selected Materials", style = { height = 50}};
            root.Add(replaceButton);
            replaceButton.RegisterCallback<MouseUpEvent>((evt) => onButtonClick());
            
            root.Add(new Label("Replace Shaders:"));

            Action onShaderChange = () =>
            {
                root.Query<ObjectField>().ForEach((shaderSelector) =>
                {
                    if (shaderSelector.name == "oldShaderSelector")
                    {
                        _oldShader = (Shader)shaderSelector.value;
                    }
                    else if (shaderSelector.name == "newShaderSelector")
                    {
                        _newShader = (Shader)shaderSelector.value;
                    }
                });
                if (_oldShader != null && _newShader != null)
                {
                    FetchShaderOptions();
                    UpdateDropdowns(root);
                }
                else
                {
                    RemoveDropdowns(root);
                }
            };

            var oldShader = new ObjectField();
            oldShader.objectType = typeof(Shader);
            oldShader.label = "Old Shader";
            oldShader.name = "oldShaderSelector";
            root.Add(oldShader);
            oldShader.RegisterValueChangedCallback((evt) => onShaderChange());

            var newShader = new ObjectField();
            newShader.objectType = typeof(Shader);
            newShader.label = "New Shader";
            newShader.name = "newShaderSelector";
            root.Add(newShader);
            newShader.RegisterValueChangedCallback((evt) => onShaderChange());
            
            root.Add(new Label("Old => New Property Map:"));
            
            if (_oldShader != null)
            {
                oldShader.value = _oldShader;
            }
            
            if (_newShader != null)
            {
                newShader.value = _newShader;
            }

            if (_oldShader != null && _newShader != null)
            {
                UpdateDropdowns(root);
            }
        }

        private void FetchShaderOptions()
        {
            if (_oldShader != null)
            {
                _oldShaderProperties.Clear();
                for (int i = 0; i < _oldShader.GetPropertyCount(); i++)
                {
                    // Add to the list if it's not a built-in shader property
                    if (!_oldShader.GetPropertyName(i).Contains("_BUILTIN_", StringComparison.OrdinalIgnoreCase))
                    {
                        _oldShaderProperties.Add(_oldShader.GetPropertyName(i));
                    }
                }
            }
            
            if (_newShader != null)
            {
                _newShaderProperties.Clear();
                _newShaderProperties.Add("Don't Map");
                for (int i = 0; i < _newShader.GetPropertyCount(); i++)
                {
                    // Add to the list if it's not a built-in shader property
                    if (!_newShader.GetPropertyName(i).Contains("_BUILTIN_", StringComparison.OrdinalIgnoreCase))
                    {
                        _newShaderProperties.Add(_newShader.GetPropertyName(i));
                    }
                }
            }
        }

        private void RemoveDropdowns(VisualElement root)
        {
            root.Query<DropdownField>().ForEach((dropdown) =>
            {
                root.Remove(dropdown);
            });
        }
        
        private void UpdateDropdowns(VisualElement root)
        {
            Action onPropertyChange = () =>
            {
                _parameterMatchList.Clear();
                root.Query<DropdownField>().ForEach((dropdown) =>
                {
                    _parameterMatchList.Add(dropdown.name, dropdown.value);
                });
            };
            
            // Unregister the change checks before destroying the dropdowns
            root.Query<DropdownField>().ForEach((dropdown) =>
            {
                dropdown.UnregisterValueChangedCallback((evt) => onPropertyChange());
            });
            // Destory all dropdowns
            RemoveDropdowns(root);
            // Recreate the dropdowns
            DropdownField dropdown;
            foreach (string property in _oldShaderProperties)
            {
                var index = Math.Max(_newShaderProperties.FindIndex(x => x.Equals(property)), 0);
                dropdown = new DropdownField(property, _newShaderProperties, index);
                dropdown.name = property;
                root.Add(dropdown);
                dropdown.RegisterValueChangedCallback((evt) => onPropertyChange());
            }
        }
    }
}
