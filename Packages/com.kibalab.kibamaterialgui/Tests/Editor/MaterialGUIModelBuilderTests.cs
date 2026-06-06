#nullable enable

using System.Linq;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Parsing;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Tests.Editor
{
    public sealed class MaterialGUIModelBuilderTests
    {
        [Test]
        public void GroupAttribute_UsesCommaSeparatedPath()
        {
            var shader = RequireShader();

            Assert.That(
                ShaderPropertyAttributeCache.TryGetGroupPath(shader, "_NestedVector", out var path),
                Is.True);
            Assert.That(path, Is.EqualTo("Root/Child"));
        }

        [Test]
        public void GroupAttribute_DoesNotTreatSlashAsPathSeparator()
        {
            Assert.That(ShaderPropertyAttributeCache.ParseGroupPath("Root,Child"), Is.EqualTo("Root/Child"));
            Assert.That(ShaderPropertyAttributeCache.ParseGroupPath("Root/Child"), Is.Empty);
            Assert.That(ShaderPropertyAttributeCache.ParseGroupPath("\"Root/Child\""), Is.Empty);
        }

        [Test]
        public void ShowIf_HidesDependentPropertyBeforeSearchAndFilters()
        {
            using var scope = new MaterialScope(RequireShader());
            var ctx = CreateContext(scope.Material);
            ctx.State.SetSearch("Shadow");

            var model = MaterialGUIModelBuilder.Build(ctx);
            var shadow = model.Properties.First(x => x.PropertyName == "_ShadowColor");

            Assert.That(shadow.ConditionVisible, Is.False);
            Assert.That(shadow.Visible, Is.False);
        }

        [Test]
        public void ShowIf_ShowsDependentPropertyWhenControllerMatches()
        {
            using var scope = new MaterialScope(RequireShader());
            scope.Material.SetFloat("_LightingToggle", 1f);
            var ctx = CreateContext(scope.Material);

            var model = MaterialGUIModelBuilder.Build(ctx);
            var shadow = model.Properties.First(x => x.PropertyName == "_ShadowColor");

            Assert.That(shadow.ConditionVisible, Is.True);
            Assert.That(shadow.Visible, Is.True);
        }

        [Test]
        public void ShowIf_ShowsDependentPropertyWhenAnySelectedMaterialMatches()
        {
            using var first = new MaterialScope(RequireShader());
            using var second = new MaterialScope(RequireShader());
            first.Material.SetFloat("_LightingToggle", 0f);
            second.Material.SetFloat("_LightingToggle", 1f);
            var ctx = CreateContext(first.Material, second.Material);

            var model = MaterialGUIModelBuilder.Build(ctx);
            var shadow = model.Properties.First(x => x.PropertyName == "_ShadowColor");

            Assert.That(shadow.ConditionVisible, Is.True);
            Assert.That(shadow.Visible, Is.True);
        }

        [Test]
        public void Search_MatchesShaderAttributeText()
        {
            using var scope = new MaterialScope(RequireShader());
            var ctx = CreateContext(scope.Material);
            ctx.State.SetSearch("Vector");

            var model = MaterialGUIModelBuilder.Build(ctx);
            var vector = model.Properties.First(x => x.PropertyName == "_NestedVector");
            var toggle = model.Properties.First(x => x.PropertyName == "_LightingToggle");

            Assert.That(vector.SearchMatched, Is.True);
            Assert.That(vector.Visible, Is.True);
            Assert.That(toggle.SearchMatched, Is.False);
            Assert.That(toggle.Visible, Is.False);
        }

        [Test]
        public void ChangedFilter_ShowsOnlyPropertiesThatDifferFromShaderDefaults()
        {
            using var scope = new MaterialScope(RequireShader());
            scope.Material.SetFloat("_LightingToggle", 1f);
            var ctx = CreateContext(scope.Material);
            ctx.State.ToggleFilter(MaterialGUIFilter.Changed);

            var model = MaterialGUIModelBuilder.Build(ctx);
            var toggle = model.Properties.First(x => x.PropertyName == "_LightingToggle");
            var vector = model.Properties.First(x => x.PropertyName == "_NestedVector");

            Assert.That(toggle.Changed, Is.True);
            Assert.That(toggle.Visible, Is.True);
            Assert.That(vector.Changed, Is.False);
            Assert.That(vector.Visible, Is.False);
        }

        [Test]
        public void ColorFilter_UsesConditionVisibleProperties()
        {
            using var scope = new MaterialScope(RequireShader());
            scope.Material.SetFloat("_LightingToggle", 1f);
            var ctx = CreateContext(scope.Material);
            ctx.State.ToggleFilter(MaterialGUIFilter.Colors);

            var model = MaterialGUIModelBuilder.Build(ctx);
            var shadow = model.Properties.First(x => x.PropertyName == "_ShadowColor");
            var toggle = model.Properties.First(x => x.PropertyName == "_LightingToggle");

            Assert.That(shadow.ConditionVisible, Is.True);
            Assert.That(shadow.Visible, Is.True);
            Assert.That(toggle.Visible, Is.False);
        }

        [Test]
        public void Diagnostics_ReportInvalidShowIf()
        {
            using var scope = new MaterialScope(RequireShader());
            var ctx = CreateContext(scope.Material);

            var model = MaterialGUIModelBuilder.Build(ctx);

            Assert.That(model.Diagnostics.Any(x => x.Message.Contains("controller property '_MissingController'")), Is.True);
        }

        [Test]
        public void ModelCollections_AreReadOnlyViews()
        {
            using var scope = new MaterialScope(RequireShader());
            var ctx = CreateContext(scope.Material);

            var model = MaterialGUIModelBuilder.Build(ctx);
            var firstProperty = model.Properties.First();

            Assert.That(((ICollection<ShaderPropertyModel>)model.Properties).IsReadOnly, Is.True);
            Assert.That(((ICollection<MaterialGUIDiagnostic>)model.Diagnostics).IsReadOnly, Is.True);
            Assert.That(((ICollection<ShaderPropertyAttributeCache.ShaderAttributeInfo>)firstProperty.Attributes).IsReadOnly, Is.True);
            Assert.That(((ICollection<ShaderPropertyModel>)model.Root.Properties).IsReadOnly, Is.True);

            Assert.Throws<System.NotSupportedException>(() => ((ICollection<ShaderPropertyModel>)model.Properties).Add(firstProperty));
            Assert.Throws<System.NotSupportedException>(() => ((ICollection<MaterialGUIDiagnostic>)model.Diagnostics).Add(default));
            Assert.Throws<System.NotSupportedException>(() => ((ICollection<ShaderPropertyAttributeCache.ShaderAttributeInfo>)firstProperty.Attributes).Add(default));
        }

        private static Shader RequireShader()
        {
            var shader = Shader.Find("KIBA_/MaterialGUITests/Conditional");
            Assert.That(shader, Is.Not.Null, "Test shader was not imported.");
            return shader!;
        }

        private static EditorContext CreateContext(params Material[] materials)
        {
            Assert.That(materials, Is.Not.Empty);
            var targets = materials.Cast<Object>().ToArray();
            var props = MaterialEditor.GetMaterialProperties(targets);
            return new EditorContext
            {
                Material = materials[0],
                Targets = materials,
                Properties = props,
                DisplayParser = new MaterialPropertyDisplayParser(),
                RendererRegistry = new MaterialPropertyRendererRegistry(),
                State = new MaterialGUIState(),
                PreferencesKeyPrefix = "KIBA_.KIBAMaterialGUI.Tests."
            };
        }

        private readonly struct MaterialScope : System.IDisposable
        {
            public readonly Material Material;

            public MaterialScope(Shader shader)
            {
                Material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }

            public void Dispose()
            {
                Object.DestroyImmediate(Material);
            }
        }
    }
}


