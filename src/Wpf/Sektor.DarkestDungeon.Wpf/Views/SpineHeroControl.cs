using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Sektor.DarkestDungeon.Spine.Wpf;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Plays a Spine hero animation and renders it onto a SkiaSharp element.</summary>
    public class SpineHeroControl : SKElement
    {
        /// <summary>Identifies the <see cref="ContentDirectory"/> dependency property.</summary>
        public static readonly DependencyProperty ContentDirectoryProperty = DependencyProperty.Register(
            nameof(ContentDirectory),
            typeof(string),
            typeof(SpineHeroControl),
            new PropertyMetadata(string.Empty, OnContentPropertyChanged));

        /// <summary>Identifies the <see cref="AnimationName"/> dependency property.</summary>
        public static readonly DependencyProperty AnimationNameProperty = DependencyProperty.Register(
            nameof(AnimationName),
            typeof(string),
            typeof(SpineHeroControl),
            new PropertyMetadata(string.Empty, OnContentPropertyChanged));

        /// <summary>Identifies the <see cref="IsPaused"/> dependency property.</summary>
        public static readonly DependencyProperty IsPausedProperty = DependencyProperty.Register(
            nameof(IsPaused),
            typeof(bool),
            typeof(SpineHeroControl),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="SpeedMultiplier"/> dependency property.</summary>
        public static readonly DependencyProperty SpeedMultiplierProperty = DependencyProperty.Register(
            nameof(SpeedMultiplier),
            typeof(double),
            typeof(SpineHeroControl),
            new PropertyMetadata(1.0, OnSpeedMultiplierChanged));

        private const float MaxFrameSeconds = 0.1f;
        private const float MinSpeedMultiplier = 0.01f;
        private const float MaxSpeedMultiplier = 100f;

        private readonly Stopwatch clock = new Stopwatch();
        private SpineContent? content;
        private SpinePlayer? player;
        private SpineFrameRenderer? renderer;
        private SpineBounds bounds = SpineBounds.Empty;
        private string loadedDirectory = string.Empty;
        private string loadedAnimation = string.Empty;
        private string? loadError;
        private TimeSpan lastTick;
        private bool ticking;

        /// <summary>Initializes a new instance of the <see cref="SpineHeroControl"/> class.</summary>
        public SpineHeroControl()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>Gets or sets the content directory with the skeleton, atlas and page texture (relative to the app base directory).</summary>
        public string ContentDirectory
        {
            get => (string)GetValue(ContentDirectoryProperty);
            set => SetValue(ContentDirectoryProperty, value);
        }

        /// <summary>Gets or sets the animation to play; an empty value plays the skeleton's first animation.</summary>
        public string AnimationName
        {
            get => (string)GetValue(AnimationNameProperty);
            set => SetValue(AnimationNameProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the animation clock is paused.</summary>
        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }

        /// <summary>Gets or sets the animation speed multiplier (1 = normal speed, 10 = ten times faster).</summary>
        public double SpeedMultiplier
        {
            get => (double)GetValue(SpeedMultiplierProperty);
            set => SetValue(SpeedMultiplierProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (loadError != null)
            {
                using var paint = new SKPaint { Color = SKColors.DarkOrange, TextSize = 14f };
                canvas.DrawText($"Spine content error: {loadError}", 12f, 24f, paint);
                return;
            }

            if (player == null || renderer == null || bounds.IsEmpty)
                return;

            float width = e.Info.Width;
            float height = e.Info.Height;
            float scale = Math.Min(width / bounds.Width, height / bounds.Height) * 0.92f;
            float originX = (width * 0.5f) - (bounds.CenterX * scale);
            float originY = (height * 0.5f) + (bounds.CenterY * scale);
            renderer.Render(canvas, player.Skeleton, scale, originX, originY);
        }

        private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((SpineHeroControl)d).OnContentPropertyChanged();

        private static void OnSpeedMultiplierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((SpineHeroControl)d).ApplyTimeScale();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!ticking)
            {
                clock.Restart();
                lastTick = TimeSpan.Zero;
                CompositionTarget.Rendering += OnRendering;
                ticking = true;
            }

            OnContentPropertyChanged();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (ticking)
            {
                CompositionTarget.Rendering -= OnRendering;
                ticking = false;
            }

            DisposeContent();
        }

        private void OnContentPropertyChanged()
        {
            if (!IsLoaded)
                return;

            string resolvedDirectory = ResolveDirectory();
            if (content == null || !string.Equals(resolvedDirectory, loadedDirectory, StringComparison.OrdinalIgnoreCase))
            {
                TryLoad(resolvedDirectory);
                return;
            }

            string animationName = ResolveAnimationName();
            if (player != null && !string.Equals(animationName, loadedAnimation, StringComparison.Ordinal))
                PlayAnimation(animationName);
        }

        private string ResolveDirectory()
        {
            string directory = ContentDirectory ?? string.Empty;
            return Path.IsPathRooted(directory) ? directory : Path.Combine(AppContext.BaseDirectory, directory);
        }

        private string ResolveAnimationName()
            => string.IsNullOrEmpty(AnimationName) && content != null ? content.DefaultAnimationName : (AnimationName ?? string.Empty);

        private void TryLoad(string directory)
        {
            DisposeContent();
            loadedDirectory = directory;

            try
            {
                string skeletonFile = FindSingleFile(directory, "*.skel*");
                string atlasFile = FindSingleFile(directory, "*.atlas*");
                content = new SpineSkeletonLoader().Load(skeletonFile, atlasFile);
                player = content.CreatePlayer();
                renderer = new SpineFrameRenderer();
                ApplyTimeScale();
                PlayAnimation(ResolveAnimationName());
            }
            catch (Exception exception)
            {
                loadError = exception.Message;
                DisposeContent();
            }

            InvalidateVisual();
        }

        private void ApplyTimeScale()
        {
            if (player != null)
                player.TimeScale = (float)Math.Clamp(SpeedMultiplier, MinSpeedMultiplier, MaxSpeedMultiplier);
        }

        private void PlayAnimation(string animationName)
        {
            if (content == null || player == null || renderer == null)
                return;
            if (string.IsNullOrEmpty(animationName))
            {
                loadError = "Skeleton has no animations.";
                return;
            }

            player.Play(animationName);
            bounds = renderer.MeasureAnimation(content.SkeletonData, animationName);
            loadedAnimation = animationName;
            loadError = null;
        }

        private static string FindSingleFile(string directory, string pattern)
        {
            string[] files = Directory.GetFiles(directory, pattern);
            if (files.Length == 0)
                throw new FileNotFoundException($"No '{pattern}' file found in '{directory}'.");
            if (files.Length > 1)
                throw new InvalidDataException($"Ambiguous content in '{directory}': {files.Length} files match '{pattern}'.");

            return files[0];
        }

        private void DisposeContent()
        {
            renderer?.Dispose();
            renderer = null;
            player?.Dispose();
            player = null;
            content?.Dispose();
            content = null;
            bounds = SpineBounds.Empty;
            loadError = null;
            loadedAnimation = string.Empty;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            TimeSpan now = clock.Elapsed;
            float deltaSeconds = (float)(now - lastTick).TotalSeconds;
            lastTick = now;

            if (player == null || IsPaused || deltaSeconds <= 0f)
                return;

            player.Update(Math.Min(deltaSeconds, MaxFrameSeconds));
            InvalidateVisual();
        }
    }
}
