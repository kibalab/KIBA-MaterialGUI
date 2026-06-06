#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    [Flags]
    public enum MaterialGUIFilter
    {
        None = 0,
        Changed = 1 << 0,
        Warnings = 1 << 1,
        Textures = 1 << 2,
        Numbers = 1 << 3,
        Colors = 1 << 4,
    }

    public enum MaterialGUIDiagnosticSeverity
    {
        Info,
        Warning,
        Error
    }

    internal sealed class MaterialGUIState
    {
        public string Search = string.Empty;
        public MaterialGUIFilter Filters = MaterialGUIFilter.None;
        public int Version;

        public bool HasFilter(MaterialGUIFilter filter) => (Filters & filter) != 0;

        public void SetSearch(string search)
        {
            search ??= string.Empty;
            if (string.Equals(Search, search, StringComparison.Ordinal)) return;
            Search = search;
            Version++;
        }

        public void ToggleFilter(MaterialGUIFilter filter)
        {
            Filters = HasFilter(filter) ? Filters & ~filter : Filters | filter;
            Version++;
        }
    }

    public readonly struct MaterialGUIDiagnostic
    {
        public readonly MaterialGUIDiagnosticSeverity Severity;
        public readonly string Message;
        public readonly string? PropertyName;
        public readonly string? GroupPath;

        public MaterialGUIDiagnostic(
            MaterialGUIDiagnosticSeverity severity,
            string message,
            string? propertyName = null,
            string? groupPath = null)
        {
            Severity = severity;
            Message = message ?? string.Empty;
            PropertyName = propertyName;
            GroupPath = groupPath;
        }
    }

    public readonly struct ConditionalVisibilityInfo
    {
        public readonly bool HasCondition;
        public readonly string SourcePropertyName;
        public readonly float ExpectedValue;
        public readonly bool Satisfied;
        public readonly bool Valid;

        public ConditionalVisibilityInfo(
            bool hasCondition,
            string sourcePropertyName,
            float expectedValue,
            bool satisfied,
            bool valid)
        {
            HasCondition = hasCondition;
            SourcePropertyName = sourcePropertyName ?? string.Empty;
            ExpectedValue = expectedValue;
            Satisfied = satisfied;
            Valid = valid;
        }
    }

    public sealed class ShaderPropertyModel
    {
        private readonly List<MaterialGUIDiagnostic> _diagnostics = new();
        private readonly ReadOnlyCollection<MaterialGUIDiagnostic> _diagnosticsView;

        internal ShaderPropertyModel()
        {
            _diagnosticsView = _diagnostics.AsReadOnly();
        }

        public MaterialProperty Property { get; internal set; } = null!;
        public string PropertyName { get; internal set; } = string.Empty;
        public string Label { get; internal set; } = string.Empty;
        public string TranslatedLabel { get; internal set; } = string.Empty;
        public string GroupPath { get; internal set; } = string.Empty;
        public MaterialProperty.PropType PropertyType { get; internal set; }
        public IReadOnlyList<ShaderPropertyAttributeCache.ShaderAttributeInfo> Attributes
        {
            get => _attributesView;
            internal set
            {
                _attributes = CopyToArray(value);
                _attributesView = Array.AsReadOnly(_attributes);
            }
        }

        public bool Changed { get; internal set; }
        public bool Mixed { get; internal set; }
        public bool ConditionVisible { get; internal set; } = true;
        public bool SearchMatched { get; internal set; } = true;
        public bool Visible { get; internal set; } = true;
        public ConditionalVisibilityInfo ConditionalVisibility { get; internal set; }
        public IReadOnlyList<MaterialGUIDiagnostic> Diagnostics => _diagnosticsView;

        internal List<MaterialGUIDiagnostic> MutableDiagnostics => _diagnostics;

        public int WarningCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _diagnostics.Count; i++)
                {
                    if (_diagnostics[i].Severity == MaterialGUIDiagnosticSeverity.Warning ||
                        _diagnostics[i].Severity == MaterialGUIDiagnosticSeverity.Error)
                        count++;
                }

                return count;
            }
        }

        private ShaderPropertyAttributeCache.ShaderAttributeInfo[] _attributes =
            Array.Empty<ShaderPropertyAttributeCache.ShaderAttributeInfo>();
        private IReadOnlyList<ShaderPropertyAttributeCache.ShaderAttributeInfo> _attributesView =
            Array.Empty<ShaderPropertyAttributeCache.ShaderAttributeInfo>();

        private static T[] CopyToArray<T>(IReadOnlyList<T>? source)
        {
            if (source == null || source.Count == 0)
                return Array.Empty<T>();

            var copy = new T[source.Count];
            for (var i = 0; i < source.Count; i++)
                copy[i] = source[i];
            return copy;
        }
    }

    public sealed class GroupNodeModel
    {
        private readonly List<ShaderPropertyModel> _properties = new();
        private readonly Dictionary<string, GroupNodeModel> _children = new(StringComparer.Ordinal);
        private readonly ReadOnlyCollection<ShaderPropertyModel> _propertiesView;
        private readonly ReadOnlyDictionary<string, GroupNodeModel> _childrenView;

        internal GroupNodeModel()
        {
            _propertiesView = _properties.AsReadOnly();
            _childrenView = new ReadOnlyDictionary<string, GroupNodeModel>(_children);
        }

        public string Name { get; internal set; } = string.Empty;
        public string PathKey { get; internal set; } = string.Empty;
        public bool Expanded { get; internal set; } = true;
        public bool Visible { get; internal set; } = true;
        public bool SearchMatchedSelf { get; internal set; }

        public IReadOnlyList<ShaderPropertyModel> Properties => _propertiesView;
        public IReadOnlyDictionary<string, GroupNodeModel> Children => _childrenView;

        public int TotalPropertyCount { get; internal set; }
        public int VisiblePropertyCount { get; internal set; }
        public int ChangedCount { get; internal set; }
        public int WarningCount { get; internal set; }
        public bool HasMixed { get; internal set; }

        internal List<ShaderPropertyModel> MutableProperties => _properties;
        internal Dictionary<string, GroupNodeModel> MutableChildren => _children;
    }

    public sealed class MaterialGUIModel
    {
        public GroupNodeModel Root { get; }
        public IReadOnlyList<ShaderPropertyModel> Properties { get; }
        public IReadOnlyList<MaterialGUIDiagnostic> Diagnostics { get; }

        internal MaterialGUIModel(
            GroupNodeModel root,
            IReadOnlyList<ShaderPropertyModel> properties,
            IReadOnlyList<MaterialGUIDiagnostic> diagnostics)
        {
            Root = root;
            Properties = ToReadOnly(properties);
            Diagnostics = ToReadOnly(diagnostics);
        }

        private static IReadOnlyList<T> ToReadOnly<T>(IReadOnlyList<T>? source)
        {
            if (source == null || source.Count == 0)
                return Array.AsReadOnly(Array.Empty<T>());

            var copy = new T[source.Count];
            for (var i = 0; i < source.Count; i++)
                copy[i] = source[i];
            return Array.AsReadOnly(copy);
        }
    }
}


