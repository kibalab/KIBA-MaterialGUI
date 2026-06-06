#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    internal static class ContributionRegistry
    {
        private static bool _scanned;
        private static readonly List<ToolbarEntry> ToolbarEntries = new();
        private static readonly List<MenuEntry> MenuEntries = new();
        private static readonly List<GroupActionEntry> GroupActionEntries = new();
        private static readonly List<DiagnosticEntry> DiagnosticEntries = new();
        private static readonly List<FilterEntry> FilterEntries = new();

        private sealed class MatchFilter
        {
            public readonly string[] ShaderNameEquals;
            public readonly string[] ShaderNameContains;
            public readonly Regex? ShaderRegex;
            public readonly string? GroupPath;

            public MatchFilter(string[] shaderNameEquals, string[] shaderNameContains, Regex? shaderRegex, string? groupPath)
            {
                ShaderNameEquals = shaderNameEquals;
                ShaderNameContains = shaderNameContains;
                ShaderRegex = shaderRegex;
                GroupPath = groupPath;
            }
        }

        private sealed class ToolbarEntry
        {
            public readonly int Order;
            public readonly IToolbarContributor Impl;
            public readonly MatchFilter Filter;

            public ToolbarEntry(int order, IToolbarContributor impl, MatchFilter filter)
            {
                Order = order;
                Impl = impl;
                Filter = filter;
            }
        }

        private sealed class MenuEntry
        {
            public readonly int Order;
            public readonly IGroupMenuContributor Impl;
            public readonly MatchFilter Filter;

            public MenuEntry(int order, IGroupMenuContributor impl, MatchFilter filter)
            {
                Order = order;
                Impl = impl;
                Filter = filter;
            }
        }

        private sealed class GroupActionEntry
        {
            public readonly int Order;
            public readonly IMaterialGUIGroupActionContributor Impl;
            public readonly MatchFilter Filter;

            public GroupActionEntry(int order, IMaterialGUIGroupActionContributor impl, MatchFilter filter)
            {
                Order = order;
                Impl = impl;
                Filter = filter;
            }
        }

        private sealed class DiagnosticEntry
        {
            public readonly int Order;
            public readonly IMaterialGUIDiagnosticProvider Impl;
            public readonly MatchFilter Filter;

            public DiagnosticEntry(int order, IMaterialGUIDiagnosticProvider impl, MatchFilter filter)
            {
                Order = order;
                Impl = impl;
                Filter = filter;
            }
        }

        private sealed class FilterEntry
        {
            public readonly int Order;
            public readonly IMaterialGUIFilterProvider Impl;
            public readonly MatchFilter Filter;

            public FilterEntry(int order, IMaterialGUIFilterProvider impl, MatchFilter filter)
            {
                Order = order;
                Impl = impl;
                Filter = filter;
            }
        }

        private static void EnsureScan()
        {
            if (_scanned) return;
            _scanned = true;

            var types = TypeCache.GetTypesWithAttribute<MaterialGUIContributionAttribute>();
            for (var i = 0; i < types.Count; i++)
            {
                var t = types[i];
                if (t == null || t.IsAbstract) continue;

                var attrs = (MaterialGUIContributionAttribute[])t.GetCustomAttributes(typeof(MaterialGUIContributionAttribute), false);
                if (attrs == null || attrs.Length == 0) continue;

                IToolbarContributor? toolImpl = null;
                IGroupMenuContributor? menuImpl = null;
                IMaterialGUIGroupActionContributor? groupActionImpl = null;
                IMaterialGUIDiagnosticProvider? diagnosticImpl = null;
                IMaterialGUIFilterProvider? filterImpl = null;

                if (typeof(IToolbarContributor).IsAssignableFrom(t))
                    toolImpl = CreateInstance<IToolbarContributor>(t);
                if (typeof(IGroupMenuContributor).IsAssignableFrom(t))
                    menuImpl = CreateInstance<IGroupMenuContributor>(t);
                if (typeof(IMaterialGUIGroupActionContributor).IsAssignableFrom(t))
                    groupActionImpl = CreateInstance<IMaterialGUIGroupActionContributor>(t);
                if (typeof(IMaterialGUIDiagnosticProvider).IsAssignableFrom(t))
                    diagnosticImpl = CreateInstance<IMaterialGUIDiagnosticProvider>(t);
                if (typeof(IMaterialGUIFilterProvider).IsAssignableFrom(t))
                    filterImpl = CreateInstance<IMaterialGUIFilterProvider>(t);

                for (var j = 0; j < attrs.Length; j++)
                {
                    var attr = attrs[j];
                    if (attr == null) continue;
                    if (!ValidateTargetInterface(t, attr.Target)) continue;
                    if (!TryCreateFilter(attr, out var filter) || filter == null) continue;

                    if (attr.Target == ContributionTarget.Toolbar && toolImpl != null)
                        ToolbarEntries.Add(new ToolbarEntry(attr.Order, toolImpl, filter));

                    if (attr.Target == ContributionTarget.GroupContextMenu && menuImpl != null)
                        MenuEntries.Add(new MenuEntry(attr.Order, menuImpl, filter));
                    if (attr.Target == ContributionTarget.GroupAction && groupActionImpl != null)
                        GroupActionEntries.Add(new GroupActionEntry(attr.Order, groupActionImpl, filter));
                    if (attr.Target == ContributionTarget.Diagnostic && diagnosticImpl != null)
                        DiagnosticEntries.Add(new DiagnosticEntry(attr.Order, diagnosticImpl, filter));
                    if (attr.Target == ContributionTarget.Filter && filterImpl != null)
                        FilterEntries.Add(new FilterEntry(attr.Order, filterImpl, filter));
                }
            }

            ToolbarEntries.Sort(static (a, b) => a.Order.CompareTo(b.Order));
            MenuEntries.Sort(static (a, b) => a.Order.CompareTo(b.Order));
            GroupActionEntries.Sort(static (a, b) => a.Order.CompareTo(b.Order));
            DiagnosticEntries.Sort(static (a, b) => a.Order.CompareTo(b.Order));
            FilterEntries.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        }

        internal static void ApplyToolbar(EditorContext ctx, ToolbarModel model, string? groupPath = null)
        {
            EnsureScan();

            var shaderName = ctx.Material?.shader != null ? ctx.Material.shader.name : string.Empty;
            var args = new InjectionArgs(HookPoint.BeforeToolbar, ctx, groupPath);

            for (var i = 0; i < ToolbarEntries.Count; i++)
            {
                var entry = ToolbarEntries[i];
                if (!Match(entry.Filter, shaderName, groupPath)) continue;
                entry.Impl.Contribute(model, args);
            }
        }

        internal static void ApplyGroupMenu(EditorContext ctx, ContextMenuModel model, string? groupPath)
        {
            EnsureScan();

            var shaderName = ctx.Material?.shader != null ? ctx.Material.shader.name : string.Empty;
            var args = new InjectionArgs(HookPoint.BeforeGroupContent, ctx, groupPath);

            for (var i = 0; i < MenuEntries.Count; i++)
            {
                var entry = MenuEntries[i];
                if (!Match(entry.Filter, shaderName, groupPath)) continue;
                entry.Impl.Contribute(model, args);
            }
        }

        internal static void ApplyGroupActions(EditorContext ctx, GroupActionModel model, string? groupPath)
        {
            EnsureScan();

            var shaderName = ctx.Material?.shader != null ? ctx.Material.shader.name : string.Empty;
            var args = new InjectionArgs(HookPoint.BeforeGroupHeader, ctx, groupPath);

            for (var i = 0; i < GroupActionEntries.Count; i++)
            {
                var entry = GroupActionEntries[i];
                if (!Match(entry.Filter, shaderName, groupPath)) continue;
                entry.Impl.Contribute(model, args);
            }
        }

        internal static void ApplyDiagnostics(EditorContext ctx, List<MaterialGUIDiagnostic> diagnostics, string? groupPath = null)
        {
            EnsureScan();

            var shaderName = ctx.Material?.shader != null ? ctx.Material.shader.name : string.Empty;
            var args = new InjectionArgs(HookPoint.AfterTree, ctx, groupPath);

            for (var i = 0; i < DiagnosticEntries.Count; i++)
            {
                var entry = DiagnosticEntries[i];
                if (!Match(entry.Filter, shaderName, groupPath)) continue;
                entry.Impl.Contribute(diagnostics, args);
            }
        }

        internal static void ApplyFilterToolbar(EditorContext ctx, ToolbarModel model, string? groupPath = null)
        {
            EnsureScan();

            var shaderName = ctx.Material?.shader != null ? ctx.Material.shader.name : string.Empty;
            var args = new InjectionArgs(HookPoint.BeforeToolbar, ctx, groupPath);

            for (var i = 0; i < FilterEntries.Count; i++)
            {
                var entry = FilterEntries[i];
                if (!Match(entry.Filter, shaderName, groupPath)) continue;
                entry.Impl.Contribute(model, args);
            }
        }

        private static bool ValidateTargetInterface(Type type, ContributionTarget target)
        {
            var required = GetRequiredInterface(target);
            if (required == null || required.IsAssignableFrom(type))
                return true;

            MaterialGUIRegistryDiagnostics.WarnOnce(
                $"contribution.interface:{type.FullName}:{target}",
                $"Contribution declaration '{type.FullName}' targets {target} but does not implement {required.Name}.");
            return false;
        }

        private static Type? GetRequiredInterface(ContributionTarget target)
        {
            switch (target)
            {
                case ContributionTarget.Toolbar:
                    return typeof(IToolbarContributor);
                case ContributionTarget.GroupContextMenu:
                    return typeof(IGroupMenuContributor);
                case ContributionTarget.GroupAction:
                    return typeof(IMaterialGUIGroupActionContributor);
                case ContributionTarget.Diagnostic:
                    return typeof(IMaterialGUIDiagnosticProvider);
                case ContributionTarget.Filter:
                    return typeof(IMaterialGUIFilterProvider);
                default:
                    return null;
            }
        }

        private static T? CreateInstance<T>(Type type) where T : class
        {
            try
            {
                return Activator.CreateInstance(type) as T;
            }
            catch (Exception ex)
            {
                MaterialGUIRegistryDiagnostics.WarnOnce(
                    $"contribution.create:{type.FullName}:{ex.GetType().FullName}",
                    $"Failed to create contribution '{type.FullName}'. {ex.Message}");
                return null;
            }
        }

        private static bool TryCreateFilter(MaterialGUIContributionAttribute attr, out MatchFilter? filter)
        {
            var shaderEquals = Normalize(attr.ShaderNameEquals);
            var shaderContains = Normalize(attr.ShaderNameContains);

            Regex? regex = null;
            if (!string.IsNullOrEmpty(attr.ShaderNameRegex))
            {
                try
                {
                    regex = new Regex(
                        attr.ShaderNameRegex,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                catch (Exception ex)
                {
                    MaterialGUIRegistryDiagnostics.WarnOnce(
                        $"contribution.regex:{attr.Target}:{attr.ShaderNameRegex}",
                        $"Invalid ShaderNameRegex on {attr.Target} contribution: {ex.Message}");
                    filter = null;
                    return false;
                }
            }

            filter = new MatchFilter(
                shaderEquals,
                shaderContains,
                regex,
                string.IsNullOrWhiteSpace(attr.GroupPath) ? null : attr.GroupPath);
            return true;
        }

#if UNITY_INCLUDE_TESTS
        internal static bool ValidateTargetInterfaceForTests(Type type, ContributionTarget target)
        {
            return ValidateTargetInterface(type, target);
        }

        internal static bool TryCreateInstanceForTests<T>(Type type) where T : class
        {
            return CreateInstance<T>(type) != null;
        }

        internal static bool TryCreateFilterForTests(MaterialGUIContributionAttribute attr)
        {
            return TryCreateFilter(attr, out _);
        }
#endif

        private static bool Match(MatchFilter filter, string shaderName, string? groupPath)
        {
            if (!MatchEqualsFilter(filter.ShaderNameEquals, shaderName))
                return false;
            if (!MatchContainsFilter(filter.ShaderNameContains, shaderName))
                return false;
            if (filter.ShaderRegex != null && !filter.ShaderRegex.IsMatch(shaderName))
                return false;

            if (string.IsNullOrEmpty(filter.GroupPath))
                return true;

            return string.Equals(filter.GroupPath, groupPath ?? string.Empty, StringComparison.Ordinal);
        }

        private static bool MatchEqualsFilter(string[] filters, string value)
        {
            if (filters.Length == 0) return true;

            for (var i = 0; i < filters.Length; i++)
            {
                if (string.Equals(filters[i], value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool MatchContainsFilter(string[] filters, string value)
        {
            if (filters.Length == 0) return true;

            for (var i = 0; i < filters.Length; i++)
            {
                if (value.IndexOf(filters[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static string[] Normalize(string[]? values)
        {
            if (values == null || values.Length == 0) return Array.Empty<string>();

            var count = 0;
            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    count++;
            }

            if (count == 0) return Array.Empty<string>();

            var normalized = new string[count];
            var index = 0;
            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i];
                if (string.IsNullOrWhiteSpace(v)) continue;
                normalized[index++] = v;
            }

            return normalized;
        }
    }
}


