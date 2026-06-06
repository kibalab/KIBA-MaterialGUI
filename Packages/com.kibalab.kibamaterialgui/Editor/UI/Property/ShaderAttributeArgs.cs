#nullable enable

using System;
using System.Globalization;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public readonly struct ShaderAttributeArgs<TArgs>
    {
        public readonly PropertyRendererArgs Base;
        public readonly ShaderPropertyAttributeCache.ShaderAttributeInfo Attribute;
        public readonly TArgs Arguments;

        public ShaderAttributeArgs(
            PropertyRendererArgs @base,
            in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute,
            TArgs arguments)
        {
            Base = @base;
            Attribute = attribute;
            Arguments = arguments;
        }

        public Rect Position => Base.Position;
        public MaterialEditor? MaterialEditor => Base.MaterialEditor;
        public Material? Material => Base.Material;
        public MaterialProperty Property => Base.Property;
        public string Label => Base.Label;
        public GUIStyle MiniGray => Base.MiniGray;
        public Shader? Shader => Base.Shader;
    }

    public static class ShaderAttributeArgumentBinder
    {
        public static bool TryParseObject<TArgs>(string rawArgs, out TArgs value)
        {
            var tokens = ShaderAttributeArgumentParser.Split(rawArgs);
            return TryParseTokens(tokens, out value);
        }

        public static bool TryParse<T1>(string rawArgs, out T1 arg1)
        {
            arg1 = default!;
            var tokens = ShaderAttributeArgumentParser.Split(rawArgs);
            if (tokens.Length != 1) return false;
            if (!TryConvertToken(tokens[0], typeof(T1), out var v1)) return false;
            arg1 = (T1)v1!;
            return true;
        }

        public static bool TryParse<T1, T2>(string rawArgs, out T1 arg1, out T2 arg2)
        {
            arg1 = default!;
            arg2 = default!;
            var tokens = ShaderAttributeArgumentParser.Split(rawArgs);
            if (tokens.Length != 2) return false;
            if (!TryConvertToken(tokens[0], typeof(T1), out var v1)) return false;
            if (!TryConvertToken(tokens[1], typeof(T2), out var v2)) return false;
            arg1 = (T1)v1!;
            arg2 = (T2)v2!;
            return true;
        }

        public static bool TryParse<T1, T2, T3>(string rawArgs, out T1 arg1, out T2 arg2, out T3 arg3)
        {
            arg1 = default!;
            arg2 = default!;
            arg3 = default!;
            var tokens = ShaderAttributeArgumentParser.Split(rawArgs);
            if (tokens.Length != 3) return false;
            if (!TryConvertToken(tokens[0], typeof(T1), out var v1)) return false;
            if (!TryConvertToken(tokens[1], typeof(T2), out var v2)) return false;
            if (!TryConvertToken(tokens[2], typeof(T3), out var v3)) return false;
            arg1 = (T1)v1!;
            arg2 = (T2)v2!;
            arg3 = (T3)v3!;
            return true;
        }

        public static bool TryParse<T1, T2, T3, T4>(string rawArgs, out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4)
        {
            arg1 = default!;
            arg2 = default!;
            arg3 = default!;
            arg4 = default!;
            var tokens = ShaderAttributeArgumentParser.Split(rawArgs);
            if (tokens.Length != 4) return false;
            if (!TryConvertToken(tokens[0], typeof(T1), out var v1)) return false;
            if (!TryConvertToken(tokens[1], typeof(T2), out var v2)) return false;
            if (!TryConvertToken(tokens[2], typeof(T3), out var v3)) return false;
            if (!TryConvertToken(tokens[3], typeof(T4), out var v4)) return false;
            arg1 = (T1)v1!;
            arg2 = (T2)v2!;
            arg3 = (T3)v3!;
            arg4 = (T4)v4!;
            return true;
        }

        private static bool TryParseTokens<TArgs>(string[] tokens, out TArgs value)
        {
            value = default!;

            var targetType = typeof(TArgs);
            if (TryConvertSingleValue(tokens, targetType, out var single))
            {
                value = (TArgs)single!;
                return true;
            }

            if (!TryCreateByConstructor(tokens, targetType, out var instance))
                return false;

            value = (TArgs)instance!;
            return true;
        }

        private static bool TryConvertSingleValue(string[] tokens, Type targetType, out object? value)
        {
            value = null;
            if (tokens.Length == 0) return false;
            if (tokens.Length > 1) return false;

            return TryConvertToken(tokens[0], targetType, out value);
        }

        private static bool TryCreateByConstructor(string[] tokens, Type targetType, out object? instance)
        {
            instance = null;

            var ctors = targetType.GetConstructors()
                .OrderByDescending(static c => c.GetParameters().Length)
                .ToArray();
            if (ctors.Length == 0) return false;

            for (int c = 0; c < ctors.Length; c++)
            {
                var ctor = ctors[c];
                var parameters = ctor.GetParameters();
                if (tokens.Length > parameters.Length) continue;

                var args = new object?[parameters.Length];
                var ok = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i < tokens.Length)
                    {
                        if (!TryConvertToken(tokens[i], parameters[i].ParameterType, out var parsed))
                        {
                            ok = false;
                            break;
                        }

                        args[i] = parsed;
                        continue;
                    }

                    if (parameters[i].HasDefaultValue)
                    {
                        args[i] = parameters[i].DefaultValue;
                        continue;
                    }

                    ok = false;
                    break;
                }

                if (!ok) continue;
                try
                {
                    instance = ctor.Invoke(args);
                    return true;
                }
                catch (Exception ex)
                {
                    KIBA_.KIBAMaterialGUI.Editor.Core.MaterialGUIInternalDiagnostics.WarnOnce(
                        "attribute-args.create:" + ctor.DeclaringType?.FullName + ":" + ex.GetType().FullName,
                        "Failed to create shader attribute args '" + ctor.DeclaringType?.FullName + "': " + ex.Message);
                }
            }

            return false;
        }

        private static bool TryConvertToken(string rawToken, Type targetType, out object? value)
        {
            value = null;
            var token = ShaderAttributeArgumentParser.TrimQuotes(rawToken?.Trim() ?? string.Empty);

            var nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    value = null;
                    return true;
                }

                targetType = nullableType;
            }

            if (targetType == typeof(string))
            {
                value = token;
                return true;
            }

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(token, out var b))
                {
                    value = b;
                    return true;
                }

                if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var bf))
                {
                    value = Mathf.Abs(bf) > Mathf.Epsilon;
                    return true;
                }

                return false;
            }

            if (targetType == typeof(int))
            {
                if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    return false;
                value = i;
                return true;
            }

            if (targetType == typeof(float))
            {
                if (!float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    return false;
                value = f;
                return true;
            }

            if (targetType == typeof(double))
            {
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                    return false;
                value = d;
                return true;
            }

            if (targetType == typeof(long))
            {
                if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                    return false;
                value = l;
                return true;
            }

            if (targetType.IsEnum)
            {
                try
                {
                    value = Enum.Parse(targetType, token, true);
                    return true;
                }
                catch
                {
                    if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iv))
                        return false;
                    value = Enum.ToObject(targetType, iv);
                    return true;
                }
            }

            return false;
        }
    }

    public abstract class ShaderPropertyRenderer<TArgs> : IMaterialGUIPropertyRenderer, IMaterialGUIPropertyRendererFilter
    {
        protected abstract string AttributeName { get; }

        protected virtual bool SupportsPropertyType(MaterialProperty.PropType propertyType) => true;

        protected virtual bool TryParseArguments(
            in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute,
            out TArgs arguments)
        {
            return ShaderAttributeArgumentBinder.TryParseObject(attribute.args, out arguments);
        }

        public bool CanRender(PropertyRendererArgs args)
        {
            return TryBuildArgs(args, out _);
        }

        public float GetHeight(PropertyRendererArgs args)
        {
            if (!TryBuildArgs(args, out var typed))
                return EditorGUIUtility.singleLineHeight;

            return GetHeight(typed);
        }

        public Rect OnGUI(PropertyRendererArgs args)
        {
            if (!TryBuildArgs(args, out var typed))
                return args.Position;

            return OnGUI(typed);
        }

        protected abstract float GetHeight(in ShaderAttributeArgs<TArgs> args);
        protected abstract Rect OnGUI(in ShaderAttributeArgs<TArgs> args);

        private bool TryBuildArgs(PropertyRendererArgs args, out ShaderAttributeArgs<TArgs> typed)
        {
            typed = default;
            if (!SupportsPropertyType(args.Property.type)) return false;
            if (string.IsNullOrWhiteSpace(AttributeName)) return false;
            if (!args.TryGetShaderAttribute(AttributeName, out var attribute)) return false;
            if (!TryParseArguments(attribute, out var parsed)) return false;

            typed = new ShaderAttributeArgs<TArgs>(args, attribute, parsed);
            return true;
        }
    }

    public abstract class ShaderPropertyRenderer<T1, T2> : ShaderPropertyRenderer<(T1, T2)>
    {
        protected override bool TryParseArguments(in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute, out (T1, T2) arguments)
        {
            arguments = default;
            if (!ShaderAttributeArgumentBinder.TryParse(attribute.args, out T1 arg1, out T2 arg2))
                return false;
            arguments = (arg1, arg2);
            return true;
        }

        protected sealed override float GetHeight(in ShaderAttributeArgs<(T1, T2)> args)
        {
            return GetHeight(args.Base, args.Arguments.Item1, args.Arguments.Item2);
        }

        protected sealed override Rect OnGUI(in ShaderAttributeArgs<(T1, T2)> args)
        {
            return OnGUI(args.Base, args.Arguments.Item1, args.Arguments.Item2);
        }

        protected abstract float GetHeight(PropertyRendererArgs args, T1 arg1, T2 arg2);
        protected abstract Rect OnGUI(PropertyRendererArgs args, T1 arg1, T2 arg2);
    }

    public abstract class ShaderPropertyRenderer<T1, T2, T3> : ShaderPropertyRenderer<(T1, T2, T3)>
    {
        protected override bool TryParseArguments(in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute, out (T1, T2, T3) arguments)
        {
            arguments = default;
            if (!ShaderAttributeArgumentBinder.TryParse(attribute.args, out T1 arg1, out T2 arg2, out T3 arg3))
                return false;
            arguments = (arg1, arg2, arg3);
            return true;
        }

        protected sealed override float GetHeight(in ShaderAttributeArgs<(T1, T2, T3)> args)
        {
            return GetHeight(args.Base, args.Arguments.Item1, args.Arguments.Item2, args.Arguments.Item3);
        }

        protected sealed override Rect OnGUI(in ShaderAttributeArgs<(T1, T2, T3)> args)
        {
            return OnGUI(args.Base, args.Arguments.Item1, args.Arguments.Item2, args.Arguments.Item3);
        }

        protected abstract float GetHeight(PropertyRendererArgs args, T1 arg1, T2 arg2, T3 arg3);
        protected abstract Rect OnGUI(PropertyRendererArgs args, T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class ShaderPropertyRenderer<T1, T2, T3, T4> : ShaderPropertyRenderer<(T1, T2, T3, T4)>
    {
        protected override bool TryParseArguments(in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute, out (T1, T2, T3, T4) arguments)
        {
            arguments = default;
            if (!ShaderAttributeArgumentBinder.TryParse(attribute.args, out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4))
                return false;
            arguments = (arg1, arg2, arg3, arg4);
            return true;
        }

        protected sealed override float GetHeight(in ShaderAttributeArgs<(T1, T2, T3, T4)> args)
        {
            return GetHeight(args.Base, args.Arguments.Item1, args.Arguments.Item2, args.Arguments.Item3, args.Arguments.Item4);
        }

        protected sealed override Rect OnGUI(in ShaderAttributeArgs<(T1, T2, T3, T4)> args)
        {
            return OnGUI(args.Base, args.Arguments.Item1, args.Arguments.Item2, args.Arguments.Item3, args.Arguments.Item4);
        }

        protected abstract float GetHeight(PropertyRendererArgs args, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        protected abstract Rect OnGUI(PropertyRendererArgs args, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }
}


