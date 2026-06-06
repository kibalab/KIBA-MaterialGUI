#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KIBA_.KIBAMaterialGUI.Tests.Editor
{
    public sealed class PropertyRendererApiTests
    {
        [Test]
        public void RendererResolution_UsesLowestOrderMatch()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_LightingToggle");
            var args = CreateArgs(scope.Material, property);

            MaterialGUIPropertyRendererRegistry.ResetForTests();

            var found = MaterialGUIPropertyRendererRegistry.TryFindRenderer(args, out var renderer);

            Assert.That(found, Is.True);
            Assert.That(renderer, Is.TypeOf<PriorityRendererA>());
        }

        [Test]
        public void RendererResolution_RejectsMissingShaderAttribute()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_LightingToggle");
            var args = CreateArgs(scope.Material, property);

            Assert.That(args.HasShaderAttribute("Vector"), Is.False);
            Assert.That(new RequiresVectorAttributeRenderer().CanRender(args), Is.True);

            MaterialGUIPropertyRendererRegistry.ResetForTests();
            var found = MaterialGUIPropertyRendererRegistry.TryFindRenderer(args, out var renderer);

            Assert.That(found, Is.True);
            Assert.That(renderer, Is.Not.TypeOf<RequiresVectorAttributeRenderer>());
        }

        [Test]
        public void PropertyRendererArgs_SetFloatValue_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_LightingToggle");
            var args = CreateArgs(scope.Material, property);

            GUI.changed = false;

            args.SetFloatValue(1f, "Test Change");

            Assert.That(property.floatValue, Is.EqualTo(1f));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void PropertyRendererArgs_SetColorValue_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_ShadowColor");
            var args = CreateArgs(scope.Material, property);
            var next = new Color(0.25f, 0.5f, 0.75f, 1f);

            GUI.changed = false;

            args.SetColorValue(next, "Test Change");

            Assert.That(property.colorValue, Is.EqualTo(next));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void PropertyRendererArgs_SetVectorValue_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_NestedVector");
            var args = CreateArgs(scope.Material, property);
            var next = new Vector4(2f, 3f, 4f, 5f);

            GUI.changed = false;

            args.SetVectorValue(next, "Test Change");

            Assert.That(property.vectorValue, Is.EqualTo(next));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void PropertyRendererArgs_SetTextureValue_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_MainTex");
            var args = CreateArgs(scope.Material, property);
            using var texture = new TextureScope();

            GUI.changed = false;

            args.SetTextureValue(texture.Texture, "Test Change");

            Assert.That(property.textureValue, Is.EqualTo(texture.Texture));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void PropertyRendererArgs_SetTextureScaleAndOffset_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_MainTex");
            var args = CreateArgs(scope.Material, property);
            var next = new Vector4(2f, 3f, 0.25f, 0.5f);

            GUI.changed = false;

            args.SetTextureScaleAndOffset(next, "Test Change");

            Assert.That(property.textureScaleAndOffset, Is.EqualTo(next));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void MaterialGUIPropertyChangeUtility_SetFloat_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_LightingToggle");

            GUI.changed = false;

            MaterialGUIPropertyChangeUtility.SetFloat(null, property, 1f, "Test Change");

            Assert.That(property.floatValue, Is.EqualTo(1f));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void MaterialGUIPropertyChangeUtility_SetColor_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_ShadowColor");
            var next = new Color(0.1f, 0.2f, 0.3f, 1f);

            GUI.changed = false;

            MaterialGUIPropertyChangeUtility.SetColor(null, property, next, "Test Change");

            Assert.That(property.colorValue, Is.EqualTo(next));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void MaterialGUIPropertyChangeUtility_SetVector_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_NestedVector");
            var next = new Vector4(3f, 2f, 1f, 0f);

            GUI.changed = false;

            MaterialGUIPropertyChangeUtility.SetVector(null, property, next, "Test Change");

            Assert.That(property.vectorValue, Is.EqualTo(next));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void MaterialGUIPropertyChangeUtility_SetTexture_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_MainTex");
            using var texture = new TextureScope();

            GUI.changed = false;

            MaterialGUIPropertyChangeUtility.SetTexture(null, property, texture.Texture, "Test Change");

            Assert.That(property.textureValue, Is.EqualTo(texture.Texture));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void MaterialGUIPropertyChangeUtility_SetTextureScaleAndOffset_RegistersGuiChanged()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_MainTex");
            var next = new Vector4(0.5f, 0.25f, 2f, 3f);

            GUI.changed = false;

            MaterialGUIPropertyChangeUtility.SetTextureScaleAndOffset(null, property, next, "Test Change");

            Assert.That(property.textureScaleAndOffset, Is.EqualTo(next));
            Assert.That(GUI.changed, Is.True);
        }

        [Test]
        public void MaterialGUIPropertyChangeUtility_DoesNotRegisterUnchangedValues()
        {
            using var scope = new MaterialScope(RequireShader());
            var property = FindProperty(scope.Material, "_LightingToggle");

            GUI.changed = false;

            MaterialGUIPropertyChangeUtility.SetFloat(null, property, property.floatValue, "Test Change");

            Assert.That(GUI.changed, Is.False);
        }

        [Test]
        public void MaterialGUIContext_ExposesReadOnlyTargetAndPropertyLists()
        {
            using var scope = new MaterialScope(RequireShader());
            var properties = MaterialEditor.GetMaterialProperties(new Object[] { scope.Material });
            var targets = new[] { scope.Material };

            var context = new EditorContext
            {
                Material = scope.Material,
                Targets = targets,
                Properties = properties,
            };

            Assert.That(context.Targets, Is.Not.InstanceOf<Material[]>());
            Assert.That(context.Properties, Is.Not.InstanceOf<MaterialProperty[]>());
            Assert.That(context.Targets[0], Is.EqualTo(scope.Material));
            Assert.That(context.Properties.Count, Is.EqualTo(properties.Length));
        }

        [Test]
        public void InternalDiagnostics_AreSilentUntilVerboseLoggingIsEnabled()
        {
            var previous = MaterialGUIInternalDiagnostics.VerboseEnabled;
            try
            {
                MaterialGUIInternalDiagnostics.ResetForTests();
                MaterialGUIInternalDiagnostics.VerboseEnabled = false;

                MaterialGUIInternalDiagnostics.WarnOnce("test.silent", "This should not be logged.");
                LogAssert.NoUnexpectedReceived();

                MaterialGUIInternalDiagnostics.VerboseEnabled = true;
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Visible diagnostic"));

                MaterialGUIInternalDiagnostics.WarnOnce("test.visible", "Visible diagnostic");
                MaterialGUIInternalDiagnostics.WarnOnce("test.visible", "Visible diagnostic");

                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                MaterialGUIInternalDiagnostics.VerboseEnabled = previous;
                MaterialGUIInternalDiagnostics.ResetForTests();
            }
        }

        [Test]
        public void MaterialGUIContributionAttribute_DefaultFiltersAreEmpty()
        {
            var attr = new MaterialGUIContributionAttribute(ContributionTarget.Toolbar);

            Assert.That(attr.ShaderNameEquals, Is.Empty);
            Assert.That(attr.ShaderNameContains, Is.Empty);
            Assert.That(attr.ShaderNameRegex, Is.Empty);
            Assert.That(attr.GroupPath, Is.Empty);
        }

        [Test]
        public void ShaderEditorInjectionAttribute_DefaultFiltersAreEmpty()
        {
            var attr = new ShaderEditorInjectionAttribute(HookPoint.AfterToolbar);

            Assert.That(attr.Hook, Is.EqualTo(HookPoint.AfterToolbar));
            Assert.That(attr.ShaderNameEquals, Is.Empty);
            Assert.That(attr.ShaderNameContains, Is.Empty);
            Assert.That(attr.ShaderNameRegex, Is.Empty);
            Assert.That(attr.RequireProperties, Is.Empty);
            Assert.That(attr.RequireKeywords, Is.Empty);
            Assert.That(attr.GroupPath, Is.Null);
            Assert.That(attr.PropertyName, Is.Null);
        }

        [Test]
        public void ShaderAttributeMetadata_ExposesPascalCaseProperties()
        {
            var attribute = new ShaderPropertyAttributeCache.ShaderAttributeInfo("Demo", "A, B", "[Demo(A, B)]");
            var enumInfo = new ShaderPropertyAttributeCache.EnumInfo
            {
                names = new[] { "Off", "On" },
                values = new[] { 0f, 1f },
            };
            var toggleInfo = new ShaderPropertyAttributeCache.ToggleInfo
            {
                found = true,
                invert = true,
                keyword = "_EXAMPLE",
            };

            Assert.That(attribute.Name, Is.EqualTo(attribute.name));
            Assert.That(attribute.Args, Is.EqualTo(attribute.args));
            Assert.That(attribute.Raw, Is.EqualTo(attribute.raw));
            Assert.That(enumInfo.Names, Is.EqualTo(enumInfo.names));
            Assert.That(enumInfo.Values, Is.EqualTo(enumInfo.values));
            Assert.That(toggleInfo.Found, Is.EqualTo(toggleInfo.found));
            Assert.That(toggleInfo.Invert, Is.EqualTo(toggleInfo.invert));
            Assert.That(toggleInfo.Keyword, Is.EqualTo(toggleInfo.keyword));
        }

        [Test]
        public void ContributionModels_HaveSafeDefaultValues()
        {
            var toolbarItem = new ToolbarItem();
            var dropdownOption = new DropdownOption();
            var contextMenuItem = new ContextMenuItem();
            var groupActionItem = new GroupActionItem();

            Assert.That(toolbarItem.Id, Is.Empty);
            Assert.That(toolbarItem.Label, Is.Empty);
            Assert.That(toolbarItem.Tooltip, Is.Empty);
            Assert.That(toolbarItem.Placeholder, Is.Empty);
            Assert.That(toolbarItem.Options, Is.Empty);
            Assert.That(dropdownOption.Id, Is.Empty);
            Assert.That(dropdownOption.Label, Is.Empty);
            Assert.That(dropdownOption.Tooltip, Is.Empty);
            Assert.That(contextMenuItem.Id, Is.Empty);
            Assert.That(contextMenuItem.Label, Is.Empty);
            Assert.That(groupActionItem.Id, Is.Empty);
            Assert.That(groupActionItem.Tooltip, Is.Empty);
        }

        [Test]
        public void ContributionModels_RejectNullItems()
        {
            Assert.Throws<System.ArgumentNullException>(() => new ToolbarModel().Add(null!));
            Assert.Throws<System.ArgumentNullException>(() => new ContextMenuModel().Add(null!));
            Assert.Throws<System.ArgumentNullException>(() => new GroupActionModel().Add(null!));
        }

        [Test]
        public void ValidateResolver_AcceptsBoolCallbacks()
        {
            var resolved = MaterialGUIPropertyValidationRegistry.TryResolveValidator(
                typeof(PropertyRendererApiTests).FullName + "." + nameof(RejectValidator),
                out var callback,
                out _,
                out var error);

            Assert.That(resolved, Is.True, error);
            Assert.That(callback(default), Is.False);
        }

        [Test]
        public void ValidateResolver_AcceptsVoidCallbacks()
        {
            s_VoidValidatorCalled = false;

            var resolved = MaterialGUIPropertyValidationRegistry.TryResolveValidator(
                typeof(PropertyRendererApiTests).FullName + "." + nameof(VoidValidator),
                out var callback,
                out _,
                out var error);

            Assert.That(resolved, Is.True, error);
            Assert.That(callback(default), Is.True);
            Assert.That(s_VoidValidatorCalled, Is.True);
        }

        [Test]
        public void RendererRegistration_InvalidRegexEmitsDiagnosticAndSkipsEntry()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Invalid ShaderNameRegex on property renderer"));

            var created = MaterialGUIPropertyRendererRegistry.TryCreateEntryForTests(
                new MaterialGUIPropertyRendererAttribute { ShaderNameRegex = "(" },
                new PriorityRendererA());

            Assert.That(created, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void RendererRegistration_ConstructorFailureEmitsDiagnosticAndSkipsRenderer()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Failed to create property renderer"));

            var created = MaterialGUIPropertyRendererRegistry.TryCreateInstanceForTests(typeof(ThrowingRenderer));

            Assert.That(created, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void RendererRegistration_WrongInterfaceDeclarationEmitsDiagnosticAndSkipsType()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("does not implement IMaterialGUIPropertyRenderer"));

            var valid = MaterialGUIPropertyRendererRegistry.ValidateRendererTypeForTests(typeof(NotAPropertyRenderer));

            Assert.That(valid, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ContributionRegistration_InvalidRegexEmitsDiagnosticAndSkipsFilter()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Invalid ShaderNameRegex on Toolbar contribution"));

            var created = ContributionRegistry.TryCreateFilterForTests(
                new MaterialGUIContributionAttribute(ContributionTarget.Toolbar) { ShaderNameRegex = "(" });

            Assert.That(created, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ContributionRegistration_ConstructorFailureEmitsDiagnosticAndSkipsContributor()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Failed to create contribution"));

            var created = ContributionRegistry.TryCreateInstanceForTests<IToolbarContributor>(typeof(ThrowingToolbarContributor));

            Assert.That(created, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ContributionRegistration_WrongInterfaceDeclarationEmitsDiagnosticAndSkipsContributor()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("targets Toolbar but does not implement IToolbarContributor"));

            var valid = ContributionRegistry.ValidateTargetInterfaceForTests(typeof(NotAToolbarContributor), ContributionTarget.Toolbar);

            Assert.That(valid, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ShaderEditorInjectionRegistration_InvalidRegexEmitsDiagnosticAndSkipsEntry()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Invalid ShaderNameRegex on shader editor extension"));

            var created = ExtensionRegistry.TryCreateEntryForTests(
                new ShaderEditorInjectionAttribute(HookPoint.AfterToolbar) { ShaderNameRegex = "(" },
                new DemoShaderEditor());

            Assert.That(created, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ShaderEditorInjectionRegistration_ConstructorFailureEmitsDiagnosticAndSkipsExtension()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Failed to create shader editor extension"));

            var created = ExtensionRegistry.TryCreateInstanceForTests(typeof(ThrowingShaderEditor));

            Assert.That(created, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ShaderEditorInjectionRegistration_WrongInterfaceDeclarationEmitsDiagnosticAndSkipsExtension()
        {
            MaterialGUIRegistryDiagnostics.ResetForTests();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("does not implement IShaderEditor"));

            var valid = ExtensionRegistry.ValidateExtensionTypeForTests(typeof(NotAShaderEditor));

            Assert.That(valid, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        private static Shader RequireShader()
        {
            var shader = Shader.Find("KIBA_/MaterialGUITests/Conditional");
            Assert.That(shader, Is.Not.Null, "Test shader was not imported.");
            return shader!;
        }

        private static MaterialProperty FindProperty(Material material, string propertyName)
        {
            var props = MaterialEditor.GetMaterialProperties(new Object[] { material });
            for (var i = 0; i < props.Length; i++)
            {
                if (props[i].name == propertyName)
                    return props[i];
            }

            Assert.Fail($"Property was not found: {propertyName}");
            return null!;
        }

        private static PropertyRendererArgs CreateArgs(Material material, MaterialProperty property)
        {
            return new PropertyRendererArgs(
                new Rect(0f, 0f, 200f, EditorGUIUtility.singleLineHeight),
                null,
                material,
                property,
                property.displayName,
                EditorStyles.centeredGreyMiniLabel);
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

        private sealed class TextureScope : System.IDisposable
        {
            public readonly Texture2D Texture;

            public TextureScope()
            {
                Texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            }

            public void Dispose()
            {
                Object.DestroyImmediate(Texture);
            }
        }

        [MaterialGUIPropertyRenderer(
            Order = -2000,
            ShaderNameEquals = new[] { "KIBA_/MaterialGUITests/Conditional" },
            PropertyName = "_LightingToggle",
            PropertyTypes = new[] { MaterialProperty.PropType.Float })]
        public sealed class PriorityRendererA : IMaterialGUIPropertyRenderer
        {
            public float GetHeight(PropertyRendererArgs args) => 0f;
            public Rect OnGUI(PropertyRendererArgs args) => args.Position;
        }

        [MaterialGUIPropertyRenderer(
            Order = -1000,
            ShaderNameEquals = new[] { "KIBA_/MaterialGUITests/Conditional" },
            PropertyName = "_LightingToggle",
            PropertyTypes = new[] { MaterialProperty.PropType.Float })]
        public sealed class PriorityRendererB : IMaterialGUIPropertyRenderer
        {
            public float GetHeight(PropertyRendererArgs args) => 0f;
            public Rect OnGUI(PropertyRendererArgs args) => args.Position;
        }

        [MaterialGUIPropertyRenderer(
            Order = -3000,
            ShaderNameEquals = new[] { "KIBA_/MaterialGUITests/Conditional" },
            PropertyName = "_LightingToggle",
            PropertyTypes = new[] { MaterialProperty.PropType.Float },
            RequireShaderAttributes = new[] { "Vector" })]
        public sealed class RequiresVectorAttributeRenderer : IMaterialGUIPropertyRenderer, IMaterialGUIPropertyRendererFilter
        {
            public bool CanRender(PropertyRendererArgs args) => true;
            public float GetHeight(PropertyRendererArgs args) => 0f;
            public Rect OnGUI(PropertyRendererArgs args) => args.Position;
        }

        public sealed class ThrowingRenderer : IMaterialGUIPropertyRenderer
        {
            public ThrowingRenderer()
            {
                throw new System.InvalidOperationException("Renderer construction failed for test.");
            }

            public float GetHeight(PropertyRendererArgs args) => 0f;
            public Rect OnGUI(PropertyRendererArgs args) => args.Position;
        }

        private sealed class NotAPropertyRenderer
        {
        }

        private sealed class ThrowingToolbarContributor : IToolbarContributor
        {
            public ThrowingToolbarContributor()
            {
                throw new System.InvalidOperationException("Contributor construction failed for test.");
            }

            public void Contribute(ToolbarModel model, InjectionArgs args)
            {
            }
        }

        private sealed class NotAToolbarContributor
        {
        }

        private sealed class ThrowingShaderEditor : IShaderEditor
        {
            public ThrowingShaderEditor()
            {
                throw new System.InvalidOperationException("Shader editor construction failed for test.");
            }

            public void OnGUI(InjectionArgs args)
            {
            }
        }

        private sealed class NotAShaderEditor
        {
        }

        private static bool s_VoidValidatorCalled;

        private static bool RejectValidator(MaterialGUIPropertyValidateContext ctx)
        {
            return false;
        }

        private static void VoidValidator(MaterialGUIPropertyValidateContext ctx)
        {
            s_VoidValidatorCalled = true;
        }

        private sealed class DemoShaderEditor : IShaderEditor
        {
            public void OnGUI(InjectionArgs args)
            {
            }
        }
    }
}


