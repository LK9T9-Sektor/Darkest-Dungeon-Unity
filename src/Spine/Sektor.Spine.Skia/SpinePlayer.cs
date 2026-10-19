using Spine;

namespace Sektor.Spine.Skia;

/// <summary>Advances and applies Spine animations onto a skeleton instance.</summary>
public sealed class SpinePlayer : IDisposable
{
    private readonly AnimationState state;
    private bool completionArmed;
    private string? pendingCompletion;

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
        state.Complete += OnStateComplete;
    }

    /// <summary>Gets the animated skeleton.</summary>
    public Skeleton Skeleton { get; }

    /// <summary>Gets the name of the currently playing animation, or <c>null</c> when nothing is playing.</summary>
    public string? CurrentAnimation { get; private set; }

    /// <summary>
    /// Raised once after an animation started with <c>loop: false</c> has finished. The event is
    /// deferred to the end of <see cref="Update"/>, so subscribers may stop, swap or dispose the
    /// player from inside the handler.
    /// </summary>
    public event Action<string>? AnimationCompleted;

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
        completionArmed = !loop;
        pendingCompletion = null;
        state.SetAnimation(0, animationName, loop);
        state.Apply(Skeleton);
        Skeleton.UpdateWorldTransform();
    }

    /// <summary>Advances the animation by the given time step and applies it to the skeleton.</summary>
    /// <param name="deltaSeconds">Elapsed time in seconds.</param>
    public void Update(float deltaSeconds)
    {
        // A zero-duration animation (pose stub content) never satisfies the runtime's own
        // completion check (lastTime < endTime is false from the start), so an armed one-shot
        // would hang forever - complete it on the first tick instead.
        if (completionArmed)
        {
            TrackEntry? entry = state.GetCurrent(0);
            if (entry != null && entry.EndTime <= 0f)
            {
                completionArmed = false;
                pendingCompletion = CurrentAnimation ?? string.Empty;
            }
        }

        state.Update(deltaSeconds);
        state.Apply(Skeleton);
        Skeleton.UpdateWorldTransform();

        if (pendingCompletion != null)
        {
            string completed = pendingCompletion;
            pendingCompletion = null;
            AnimationCompleted?.Invoke(completed);
        }
    }

    private void OnStateComplete(AnimationState _, int trackIndex, int loopCount)
    {
        if (trackIndex != 0 || !completionArmed)
            return;

        completionArmed = false;
        pendingCompletion = CurrentAnimation ?? string.Empty;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        state.Complete -= OnStateComplete;
        state.ClearTracks();
    }
}
