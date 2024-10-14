using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SetupRenderFeatures
{
    public List<RenderObjects> RenderObjectsToAdd;
    
    [InitializeOnLoadMethod] [MenuItem("Tools/Simple Grid/Add Renderer Featues", priority = 10)]
    static void InitOnLoad()
    {
        EditorApplication.delayCall += Setup;
    }

    static void Setup()
    {
        addRendererFeature<RenderObjects>();
    }

    static void addRendererFeature<T>() where T : ScriptableRendererFeature
    {
        var handledDataObjects = new List<ScriptableRendererData>();

        int levels = QualitySettings.names.Length;
        for (int level = 0; level < levels; level++)
        {
            // Fetch renderer data
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;//QualitySettings.GetRenderPipelineAssetAt(level) as UniversalRenderPipelineAsset;
            // Do NOT use asset.LoadBuiltinRendererData().
            // It's a trap, see: https://github.com/Unity-Technologies/Graphics/blob/b57fcac51bb88e1e589b01e32fd610c991f16de9/Packages/com.unity.render-pipelines.universal/Runtime/Data/UniversalRenderPipelineAsset.cs#L719
            var data = getDefaultRenderer(asset);

            // This is needed in case multiple renderers share the same renderer data object.
            // If they do then we only handle it once.
            if (handledDataObjects.Contains(data))
            {
                continue;
            }
            handledDataObjects.Add(data);

            // Create & add feature if not yet existing
            bool _simpleGridLineMask = false;
            bool _simpleGridCells = false;
            foreach (var feature in data.rendererFeatures)
            {
                if (feature is RenderObjects)
                {
                    if (((RenderObjects)feature).name == "SimpleGridLineMask")
                    {
                        _simpleGridLineMask = true;
                        continue;
                    }
                    if (((RenderObjects)feature).name == "SimpleGridCells")
                    {
                        _simpleGridCells = true;
                        continue;
                    }
                }
            }
            if (!_simpleGridCells)
            {
                // Create the feature
                RenderObjects feature = ScriptableObject.CreateInstance<RenderObjects>();
                feature.name = "SimpleGridCells";
                feature.settings.filterSettings.RenderQueueType = RenderQueueType.Transparent;
                feature.settings.Event = RenderPassEvent.AfterRenderingOpaques;
                StencilStateData _stencilData = new StencilStateData();
                _stencilData.stencilCompareFunction = CompareFunction.NotEqual;
                _stencilData.passOperation = StencilOp.Keep;
                _stencilData.failOperation = StencilOp.Keep;
                _stencilData.zFailOperation = StencilOp.Keep;
                _stencilData.stencilReference = 1;
                _stencilData.overrideStencilState = true;
                feature.settings.stencilSettings = _stencilData;

                // Add it to the renderer data.
                addRenderFeature(data, feature);

                Debug.Log("Added render feature '" + feature.name + "' to " + data.name + ".");
            }
            if (!_simpleGridLineMask)
            {
                // Create the feature
                RenderObjects feature = ScriptableObject.CreateInstance<RenderObjects>();
                feature.name = "SimpleGridLineMask";
                feature.settings.filterSettings.RenderQueueType = RenderQueueType.Transparent;
                feature.settings.Event = RenderPassEvent.BeforeRenderingOpaques;
                StencilStateData _stencilData = new StencilStateData();
                _stencilData.stencilCompareFunction = CompareFunction.Always;
                _stencilData.passOperation = StencilOp.Replace;
                _stencilData.failOperation = StencilOp.Keep;
                _stencilData.zFailOperation = StencilOp.Keep;
                _stencilData.stencilReference = 1;
                _stencilData.overrideStencilState = true;
                feature.settings.stencilSettings = _stencilData;

                // Add it to the renderer data.
                addRenderFeature(data, feature);

                Debug.Log("Added render feature '" + feature.name + "' to " + data.name + ".");
            }
        }
    }

    /// <summary>
    /// Gets the default renderer index.
    /// Thanks to: https://discussions.unity.com/t/842637/2
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    static int getDefaultRendererIndex(UniversalRenderPipelineAsset asset)
    {
        return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);
    }

    /// <summary>
    /// Gets the renderer from the current pipeline asset that's marked as default.
    /// Thanks to: https://discussions.unity.com/t/842637/2
    /// </summary>
    /// <returns></returns>
    static ScriptableRendererData getDefaultRenderer(UniversalRenderPipelineAsset asset)
    {
        if (asset)
        {
            ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                    .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(asset);
            int defaultRendererIndex = getDefaultRendererIndex(asset);

            return rendererDataList[defaultRendererIndex];
        }
        else
        {
            Debug.LogError("No Universal Render Pipeline is currently active.");
            return null;
        }
    }

    /// <summary>
    /// Based on Unity add feature code.
    /// See: AddComponent() in https://github.com/Unity-Technologies/Graphics/blob/d0473769091ff202422ad13b7b764c7b6a7ef0be/com.unity.render-pipelines.universal/Editor/ScriptableRendererDataEditor.cs#180
    /// </summary>
    /// <param name="data"></param>
    /// <param name="feature"></param>
    static void addRenderFeature(ScriptableRendererData data, ScriptableRendererFeature feature)
    {
        // Let's mirror what Unity does.
        var serializedObject = new SerializedObject(data);

        var renderFeaturesProp = serializedObject.FindProperty("m_RendererFeatures"); // Let's hope they don't change these.
        var renderFeaturesMapProp = serializedObject.FindProperty("m_RendererFeatureMap");

        serializedObject.Update();

        // Store this new effect as a sub-asset so we can reference it safely afterwards.
        // Only when we're not dealing with an instantiated asset
        if (EditorUtility.IsPersistent(data))
            AssetDatabase.AddObjectToAsset(feature, data);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out var guid, out long localId);

        // Grow the list first, then add - that's how serialized lists work in Unity
        renderFeaturesProp.arraySize++;
        var componentProp = renderFeaturesProp.GetArrayElementAtIndex(renderFeaturesProp.arraySize - 1);
        componentProp.objectReferenceValue = feature;

        // Update GUID Map
        renderFeaturesMapProp.arraySize++;
        var guidProp = renderFeaturesMapProp.GetArrayElementAtIndex(renderFeaturesMapProp.arraySize - 1);
        guidProp.longValue = localId;

        // Force save / refresh
        if (EditorUtility.IsPersistent(data))
        {
            AssetDatabase.SaveAssetIfDirty(data);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
