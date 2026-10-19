using SkiaSharp;
using Spine;

namespace Sektor.Spine.Skia;

/// <summary>Renders a Spine skeleton onto a SkiaSharp canvas and measures its world bounds.</summary>
public sealed class SpineFrameRenderer : IDisposable
{
    private static readonly IReadOnlyDictionary<BlendMode, SKBlendMode> SkiaBlendModes = new Dictionary<BlendMode, SKBlendMode>
    {
        [BlendMode.normal] = SKBlendMode.SrcOver,
        [BlendMode.additive] = SKBlendMode.Plus,
        [BlendMode.multiply] = SKBlendMode.Modulate,
        [BlendMode.screen] = SKBlendMode.Screen,
    };

    private readonly AttachmentGeometries geometries = AttachmentGeometries.CreateDefault();
    private readonly List<CachedPaint> paints = new List<CachedPaint>();
    private float[] vertexBuffer = new float[8];
    private bool disposed;

    /// <summary>Renders the skeleton with the given scale and screen-space origin of the skeleton root.</summary>
    /// <param name="canvas">The destination canvas.</param>
    /// <param name="skeleton">The posed skeleton (world transform already updated).</param>
    /// <param name="scale">Pixels per skeleton unit.</param>
    /// <param name="originX">Screen X of the skeleton root.</param>
    /// <param name="originY">Screen Y of the skeleton root (Y down, skeleton Y is flipped).</param>
    public void Render(SKCanvas canvas, Skeleton skeleton, float scale, float originX, float originY)
    {
        if (canvas == null)
            throw new ArgumentNullException(nameof(canvas));
        if (skeleton == null)
            throw new ArgumentNullException(nameof(skeleton));
        ThrowIfDisposed();

        foreach (Slot slot in skeleton.DrawOrder)
        {
            Attachment? attachment = slot.Attachment;
            if (attachment == null)
                continue;

            AttachmentGeometry? geometry = geometries.Find(attachment);
            if (geometry == null)
                continue;

            SKBitmap? texture = geometry.GetTexture(attachment);
            if (texture == null)
                continue;

            AttachmentTint tint = geometry.GetTint(attachment);
            float alpha = skeleton.A * slot.A * tint.A;
            if (alpha <= 0f)
                continue;

            int vertexCount = geometry.GetVertexCount(attachment);
            EnsureVertexBuffer(vertexCount * 2);
            geometry.ComputeWorldVertices(slot, attachment, vertexBuffer);

            float[] uvs = geometry.GetUvs(attachment);
            var positions = new SKPoint[vertexCount];
            var texs = new SKPoint[vertexCount];
            SKColor color = CreateColor(skeleton, slot, tint, alpha);
            float textureWidth = texture.Width;
            float textureHeight = texture.Height;

            for (int i = 0, v = 0; i < vertexCount; i++, v += 2)
            {
                positions[i] = new SKPoint(
                    originX + (vertexBuffer[v] * scale),
                    originY - (vertexBuffer[v + 1] * scale));
                texs[i] = new SKPoint(uvs[v] * textureWidth, uvs[v + 1] * textureHeight);
            }

            SKBlendMode blend = ResolveBlendMode(slot);
            using var vertices = SKVertices.CreateCopy(
                SKVertexMode.Triangles, positions, texs, null, ToIndices(geometry.GetTriangles(attachment)));
            canvas.DrawVertices(vertices, blend, GetPaint(texture, blend, color));
        }
    }

    /// <summary>Measures the world-space bounds of the current skeleton pose.</summary>
    /// <param name="skeleton">The posed skeleton (world transform already updated).</param>
    /// <returns>The bounds, or <see cref="SpineBounds.Empty"/> when nothing is renderable.</returns>
    public SpineBounds Measure(Skeleton skeleton)
    {
        if (skeleton == null)
            throw new ArgumentNullException(nameof(skeleton));
        ThrowIfDisposed();

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        bool found = false;

        foreach (Slot slot in skeleton.DrawOrder)
        {
            Attachment? attachment = slot.Attachment;
            if (attachment == null)
                continue;

            AttachmentGeometry? geometry = geometries.Find(attachment);
            if (geometry == null)
                continue;

            int vertexCount = geometry.GetVertexCount(attachment);
            EnsureVertexBuffer(vertexCount * 2);
            geometry.ComputeWorldVertices(slot, attachment, vertexBuffer);

            for (int v = 0; v < vertexCount * 2; v += 2)
            {
                minX = Math.Min(minX, vertexBuffer[v]);
                maxX = Math.Max(maxX, vertexBuffer[v]);
                minY = Math.Min(minY, vertexBuffer[v + 1]);
                maxY = Math.Max(maxY, vertexBuffer[v + 1]);
            }

            found = true;
        }

        return found ? new SpineBounds(minX, minY, maxX, maxY) : SpineBounds.Empty;
    }

    /// <summary>Measures the union bounds of an animation across its duration on a throwaway skeleton.</summary>
    /// <param name="skeletonData">The skeleton data owning the animation.</param>
    /// <param name="animationName">The animation name (validated against the skeleton data).</param>
    /// <param name="sampleCount">The number of evenly spaced samples.</param>
    /// <returns>The union bounds.</returns>
    public SpineBounds MeasureAnimation(SkeletonData skeletonData, string animationName, int sampleCount = 12)
    {
        if (skeletonData == null)
            throw new ArgumentNullException(nameof(skeletonData));
        if (sampleCount < 1)
            throw new ArgumentOutOfRangeException(nameof(sampleCount));
        ThrowIfDisposed();

        Animation animation = skeletonData.FindAnimation(animationName)
            ?? throw new ArgumentException($"Animation '{animationName}' not found in skeleton data.", nameof(animationName));

        var probe = new Skeleton(skeletonData);
        var state = new AnimationState(new AnimationStateData(skeletonData));
        state.SetAnimation(0, animation, true);
        float step = animation.Duration / sampleCount;
        SpineBounds bounds = SpineBounds.Empty;

        for (int i = 0; i < sampleCount; i++)
        {
            state.Apply(probe);
            probe.UpdateWorldTransform();
            bounds = bounds.Union(Measure(probe));
            state.Update(step);
        }

        return bounds;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        foreach (CachedPaint cached in paints)
        {
            cached.Paint.Dispose();
            cached.Shader.Dispose();
            cached.ColorFilter.Dispose();
        }

        paints.Clear();
    }

    private static SKColor CreateColor(Skeleton skeleton, Slot slot, AttachmentTint tint, float alpha)
        => new SKColor(
            ToByte(skeleton.R * slot.R * tint.R),
            ToByte(skeleton.G * slot.G * tint.G),
            ToByte(skeleton.B * slot.B * tint.B),
            ToByte(alpha));

    private static byte ToByte(float value) => (byte)Math.Clamp(value * 255f, 0f, 255f);

    private static ushort[] ToIndices(int[] triangles)
    {
        var indices = new ushort[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            indices[i] = (ushort)triangles[i];

        return indices;
    }

    private static SKBlendMode ResolveBlendMode(Slot slot)
        => SkiaBlendModes.TryGetValue(slot.Data.BlendMode, out SKBlendMode mode) ? mode : SKBlendMode.SrcOver;

    private void EnsureVertexBuffer(int length)
    {
        if (vertexBuffer.Length < length)
            vertexBuffer = new float[length];
    }

    private SKPaint GetPaint(SKBitmap texture, SKBlendMode blend, SKColor color)
    {
        foreach (CachedPaint cached in paints)
        {
            if (cached.Texture == texture && cached.Blend == blend && cached.Color == color)
                return cached.Paint;
        }

        SKShader shader = SKShader.CreateBitmap(texture, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
        SKColorFilter colorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate);
        var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.Low,
            Shader = shader,
            ColorFilter = colorFilter,
        };
        paints.Add(new CachedPaint(texture, blend, color, shader, colorFilter, paint));
        return paint;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(SpineFrameRenderer));
    }

    private sealed class CachedPaint
    {
        public CachedPaint(SKBitmap texture, SKBlendMode blend, SKColor color, SKShader shader, SKColorFilter colorFilter, SKPaint paint)
        {
            Texture = texture;
            Blend = blend;
            Color = color;
            Shader = shader;
            ColorFilter = colorFilter;
            Paint = paint;
        }

        public SKBitmap Texture { get; }

        public SKBlendMode Blend { get; }

        public SKColor Color { get; }

        public SKShader Shader { get; }

        public SKColorFilter ColorFilter { get; }

        public SKPaint Paint { get; }
    }
}
