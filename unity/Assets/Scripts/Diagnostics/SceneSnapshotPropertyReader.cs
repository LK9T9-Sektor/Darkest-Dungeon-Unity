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
    private const int TextPreviewLength = 80;

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
        Register<CanvasScaler>(ReadCanvasScaler);
        Register<Text>(ReadText);
        Register<LayoutGroup>(ReadLayoutGroup);
        Register<HorizontalOrVerticalLayoutGroup>(ReadHorizontalOrVerticalLayoutGroup);
        Register<GridLayoutGroup>(ReadGridLayoutGroup);
        Register<ContentSizeFitter>(ReadContentSizeFitter);
        Register<LayoutElement>(ReadLayoutElement);
        Register<AspectRatioFitter>(ReadAspectRatioFitter);
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
        AddScreenRect(rectTransform, target);
    }

    /// <summary>
    /// Projects the rect's world corners to screen pixels and screen fractions via the owning
    /// root canvas (overlay canvases need no camera, camera/world-space canvases use their
    /// world camera or <see cref="Camera.main"/> as a fallback).
    /// </summary>
    private static void AddScreenRect(RectTransform rectTransform, ComponentSnapshot target)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            return;
        }

        canvas = canvas.rootCanvas;
        Camera camera = null;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            camera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            if (camera == null)
            {
                Add(target, "screenRectPx", "<no-camera>");
                return;
            }
        }

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        Add(target, "screenRectPx", FormatRectMinMax(min, max, "0.##"));
        if (Screen.width > 0 && Screen.height > 0)
        {
            Vector2 normalizedMin = new Vector2(min.x / Screen.width, min.y / Screen.height);
            Vector2 normalizedMax = new Vector2(max.x / Screen.width, max.y / Screen.height);
            Add(target, "screenRectNorm", FormatRectMinMax(normalizedMin, normalizedMax, "0.####"));
        }
    }

    private static string FormatRectMinMax(Vector2 min, Vector2 max, string numberFormat)
    {
        return string.Format(CultureInfo.InvariantCulture, "(x={0}, y={1}, w={2}, h={3})",
            min.x.ToString(numberFormat, CultureInfo.InvariantCulture),
            min.y.ToString(numberFormat, CultureInfo.InvariantCulture),
            (max.x - min.x).ToString(numberFormat, CultureInfo.InvariantCulture),
            (max.y - min.y).ToString(numberFormat, CultureInfo.InvariantCulture));
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
        Add(target, "pixelRect", FormatRect(camera.pixelRect));
        Add(target, "targetTexture", ObjectName(camera.targetTexture));
    }

    private static void ReadCanvas(Canvas canvas, ComponentSnapshot target)
    {
        Add(target, "renderMode", canvas.renderMode.ToString());
        Add(target, "sortingLayerName", canvas.sortingLayerName);
        Add(target, "sortingOrder", canvas.sortingOrder.ToString(CultureInfo.InvariantCulture));
        Add(target, "overrideSorting", FormatBool(canvas.overrideSorting));
        Add(target, "pixelPerfect", FormatBool(canvas.pixelPerfect));
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
        Add(target, "fillMethod", image.fillMethod.ToString());
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

    private static void ReadCanvasScaler(CanvasScaler canvasScaler, ComponentSnapshot target)
    {
        Add(target, "uiScaleMode", canvasScaler.uiScaleMode.ToString());
        Add(target, "referenceResolution", FormatVector2(canvasScaler.referenceResolution));
        Add(target, "screenMatchMode", canvasScaler.screenMatchMode.ToString());
        Add(target, "matchWidthOrHeight", FormatFloat(canvasScaler.matchWidthOrHeight));
        Add(target, "referencePixelsPerUnit", FormatFloat(canvasScaler.referencePixelsPerUnit));
    }

    private static void ReadText(Text text, ComponentSnapshot target)
    {
        string value = text.text ?? string.Empty;
        if (value.Length > TextPreviewLength)
        {
            value = value.Substring(0, TextPreviewLength) + "...";
        }

        Add(target, "text", value);
        Add(target, "font", ObjectName(text.font));
        Add(target, "fontSize", text.fontSize.ToString(CultureInfo.InvariantCulture));
        Add(target, "alignment", text.alignment.ToString());
    }

    private static void ReadLayoutGroup(LayoutGroup layoutGroup, ComponentSnapshot target)
    {
        Add(target, "padding", FormatPadding(layoutGroup.padding));
        Add(target, "childAlignment", layoutGroup.childAlignment.ToString());
    }

    private static void ReadHorizontalOrVerticalLayoutGroup(HorizontalOrVerticalLayoutGroup layoutGroup,
        ComponentSnapshot target)
    {
        Add(target, "spacing", FormatFloat(layoutGroup.spacing));
        Add(target, "childControlWidth", FormatBool(layoutGroup.childControlWidth));
        Add(target, "childControlHeight", FormatBool(layoutGroup.childControlHeight));
        Add(target, "childForceExpandWidth", FormatBool(layoutGroup.childForceExpandWidth));
        Add(target, "childForceExpandHeight", FormatBool(layoutGroup.childForceExpandHeight));
    }

    private static void ReadGridLayoutGroup(GridLayoutGroup layoutGroup, ComponentSnapshot target)
    {
        Add(target, "cellSize", FormatVector2(layoutGroup.cellSize));
        Add(target, "spacing", FormatVector2(layoutGroup.spacing));
        Add(target, "constraint", layoutGroup.constraint.ToString());
        Add(target, "constraintCount", layoutGroup.constraintCount.ToString(CultureInfo.InvariantCulture));
    }

    private static void ReadContentSizeFitter(ContentSizeFitter fitter, ComponentSnapshot target)
    {
        Add(target, "horizontalFit", fitter.horizontalFit.ToString());
        Add(target, "verticalFit", fitter.verticalFit.ToString());
    }

    private static void ReadLayoutElement(LayoutElement element, ComponentSnapshot target)
    {
        Add(target, "minWidth", FormatFloat(element.minWidth));
        Add(target, "minHeight", FormatFloat(element.minHeight));
        Add(target, "preferredWidth", FormatFloat(element.preferredWidth));
        Add(target, "preferredHeight", FormatFloat(element.preferredHeight));
        Add(target, "flexibleWidth", FormatFloat(element.flexibleWidth));
        Add(target, "flexibleHeight", FormatFloat(element.flexibleHeight));
    }

    private static void ReadAspectRatioFitter(AspectRatioFitter fitter, ComponentSnapshot target)
    {
        Add(target, "aspectMode", fitter.aspectMode.ToString());
        Add(target, "aspectRatio", FormatFloat(fitter.aspectRatio));
    }

    private static string FormatPadding(RectOffset padding)
    {
        return string.Format(CultureInfo.InvariantCulture, "(l={0}, r={1}, t={2}, b={3})",
            padding.left, padding.right, padding.top, padding.bottom);
    }
}
