using Spine;

namespace Sektor.Spine.Skia;

/// <summary>Advances and applies Spine animations onto a skeleton instance.</summary>
public sealed class SpinePlayer : IDisposable
{
    private readonly AnimationState state;

    /// <summary>Initializes a new instance of the <see cref="SpinePlayer"/> class.</summary>
    /// <param name="skeletonData">The skeleton data to instantiate.</param>
    public SpinePlayer(SkeletonData skeletonData)
    {
        if (skeletonData == null)
            throw new ArgumentNullException(nameof(skeletonData));

        Skeleton = new Skeleton(skeletonData);
        Skeleton.SetToSetupPose();
        Skeleton.UpdateWorldTransform();
        state = new AnimationState(new AnimationStateData(skeletonData));
    }

    /// <summary>Gets the animated skeleton.</summary>
    public Skeleton Skeleton { get; }

    /// <summary>Gets the name of the currently playing animation, or <c>null</c> when nothing is playing.</summary>
    public string? CurrentAnimation { get; private set; }

    /// <summary>Gets or sets the animation time scale (Spine uses a float); values above one speed the animation up.</summary>
    public float TimeScale
    {
        get => state.TimeScale;
        set => state.TimeScale = value;
    }

    /// <summary>Starts playing an animation on the base track.</summary>
    /// <param name="animationName">The animation name (validated against the skeleton data).</param>
    /// <param name="loop">Whether the animation loops.</param>
    public void Play(string animationName, bool loop = true)
    {
        if (Skeleton.Data.FindAnimation(animationName) == null)
            throw new ArgumentException($"Animation '{animationName}' not found in skeleton data.", nameof(animationName));

        CurrentAnimation = animationName;
        state.SetAnimation(0, animationName, loop);
        state.Apply(Skeleton);
        Skeleton.UpdateWorldTransform();
    }

    /// <summary>Advances the animation by the given time step and applies it to the skeleton.</summary>
    /// <param name="deltaSeconds">Elapsed time in seconds.</param>
    public void Update(float deltaSeconds)
    {
        state.Update(deltaSeconds);
        state.Apply(Skeleton);
        Skeleton.UpdateWorldTransform();
    }

    /// <inheritdoc/>
    public void Dispose() => state.ClearTracks();
}
