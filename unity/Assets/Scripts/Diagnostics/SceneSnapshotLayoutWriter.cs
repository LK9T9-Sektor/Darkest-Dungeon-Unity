using System.Text;

/// <summary>
/// Builds the compact CSV layout list for a <see cref="SceneSnapshot"/>: one row per UI rect node
/// (anchor geometry, screen-space rect in pixels and screen fractions, sprite, text) plus summary
/// rows for canvases (with their <see cref="UnityEngine.UI.CanvasScaler"/>) and cameras. The file
/// is written next to the JSON by <see cref="SceneSnapshotDumper"/>. All values are the property
/// strings produced by <see cref="SceneSnapshotPropertyReader"/> (the constant names below mirror
/// its property keys), so both artifacts stay in sync; it is meant for AI/WPF consumption, not for
/// human-authored parsing.
/// </summary>
public static class SceneSnapshotLayoutWriter
{
    private const string KindCanvas = "Canvas";
    private const string KindCamera = "Camera";
    private const string KindUi = "UI";

    private const string TypeRectTransform = "UnityEngine.RectTransform";
    private const string TypeCanvas = "UnityEngine.Canvas";
    private const string TypeCanvasScaler = "UnityEngine.UI.CanvasScaler";
    private const string TypeCamera = "UnityEngine.Camera";
    private const string TypeImage = "UnityEngine.UI.Image";
    private const string TypeText = "UnityEngine.UI.Text";

    private const string PropertyAnchoredPosition = "anchoredPosition";
    private const string PropertySizeDelta = "sizeDelta";
    private const string PropertyPivot = "pivot";
    private const string PropertyAnchorMin = "anchorMin";
    private const string PropertyAnchorMax = "anchorMax";
    private const string PropertyScreenRectPx = "screenRectPx";
    private const string PropertyScreenRectNorm = "screenRectNorm";
    private const string PropertySprite = "sprite";
    private const string PropertyImageType = "imageType";
    private const string PropertyText = "text";
    private const string PropertyFontSize = "fontSize";
    private const string PropertyRenderMode = "renderMode";
    private const string PropertySortingOrder = "sortingOrder";
    private const string PropertyScaleFactor = "scaleFactor";
    private const string PropertyUiScaleMode = "uiScaleMode";
    private const string PropertyReferenceResolution = "referenceResolution";
    private const string PropertyScreenMatchMode = "screenMatchMode";
    private const string PropertyMatchWidthOrHeight = "matchWidthOrHeight";
    private const string PropertyOrthographic = "orthographic";
    private const string PropertyOrthographicSize = "orthographicSize";
    private const string PropertyFieldOfView = "fieldOfView";
    private const string PropertyClearFlags = "clearFlags";
    private const string PropertyDepth = "depth";
    private const string PropertyPixelRect = "pixelRect";

    private static readonly string[] Header =
    {
        "Kind", "NodePath", "Canvas", "Active",
        "AnchorMin", "AnchorMax", "Pivot", "AnchoredPosition", "SizeDelta",
        "ScreenRectPx", "ScreenRectNorm",
        "Sprite", "ImageType", "Text", "FontSize",
        "RenderMode", "SortingOrder", "ScaleFactor",
        "UiScaleMode", "ReferenceResolution", "ScreenMatchMode", "MatchWidthOrHeight",
        "Orthographic", "OrthographicSize", "FieldOfView", "ClearFlags", "Depth", "PixelRect"
    };

    /// <summary>Builds the CSV text for the snapshot (CRLF line endings, every field quoted).</summary>
    /// <param name="snapshot">The snapshot to describe (null-safe; an empty list yields the header).</param>
    /// <returns>The complete CSV document as a string.</returns>
    public static string Build(SceneSnapshot snapshot)
    {
        StringBuilder builder = new StringBuilder();
        AppendRow(builder, Header);
        if (snapshot != null && snapshot.Roots != null)
        {
            for (int i = 0; i < snapshot.Roots.Count; i++)
            {
                AppendNode(builder, snapshot.Roots[i], string.Empty);
            }
        }

        return builder.ToString();
    }

    private static void AppendNode(StringBuilder builder, SceneNodeSnapshot node, string parentCanvasPath)
    {
        if (node == null)
        {
            return;
        }

        string canvasPath = parentCanvasPath;
        ComponentSnapshot canvas = FindComponent(node, TypeCanvas);
        if (canvas != null)
        {
            canvasPath = node.Path;
            ComponentSnapshot scaler = FindComponent(node, TypeCanvasScaler);
            AppendRow(builder, new[]
            {
                KindCanvas, node.Path, node.Path, FormatBool(node.ActiveInHierarchy),
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty,
                Value(canvas, PropertyRenderMode), Value(canvas, PropertySortingOrder),
                Value(canvas, PropertyScaleFactor),
                Value(scaler, PropertyUiScaleMode), Value(scaler, PropertyReferenceResolution),
                Value(scaler, PropertyScreenMatchMode), Value(scaler, PropertyMatchWidthOrHeight),
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
            });
        }

        ComponentSnapshot camera = FindComponent(node, TypeCamera);
        if (camera != null)
        {
            AppendRow(builder, new[]
            {
                KindCamera, node.Path, canvasPath, FormatBool(node.ActiveInHierarchy),
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty,
                Value(camera, PropertyOrthographic), Value(camera, PropertyOrthographicSize),
                Value(camera, PropertyFieldOfView), Value(camera, PropertyClearFlags),
                Value(camera, PropertyDepth), Value(camera, PropertyPixelRect)
            });
        }

        ComponentSnapshot rect = FindComponent(node, TypeRectTransform);
        if (rect != null)
        {
            ComponentSnapshot image = FindComponent(node, TypeImage);
            ComponentSnapshot text = FindComponent(node, TypeText);
            AppendRow(builder, new[]
            {
                KindUi, node.Path, canvasPath, FormatBool(node.ActiveInHierarchy),
                Value(rect, PropertyAnchorMin), Value(rect, PropertyAnchorMax),
                Value(rect, PropertyPivot), Value(rect, PropertyAnchoredPosition),
                Value(rect, PropertySizeDelta),
                Value(rect, PropertyScreenRectPx), Value(rect, PropertyScreenRectNorm),
                Value(image, PropertySprite), Value(image, PropertyImageType),
                Value(text, PropertyText), Value(text, PropertyFontSize),
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
            });
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            AppendNode(builder, node.Children[i], canvasPath);
        }
    }

    private static ComponentSnapshot FindComponent(SceneNodeSnapshot node, string typeFullName)
    {
        for (int i = 0; i < node.Components.Count; i++)
        {
            if (node.Components[i].Type == typeFullName)
            {
                return node.Components[i];
            }
        }

        return null;
    }

    private static string Value(ComponentSnapshot component, string propertyName)
    {
        if (component == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < component.Properties.Count; i++)
        {
            if (component.Properties[i].Name == propertyName)
            {
                return component.Properties[i].Value ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static void AppendRow(StringBuilder builder, string[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            AppendField(builder, fields[i]);
        }

        builder.Append("\r\n");
    }

    private static void AppendField(StringBuilder builder, string value)
    {
        builder.Append('"');
        if (!string.IsNullOrEmpty(value))
        {
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character == '"')
                {
                    builder.Append('"');
                }

                builder.Append(character);
            }
        }

        builder.Append('"');
    }
}
