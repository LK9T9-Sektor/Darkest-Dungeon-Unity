using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extracts diagnostic properties from scene components into <see cref="ComponentSnapshot"/>.
/// Uses a Type-to-extractor registry (no switch dispatch): every registered extractor whose type
/// the component instance implements is applied, so base-class properties (for example
/// <see cref="Renderer"/> material and shader) come before type-specific ones. All values are
/// formatted with the invariant culture.
/// </summary>
public class SceneSnapshotPropertyReader
{
    private readonly List<Type> _types = new List<Type>();
    private readonly List<Action<Component, ComponentSnapshot>> _extractors =
        new List<Action<Component, ComponentSnapshot>>();

    /// <summary>Creates the reader with all built-in extractors registered.</summary>
    public SceneSnapshotPropertyReader()
    {
        Register<Behaviour>(ReadBehaviour);
        Register<Transform>(ReadTransform);
        Register<RectTransform>(ReadRectTransform);
        Register<Renderer>(ReadRenderer);
        Register<SpriteRenderer>(ReadSpriteRenderer);
        Register<Camera>(ReadCamera);
        Register<Canvas>(ReadCanvas);
        Register<CanvasGroup>(ReadCanvasGroup);
        Register<CanvasRenderer>(ReadCanvasRenderer);
        Register<Graphic>(ReadGraphic);
        Register<Image>(ReadImage);
    }

    /// <summary>Applies every matching extractor to the given component.</summary>
    /// <param name="component">The component to inspect (must not be null).</param>
    /// <param name="target">The snapshot to fill.</param>
    public void Read(Component component, ComponentSnapshot target)
    {
        for (int i = 0; i < _types.Count; i++)
        {
            if (_types[i].IsInstanceOfType(component))
            {
                _extractors[i](component, target);
            }
        }
    }

    private void Register<T>(Action<T, ComponentSnapshot> extractor) where T : Component
    {
        _types.Add(typeof(T));
        _extractors.Add((component, target) => extractor((T)component, target));
    }

    private static void Add(ComponentSnapshot target, string name, string value)
    {
        ComponentPropertySnapshot property = new ComponentPropertySnapshot();
        property.Name = name;
        property.Value = value;
        target.Properties.Add(property);
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static string FormatVector2(Vector2 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "(x={0:0.######}, y={1:0.######})",
            value.x, value.y);
    }

    private static string FormatVector3(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "(x={0:0.######}, y={1:0.######}, z={2:0.######})",
            value.x, value.y, value.z);
    }

    private static string FormatColor(Color value)
    {
        return string.Format(CultureInfo.InvariantCulture,
            "(r={0:0.####}, g={1:0.####}, b={2:0.####}, a={3:0.####})",
            value.r, value.g, value.b, value.a);
    }

    private static string FormatRect(Rect value)
    {
        return string.Format(CultureInfo.InvariantCulture,
            "(x={0:0.###}, y={1:0.###}, w={2:0.###}, h={3:0.###})",
            value.x, value.y, value.width, value.height);
    }

    private static string ObjectName(UnityEngine.Object value)
    {
        return value != null ? value.name : "<null>";
    }

    private static string ShaderName(Material material)
    {
        return material != null && material.shader != null ? material.shader.name : "<null>";
    }

    private static string TextureName(Sprite sprite)
    {
        return sprite != null && sprite.texture != null ? sprite.texture.name : "<null>";
    }

    private static void ReadBehaviour(Behaviour behaviour, ComponentSnapshot target)
    {
        Add(target, "enabled", FormatBool(behaviour.enabled));
    }

    private static void ReadTransform(Transform transform, ComponentSnapshot target)
    {
        Add(target, "position", FormatVector3(transform.position));
        Add(target, "localPosition", FormatVector3(transform.localPosition));
        Add(target, "localScale", FormatVector3(transform.localScale));
        Add(target, "worldRotationEuler", FormatVector3(transform.rotation.eulerAngles));
        Add(target, "localRotationEuler", FormatVector3(transform.localRotation.eulerAngles));
        Add(target, "childCount", transform.childCount.ToString(CultureInfo.InvariantCulture));
    }

    private static void ReadRectTransform(RectTransform rectTransform, ComponentSnapshot target)
    {
        Add(target, "anchoredPosition", FormatVector2(rectTransform.anchoredPosition));
        Add(target, "sizeDelta", FormatVector2(rectTransform.sizeDelta));
        Add(target, "pivot", FormatVector2(rectTransform.pivot));
        Add(target, "anchorMin", FormatVector2(rectTransform.anchorMin));
        Add(target, "anchorMax", FormatVector2(rectTransform.anchorMax));
        Add(target, "rect", FormatRect(rectTransform.rect));
    }

    private static void ReadRenderer(Renderer renderer, ComponentSnapshot target)
    {
        Add(target, "enabled", FormatBool(renderer.enabled));
        Add(target, "sharedMaterial", ObjectName(renderer.sharedMaterial));
        Add(target, "shader", ShaderName(renderer.sharedMaterial));
        Add(target, "sortingLayerName", renderer.sortingLayerName);
        Add(target, "sortingOrder", renderer.sortingOrder.ToString(CultureInfo.InvariantCulture));
        Add(target, "isVisible", FormatBool(renderer.isVisible));
        Add(target, "boundsCenter", FormatVector3(renderer.bounds.center));
        Add(target, "boundsSize", FormatVector3(renderer.bounds.size));
    }

    private static void ReadSpriteRenderer(SpriteRenderer renderer, ComponentSnapshot target)
    {
        Add(target, "sprite", ObjectName(renderer.sprite));
        Add(target, "spriteTexture", TextureName(renderer.sprite));
        Add(target, "color", FormatColor(renderer.color));
        Add(target, "flipX", FormatBool(renderer.flipX));
        Add(target, "flipY", FormatBool(renderer.flipY));
        Add(target, "drawMode", renderer.drawMode.ToString());
        Add(target, "size", FormatVector2(renderer.size));
    }

    private static void ReadCamera(Camera camera, ComponentSnapshot target)
    {
        Add(target, "orthographic", FormatBool(camera.orthographic));
        Add(target, "orthographicSize", FormatFloat(camera.orthographicSize));
        Add(target, "fieldOfView", FormatFloat(camera.fieldOfView));
        Add(target, "clearFlags", camera.clearFlags.ToString());
        Add(target, "backgroundColor", FormatColor(camera.backgroundColor));
        Add(target, "cullingMask", camera.cullingMask.ToString(CultureInfo.InvariantCulture));
        Add(target, "depth", FormatFloat(camera.depth));
        Add(target, "rect", FormatRect(camera.rect));
    }

    private static void ReadCanvas(Canvas canvas, ComponentSnapshot target)
    {
        Add(target, "renderMode", canvas.renderMode.ToString());
        Add(target, "sortingLayerName", canvas.sortingLayerName);
        Add(target, "sortingOrder", canvas.sortingOrder.ToString(CultureInfo.InvariantCulture));
        Add(target, "overrideSorting", FormatBool(canvas.overrideSorting));
        Add(target, "scaleFactor", FormatFloat(canvas.scaleFactor));
        Add(target, "worldCamera", ObjectName(canvas.worldCamera));
    }

    private static void ReadCanvasGroup(CanvasGroup canvasGroup, ComponentSnapshot target)
    {
        Add(target, "alpha", FormatFloat(canvasGroup.alpha));
        Add(target, "interactable", FormatBool(canvasGroup.interactable));
        Add(target, "blocksRaycasts", FormatBool(canvasGroup.blocksRaycasts));
        Add(target, "ignoreParentGroups", FormatBool(canvasGroup.ignoreParentGroups));
    }

    private static void ReadCanvasRenderer(CanvasRenderer canvasRenderer, ComponentSnapshot target)
    {
        Add(target, "alpha", FormatFloat(canvasRenderer.GetAlpha()));
        Add(target, "cull", FormatBool(canvasRenderer.cull));
        Add(target, "materialCount", canvasRenderer.materialCount.ToString(CultureInfo.InvariantCulture));
        Material renderMaterial = canvasRenderer.GetMaterial();
        Add(target, "renderMaterial", ObjectName(renderMaterial));
        Add(target, "renderShader", ShaderName(renderMaterial));
        Texture renderTexture = renderMaterial != null ? renderMaterial.mainTexture : null;
        Add(target, "renderTexture", renderTexture != null ? renderTexture.name : "<null>");
        if (renderTexture != null)
        {
            Add(target, "renderTextureSize", renderTexture.width + "x" + renderTexture.height);
        }
    }

    private static void ReadGraphic(Graphic graphic, ComponentSnapshot target)
    {
        Add(target, "color", FormatColor(graphic.color));
        Add(target, "raycastTarget", FormatBool(graphic.raycastTarget));
        Add(target, "graphicMaterial", ObjectName(graphic.material));
        Add(target, "graphicShader", ShaderName(graphic.material));
        Add(target, "canvasRendererAlpha", FormatFloat(graphic.canvasRenderer.GetAlpha()));
    }

    private static void ReadImage(Image image, ComponentSnapshot target)
    {
        Add(target, "sprite", ObjectName(image.sprite));
        Add(target, "spriteTexture", TextureName(image.sprite));
        Add(target, "imageType", image.type.ToString());
        Add(target, "fillAmount", FormatFloat(image.fillAmount));
        Add(target, "preserveAspect", FormatBool(image.preserveAspect));
        if (image.sprite != null)
        {
            Add(target, "spriteRect", FormatRect(image.sprite.rect));
            Add(target, "spriteTextureRect", FormatRect(image.sprite.textureRect));
            Add(target, "spritePixelsPerUnit", FormatFloat(image.sprite.pixelsPerUnit));
        }

        Texture2D texture = image.sprite != null ? image.sprite.texture : null;
        if (texture != null)
        {
            Add(target, "textureSize", texture.width + "x" + texture.height);
            Add(target, "textureFormat", texture.format.ToString());
            Add(target, "textureMipCount", texture.mipmapCount.ToString(CultureInfo.InvariantCulture));
            Add(target, "textureIsReadable", FormatBool(texture.isReadable));
            Add(target, "textureFilterMode", texture.filterMode.ToString());
            Add(target, "textureWrapMode", texture.wrapMode.ToString());
        }
    }
}
