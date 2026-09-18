using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using KIBA_.KIBAMaterialGUI.Editor.IO;
using KIBA_.KIBAMaterialGUI.Editor.Parsing;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using KIBA_.KIBAMaterialGUI.Editor.Localization;
using KIBA_.KIBAMaterialGUI.Editor.UI;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KIBA_.KIBAMaterialGUI.Tests.Editor
{
    public class ImprovementRegressionTests
    {
        private readonly List<Object> _objects = new List<Object>();

        [TearDown]
        public void Cleanup()
        {
            TestDiagnosticProvider.Enabled = false;
            TestDiagnosticProvider.GroupOnly = false;
            GradientTexturePersistenceService.Clear();
            for (var i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }

        [MaterialGUIContribution(ContributionTarget.Diagnostic, ShaderNameRegex = "^Review/MaterialGUI$")]
        public sealed class TestDiagnosticProvider : IMaterialGUIDiagnosticProvider
        {
            internal static bool Enabled;
            internal static bool GroupOnly;
            public void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs args)
            {
                if (!Enabled) return;
                diagnostics.Add(new MaterialGUIDiagnostic(MaterialGUIDiagnosticSeverity.Warning, "Test warning", GroupOnly ? null : "_Integer", "Data/More/Fields"));
                diagnostics.Add(new MaterialGUIDiagnostic(MaterialGUIDiagnosticSeverity.Info, "Test info"));
            }
        }

        [Test]
        public void Session_ReusesTreeWhileRefreshingConditionsAndSearch()
        {
            var mat = NewMaterial();
            var ctx = Context(mat);
            var session = new MaterialGUISession();
            var tree = session.GetTree(ctx);
            mat.SetFloat("_LightingToggle", 1);
            ctx.Properties = MaterialEditor.GetMaterialProperties(new Object[] { mat });
            ctx.State.SetSearch("Shadow");
            Assert.That(session.GetTree(ctx), Is.SameAs(tree));
            var shadow = ctx.Model.Properties.First(p => p.PropertyName == "_ShadowColor");
            Assert.That(shadow.Visible, Is.True);
            Assert.That(tree.Properties.Count, Is.EqualTo(1));
            ctx.State.SetSearch("Root");
            mat.SetFloat("_LightingToggle", 0);
            ctx.Properties = MaterialEditor.GetMaterialProperties(new Object[] { mat });
            session.GetTree(ctx);
            Assert.That(shadow.Visible, Is.False);
        }

        [Test]
        public void Session_RebuildsAfterShaderInvalidation()
        {
            var ctx = Context(NewMaterial());
            var session = new MaterialGUISession();
            var first = session.GetTree(ctx);
            ShaderPropertyAttributeCache.InvalidateAll();
            Assert.That(session.GetTree(ctx), Is.Not.SameAs(first));
        }

        [Test]
        public void Session_RebindsSelectionWithoutRebuildingSchema()
        {
            var first = NewMaterial();
            var second = NewMaterial();
            second.SetFloat("_LightingToggle", 1);
            var session = new MaterialGUISession();
            var firstTree = session.GetTree(Context(first));
            var next = Context(second);
            Assert.That(session.GetTree(next), Is.SameAs(firstTree));
            Assert.That(next.Model.Properties.First(p => p.PropertyName == "_LightingToggle").Property.targets[0], Is.SameAs(second));
            Assert.That(next.Model.Properties.First(p => p.PropertyName == "_ShadowColor").Visible, Is.True);
        }

        [Test]
        public void Search_DoesNotShowEmptyConditionalGroupOrPersistAutoExpansion()
        {
            var ctx = Context(NewMaterial("Review/MaterialGUI"));
            ctx.State.SetSearch("Conditional");
            Assert.That(MaterialGUIModelBuilder.Build(ctx).Root.Children["Conditional"].Visible, Is.False);
            ctx.State.SetSearch("Integer");
            FoldState.SaveFold(ctx.PreferencesKeyPrefix, "Data", false);
            try
            {
                var session = new MaterialGUISession();
                session.GetTree(ctx);
                Assert.That(ctx.Model.Root.Children["Data"].Expanded, Is.True);
                ctx.State.SetSearch("");
                session.GetTree(ctx);
                Assert.That(ctx.Model.Root.Children["Data"].Expanded, Is.False);
            }
            finally { FoldState.SaveFold(ctx.PreferencesKeyPrefix, "Data", true); }
        }

        [Test]
        public void ProviderWarnings_AffectFilterAndGroupCountsWithoutAccumulating()
        {
            var ctx = Context(NewMaterial("Review/MaterialGUI"));
            TestDiagnosticProvider.Enabled = true;
            ctx.State.ToggleFilter(MaterialGUIFilter.Warnings);
            var session = new MaterialGUISession();
            session.GetTree(ctx);
            session.GetTree(ctx);
            Assert.That(ctx.Model.Properties.First(p => p.PropertyName == "_Integer").Visible, Is.True);
            Assert.That(ctx.Model.Root.Children["Data"].WarningCount, Is.EqualTo(1));
            Assert.That(ctx.Model.Diagnostics.Count(d => d.Message == "Test warning"), Is.EqualTo(1));
            Assert.That(ctx.Model.Diagnostics.Any(d => d.Message == "Test info"), Is.True);
        }

        [Test]
        public void GroupProviderWarning_ShowsEligibleDescendants()
        {
            TestDiagnosticProvider.Enabled = TestDiagnosticProvider.GroupOnly = true;
            var ctx = Context(NewMaterial("Review/MaterialGUI"));
            ctx.State.Filters = MaterialGUIFilter.Warnings;
            var model = MaterialGUIModelBuilder.Build(ctx);
            Assert.That(model.Properties.First(p => p.PropertyName == "_Integer").Visible, Is.True);
            Assert.That(model.Root.Children["Data"].WarningCount, Is.EqualTo(1));
        }

        private sealed class ExitRenderer : IMaterialGUIPropertyRenderer
        {
            public float GetHeight(PropertyRendererArgs args) => throw new ExitGUIException();
            public Rect OnGUI(PropertyRendererArgs args) => throw new ExitGUIException();
        }

        [Test]
        public void Renderer_DoesNotSwallowUnityExitGUI()
        {
            var mat = NewMaterial();
            var property = Context(mat).Properties[0];
            var args = new PropertyRendererArgs(Rect.zero, null, mat, property, "Test", new GUIStyle());
            var resolved = new MaterialPropertyRendererRegistry.ResolvedPropertyRenderer(new ExitRenderer());
            Assert.Throws<ExitGUIException>(() => resolved.GetHeight(args));
            Assert.Throws<ExitGUIException>(() => resolved.OnGUI(args));
        }

        [Test]
        public void GradientRelease_DestroysOwnedTransientObjects()
        {
            var mat = NewMaterial();
            var prop = Context(mat).Properties.First(p => p.name == "_MainTex");
            var service = new GradientTexturePersistenceService();
            var metadata = service.LoadOrCreateMetadata(mat, prop);
            var texture = service.LoadOrCreateTexture(mat, prop, metadata);
            GradientTexturePersistenceService.Release(mat);
            Assert.That(metadata == null, Is.True);
            Assert.That(texture == null, Is.True);
        }

        [Test]
        public void GradientCache_PromotesResourcesWhenMaterialIsSaved()
        {
            var path = "Assets/MaterialGUIRegression_" + Guid.NewGuid().ToString("N") + ".mat";
            var mat = NewMaterial();
            var prop = Context(mat).Properties.First(p => p.name == "_MainTex");
            var service = new GradientTexturePersistenceService();
            var metadata = service.LoadOrCreateMetadata(mat, prop);
            var texture = service.LoadOrCreateTexture(mat, prop, metadata);
            metadata.Width = 64;
            mat.SetTexture(prop.name, texture);
            mat.hideFlags = HideFlags.None;
            try
            {
                AssetDatabase.CreateAsset(mat, path);
                Assert.That(service.LoadOrCreateMetadata(mat, prop), Is.SameAs(metadata));
                Assert.That(EditorUtility.IsPersistent(metadata), Is.True);
                Assert.That(EditorUtility.IsPersistent(texture), Is.True);
                Assert.That(metadata.Width, Is.EqualTo(64));
                Assert.That(AssetDatabase.GetAssetPath(texture), Is.EqualTo(path));
                GradientTexturePersistenceService.Clear();
                Assert.That(texture != null, Is.True, "Clearing the cache must not destroy persistent subassets.");
            }
            finally
            {
                GradientTexturePersistenceService.Clear();
                AssetDatabase.DeleteAsset(path);
            }
        }

        [Test]
        public void Preset_RoundTripsTheCorrectTextureSubasset()
        {
            var path = "Assets/MaterialGUIRegression_" + Guid.NewGuid().ToString("N") + ".mat";
            var mat = NewMaterial();
            mat.hideFlags = HideFlags.None;
            try
            {
                AssetDatabase.CreateAsset(mat, path);
                var first = new Texture2D(1, 1) { name = "First" };
                var second = new Texture2D(1, 1) { name = "Second" };
                AssetDatabase.AddObjectToAsset(first, mat);
                AssetDatabase.AddObjectToAsset(second, mat);
                mat.SetTexture("_MainTex", second);
                var ctx = Context(mat);
                var property = ctx.Properties.First(p => p.name == "_MainTex");
                var controller = new PresetController();
                var entry = controller.CapturePreset(ctx, "Subasset", new List<MaterialProperty> { property });
                var json = JsonUtility.ToJson(entry);
                Assert.That(entry.Values[0].TextureLocalId, Is.Not.EqualTo(0));
                property.textureValue = first;
                var restored = ShaderPresetSerialization.ReadEntry(json);
                Assert.That(restored.Values.Count, Is.EqualTo(1));
                Assert.That(restored.Values[0].TextureLocalId, Is.EqualTo(entry.Values[0].TextureLocalId));
                controller.ApplyPreset(ctx, restored);
                Assert.That(mat.GetTexture("_MainTex"), Is.SameAs(second));
            }
            finally { AssetDatabase.DeleteAsset(path); }
        }

        [TestCase("_ShadowColor")]
        [TestCase("_NestedVector")]
        [TestCase("_MainTex")]
        public void ValueHelpers_UnifyMixedSelections(string propertyName)
        {
            var first = NewMaterial();
            var second = NewMaterial();
            second.SetColor("_ShadowColor", Color.red);
            second.SetVector("_NestedVector", Vector4.zero);
            second.SetTexture("_MainTex", Track(new Texture2D(1, 1)));
            var property = Context(first, second).Properties.First(p => p.name == propertyName);
            Assert.That(property.hasMixedValue, Is.True);
            var args = new PropertyRendererArgs(Rect.zero, null, first, property, propertyName, new GUIStyle());
            switch (property.type)
            {
                case MaterialProperty.PropType.Color:
                    args.SetColorValue(property.colorValue);
                    Assert.That(second.GetColor(propertyName), Is.EqualTo(first.GetColor(propertyName)));
                    break;
                case MaterialProperty.PropType.Vector:
                    args.SetVectorValue(property.vectorValue);
                    Assert.That(second.GetVector(propertyName), Is.EqualTo(first.GetVector(propertyName)));
                    break;
                case MaterialProperty.PropType.Texture:
                    args.SetTextureValue(property.textureValue);
                    Assert.That(second.GetTexture(propertyName), Is.SameAs(first.GetTexture(propertyName)));
                    break;
            }
        }

        [Test]
        public void ValidationRollback_RestoresTextureTransformAndKeywordsPerTarget()
        {
            var first = NewMaterial();
            var second = NewMaterial();
            var texture = Track(new Texture2D(1, 1));
            second.SetTexture("_MainTex", texture);
            second.SetTextureScale("_MainTex", new Vector2(3, 4));
            second.SetTextureOffset("_MainTex", new Vector2(0.2f, 0.4f));
            var property = Context(first, second).Properties.First(p => p.name == "_MainTex");
            Assert.That(first.shader.keywordSpace.FindKeyword("REVIEW_FIRST").isValid, Is.True);
            first.EnableKeyword("REVIEW_FIRST");
            second.EnableKeyword("REVIEW_SECOND");
            var snapshotType = typeof(PropertyRowHost).GetNestedType("PropertyValueSnapshot", BindingFlags.NonPublic);
            var snapshot = Activator.CreateInstance(snapshotType, new object[] { property });
            property.textureValue = null;
            property.textureScaleAndOffset = Vector4.zero;
            first.DisableKeyword("REVIEW_FIRST");
            second.DisableKeyword("REVIEW_SECOND");
            snapshotType.GetMethod("Restore").Invoke(snapshot, new object[] { property });
            Assert.That(second.GetTexture("_MainTex"), Is.SameAs(texture));
            Assert.That(first.GetTextureScale("_MainTex"), Is.EqualTo(Vector2.one));
            Assert.That(second.GetTextureScale("_MainTex"), Is.EqualTo(new Vector2(3, 4)));
            Assert.That(second.GetTextureOffset("_MainTex"), Is.EqualTo(new Vector2(0.2f, 0.4f)));
            Assert.That(first.IsKeywordEnabled("REVIEW_FIRST"), Is.True);
            Assert.That(second.IsKeywordEnabled("REVIEW_SECOND"), Is.True);
        }

        [Test]
        public void DefaultCache_ReusesAndDisposesOwnedMaterials()
        {
            var shader = NewMaterial().shader;
            var first = ShaderDefaultMaterialCache.Get(shader);
            Assert.That(ShaderDefaultMaterialCache.Get(shader), Is.SameAs(first));
            ShaderPropertyAttributeCache.InvalidateAll();
            var next = ShaderDefaultMaterialCache.Get(shader);
            Assert.That(first == null, Is.True);
            Assert.That(next, Is.Not.Null);
        }

        [Test]
        public void Preset_RoundTripsTextureUVAndStableJsonFields()
        {
            var mat = NewMaterial();
            mat.SetTexture("_MainTex", null);
            var ctx = Context(mat);
            var property = ctx.Properties.First(p => p.name == "_MainTex");
            var expected = new Vector4(2, 3, 0.25f, 0.5f);
            property.textureScaleAndOffset = expected;
            var controller = new PresetController();
            var json = JsonUtility.ToJson(controller.CapturePreset(ctx, "UV", new List<MaterialProperty> { property }));
            Assert.That(json, Does.Contain("\"Name\":\"UV\""));
            Assert.That(json, Does.Not.Contain("k__BackingField"));
            property.textureValue = Track(new Texture2D(1, 1));
            property.textureScaleAndOffset = Vector4.zero;
            controller.ApplyPreset(ctx, JsonUtility.FromJson<PresetEntry>(json));
            Assert.That(mat.GetTexture("_MainTex"), Is.Null);
            Assert.That(mat.GetTextureScale("_MainTex"), Is.EqualTo(new Vector2(expected.x, expected.y)));
            Assert.That(mat.GetTextureOffset("_MainTex"), Is.EqualTo(new Vector2(expected.z, expected.w)));
        }

        [Test]
        public void Preset_ReadsLegacyBackingFieldJson()
        {
            const string json = "{\"<Name>k__BackingField\":\"Legacy\",\"<Values>k__BackingField\":[{\"<Property>k__BackingField\":\"_LightingToggle\",\"<Float>k__BackingField\":1}]}";
            var entry = ShaderPresetSerialization.ReadEntry(json);
            Assert.That(entry.Name, Is.EqualTo("Legacy"));
            Assert.That(entry.Values[0].Property, Is.EqualTo("_LightingToggle"));
            Assert.That(entry.Values[0].Float, Is.EqualTo(1));
            var storeJson = "{\"<Groups>k__BackingField\":[{\"<ShaderNameRegex>k__BackingField\":\".*\",\"<Presets>k__BackingField\":[" + json + "]}]}";
            Assert.That(ShaderPresetSerialization.ReadStore(storeJson).Groups[0].Presets[0].Name, Is.EqualTo("Legacy"));
            var newFormat = JsonUtility.ToJson(ShaderPresetSerialization.ReadStore(storeJson));
            Assert.That(ShaderPresetSerialization.ReadStore(newFormat).Groups[0].Presets[0].Name, Is.EqualTo("Legacy"));
        }

        [Test]
        public void Integer_EditFilterResetAndPresetAreSupported()
        {
            var mat = NewMaterial("Review/MaterialGUI");
            var ctx = Context(mat);
            var property = ctx.Properties.First(p => p.name == "_Integer");
            new PropertyRendererArgs(Rect.zero, ctx.MaterialEditor, mat, property, "Integer", new GUIStyle()).SetIntValue(7);
            ctx.State.Filters = MaterialGUIFilter.Numbers | MaterialGUIFilter.Changed;
            var model = MaterialGUIModelBuilder.Build(ctx);
            Assert.That(model.Properties.First(p => p.PropertyName == "_Integer").Visible, Is.True);
            var controller = new PresetController();
            var preset = controller.CapturePreset(ctx, "Int", new List<MaterialProperty> { property });
            PropertyRowHost.ResetPropertiesToShaderDefaults(ctx, new[] { property });
            Assert.That(mat.GetInteger("_Integer"), Is.EqualTo(2));
            ctx.Properties = MaterialEditor.GetMaterialProperties(new Object[] { mat });
            controller.ApplyPreset(ctx, preset);
            Assert.That(mat.GetInteger("_Integer"), Is.EqualTo(7));
        }

        [Test]
        public void ResetGroup_CanUndoAndRedoAllValuesTogether()
        {
            var mat = NewMaterial();
            mat.SetFloat("_LightingToggle", 1);
            mat.SetVector("_NestedVector", new Vector4(5, 6, 7, 8));
            var ctx = Context(mat);
            Undo.IncrementCurrentGroup();
            PropertyRowHost.ResetPropertiesToShaderDefaults(ctx, ctx.Properties);
            Undo.FlushUndoRecordObjects();
            Assert.That(mat.GetFloat("_LightingToggle"), Is.EqualTo(0));
            Assert.That(mat.GetVector("_NestedVector"), Is.EqualTo(new Vector4(1, 1, 0, 0)));
            Undo.PerformUndo();
            Assert.That(mat.GetFloat("_LightingToggle"), Is.EqualTo(1));
            Assert.That(mat.GetVector("_NestedVector"), Is.EqualTo(new Vector4(5, 6, 7, 8)));
            Undo.PerformRedo();
            Assert.That(mat.GetFloat("_LightingToggle"), Is.EqualTo(0));
            Undo.ClearUndo(mat);
        }

        [Test]
        public void Localization_FallsBackToDisplayLabel()
        {
            var ctx = Context(NewMaterial());
            ctx.LocalizationStore = new ShaderLocalizationStore();
            var model = MaterialGUIModelBuilder.Build(ctx);
            Assert.That(model.Properties.First(p => p.PropertyName == "_LightingToggle").TranslatedLabel, Is.EqualTo("Lighting Toggle"));
        }

        private T Track<T>(T obj) where T : Object
        {
            _objects.Add(obj);
            return obj;
        }

        private Material NewMaterial(string shader = "KIBA_/MaterialGUITests/Conditional")
        {
            return Track(new Material(Shader.Find(shader)) { hideFlags = HideFlags.HideAndDontSave });
        }

        private EditorContext Context(params Material[] mats)
        {
            return new EditorContext
            {
                Material = mats[0],
                MaterialEditor = Track((MaterialEditor)UnityEditor.Editor.CreateEditor(mats[0], typeof(MaterialEditor))),
                Targets = mats,
                Properties = MaterialEditor.GetMaterialProperties(mats.Cast<Object>().ToArray()),
                DisplayParser = new MaterialPropertyDisplayParser(),
                RendererRegistry = new MaterialPropertyRendererRegistry(),
                PresetStore = new ShaderPresetStore(),
                PreferencesKeyPrefix = "MaterialGUI.Review."
            };
        }

        [Test]
        public void CachedTree_ShouldBindCurrentMaterialProperties()
        {
            var mat = NewMaterial();
            var ctx = Context(mat);
            var gui = new KIBA_.KIBAMaterialGUI.Editor.MaterialGUI();
            var type = gui.GetType();
            type.GetField("_material", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(gui, mat);
            var build = type.GetMethod("GetOrBuildTree", BindingFlags.NonPublic | BindingFlags.Instance);
            build.Invoke(gui, new object[] { ctx });
            mat.SetFloat("_LightingToggle", 1f);
            ctx.Properties = MaterialEditor.GetMaterialProperties(new Object[] { mat });
            Assert.That(ctx.Properties.First(p => p.name == "_LightingToggle").floatValue, Is.EqualTo(1f));
            build.Invoke(gui, new object[] { ctx });
            Assert.That(ctx.Model.Properties.First(p => p.PropertyName == "_LightingToggle").Property.floatValue,
                Is.EqualTo(1f), "Cached tree must follow refreshed property wrappers.");
        }

        [Test]
        public void SetFloatValue_ShouldUnifyMixedTargetsAtRepresentativeValue()
        {
            var first = NewMaterial();
            var second = NewMaterial();
            second.SetFloat("_LightingToggle", 1f);
            var property = Context(first, second).Properties.First(p => p.name == "_LightingToggle");
            Assert.That(property.hasMixedValue, Is.True);
            var expected = property.floatValue;
            var args = new PropertyRendererArgs(Rect.zero, null, first, property, "Toggle", new GUIStyle());
            args.SetFloatValue(expected);
            Assert.That(new[] { first.GetFloat(property.name), second.GetFloat(property.name) },
                Is.All.EqualTo(expected));
        }

        [Test]
        public void GradientMetadata_ShouldBeReusedForTransientMaterial()
        {
            var mat = NewMaterial();
            var property = Context(mat).Properties.First(p => p.name == "_MainTex");
            var persistence = new GradientTexturePersistenceService();
            var metaA = Track(persistence.LoadOrCreateMetadata(mat, property));
            var metaB = Track(persistence.LoadOrCreateMetadata(mat, property));
            Assert.That(metaB, Is.SameAs(metaA));
        }

        [Test]
        public void GradientTexture_ShouldBeReusedForTransientMaterial()
        {
            var mat = NewMaterial();
            var property = Context(mat).Properties.First(p => p.name == "_MainTex");
            var persistence = new GradientTexturePersistenceService();
            var meta = Track(persistence.LoadOrCreateMetadata(mat, property));
            var texA = Track(persistence.LoadOrCreateTexture(mat, property, meta));
            var texB = Track(persistence.LoadOrCreateTexture(mat, property, meta));
            Assert.That(texB, Is.SameAs(texA));
        }

        [Test]
        public void PresetMatching_ShouldPopulateNewContextForSameShader()
        {
            var mat = NewMaterial();
            var first = Context(mat);
            first.PresetStore.Groups.Add(new PresetGroup
            {
                ShaderNameRegex = ".*",
                Presets = new List<PresetEntry> { new PresetEntry { Name = "Demo" } }
            });
            var controller = new PresetController();
            controller.BuildMatchedPresets(first);
            Assert.That(first.MatchedPresets.Count, Is.EqualTo(1));
            var next = Context(mat);
            next.PresetStore = first.PresetStore;
            controller.BuildMatchedPresets(next);
            Assert.That(next.MatchedPresets.Count, Is.EqualTo(1));
        }

        [Test]
        public void Preset_ShouldRestoreEmptyTexture()
        {
            var mat = NewMaterial();
            mat.SetTexture("_MainTex", null);
            var ctx = Context(mat);
            var property = ctx.Properties.First(p => p.name == "_MainTex");
            var controller = new PresetController();
            var preset = controller.CapturePreset(ctx, "Empty", new List<MaterialProperty> { property });
            property.textureValue = Track(new Texture2D(1, 1));
            controller.ApplyPreset(ctx, preset);
            Assert.That(mat.GetTexture("_MainTex"), Is.Null);
        }

        [Test]
        public void HideInInspector_ShouldNotProduceVisibleRow()
        {
            var mat = NewMaterial("Review/MaterialGUI");
            var ctx = Context(mat);
            var property = ctx.Properties.First(p => p.name == "_Hidden");
            Assert.That((property.flags & MaterialProperty.PropFlags.HideInInspector) != 0, Is.True);
            var model = MaterialGUIModelBuilder.Build(ctx);
            Assert.That(model.Properties.All(p => p.PropertyName != "_Hidden" || !p.Visible), Is.True);
        }

        [Test]
        public void Search_ShouldExposeCollapsedParentGroup()
        {
            var mat = NewMaterial();
            var ctx = Context(mat);
            FoldState.SaveFold(ctx.PreferencesKeyPrefix, "Root", false);
            try
            {
                ctx.State.SetSearch("_NestedVector");
                var model = MaterialGUIModelBuilder.Build(ctx);
                Assert.That(model.Root.Children["Root"].Expanded, Is.True);
            }
            finally
            {
                FoldState.SaveFold(ctx.PreferencesKeyPrefix, "Root", true);
                EditorPrefs.DeleteKey(ctx.PreferencesKeyPrefix + "Fold.Root");
            }
        }

        [Test]
        public void ValidationRollback_ShouldPreserveMixedOriginalValues()
        {
            var first = NewMaterial();
            var second = NewMaterial();
            second.SetFloat("_LightingToggle", 1f);
            var property = Context(first, second).Properties.First(p => p.name == "_LightingToggle");
            var snapshotType = typeof(PropertyRowHost).GetNestedType("PropertyValueSnapshot", BindingFlags.NonPublic);
            var snapshot = Activator.CreateInstance(snapshotType, new object[] { property });
            property.floatValue = 2f;
            snapshotType.GetMethod("Restore").Invoke(snapshot, new object[] { property });
            Assert.That(first.GetFloat(property.name), Is.EqualTo(0f));
            Assert.That(second.GetFloat(property.name), Is.EqualTo(1f));
        }
    }
}
