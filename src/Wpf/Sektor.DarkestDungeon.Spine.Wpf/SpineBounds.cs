namespace Sektor.DarkestDungeon.Spine.Wpf;

/// <summary>Axis-aligned bounding box of a skeleton pose, in skeleton units.</summary>
public readonly struct SpineBounds
{
    /// <summary>An empty bounds value (no vertices measured).</summary>
    public static SpineBounds Empty { get; } = new SpineBounds(0f, 0f, 0f, 0f, isEmpty: true);

    /// <summary>Initializes a new instance of the <see cref="SpineBounds"/> struct.</summary>
    /// <param name="minX">Left edge.</param>
    /// <param name="minY">Bottom edge (skeleton space, Y up).</param>
    /// <param name="maxX">Right edge.</param>
    /// <param name="maxY">Top edge (skeleton space, Y up).</param>
    public SpineBounds(float minX, float minY, float maxX, float maxY)
        : this(minX, minY, maxX, maxY, isEmpty: false)
    {
    }

    private SpineBounds(float minX, float minY, float maxX, float maxY, bool isEmpty)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        IsEmpty = isEmpty;
    }

    /// <summary>Gets the left edge.</summary>
    public float MinX { get; }

    /// <summary>Gets the bottom edge (skeleton space, Y up).</summary>
    public float MinY { get; }

    /// <summary>Gets the right edge.</summary>
    public float MaxX { get; }

    /// <summary>Gets the top edge (skeleton space, Y up).</summary>
    public float MaxY { get; }

    /// <summary>Gets a value indicating whether no vertices were measured.</summary>
    public bool IsEmpty { get; }

    /// <summary>Gets the horizontal size.</summary>
    public float Width => MaxX - MinX;

    /// <summary>Gets the vertical size.</summary>
    public float Height => MaxY - MinY;

    /// <summary>Gets the horizontal center.</summary>
    public float CenterX => (MinX + MaxX) * 0.5f;

    /// <summary>Gets the vertical center.</summary>
    public float CenterY => (MinY + MaxY) * 0.5f;

    /// <summary>Returns the smallest bounds containing both operands.</summary>
    /// <param name="other">The other bounds.</param>
    /// <returns>The union of the two boxes.</returns>
    public SpineBounds Union(SpineBounds other)
    {
        if (IsEmpty)
            return other;
        if (other.IsEmpty)
            return this;
        return new SpineBounds(
            Math.Min(MinX, other.MinX),
            Math.Min(MinY, other.MinY),
            Math.Max(MaxX, other.MaxX),
            Math.Max(MaxY, other.MaxY));
    }
}
