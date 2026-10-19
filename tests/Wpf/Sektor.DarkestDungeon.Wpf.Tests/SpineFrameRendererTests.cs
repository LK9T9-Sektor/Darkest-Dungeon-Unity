using System;

using NUnit.Framework;
using SkiaSharp;

using Sektor.DarkestDungeon.Spine.Wpf;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Renders the Crusader idle animation headlessly onto a SkiaSharp surface.</summary>
    [TestFixture]
    public class SpineFrameRendererTests
    {
        private const int FrameSize = 256;

        /// <summary>Frame 0 must contain visible pixels, and advancing the idle animation must change them.</summary>
        [Test]
        public void IdleAnimation_RendersNonBlankAndMovingFrames()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var renderer = new SpineFrameRenderer();
            using var player = content.CreatePlayer();
            player.Play("idle");

            SpineBounds bounds = renderer.MeasureAnimation(content.SkeletonData, "idle");
            Assert.That(bounds.Width, Is.GreaterThan(0f), "measured animation bounds must be non-empty");
            Assert.That(bounds.Height, Is.GreaterThan(0f), "measured animation bounds must be non-empty");

            using var first = RenderFrame(renderer, player, bounds);
            for (int i = 0; i < 30; i++)
                player.Update(1f / 60f);
            using var second = RenderFrame(renderer, player, bounds);

            Assert.That(CountPainted(first), Is.GreaterThan(500), "first frame must contain visible pixels");
            Assert.That(CountDifferences(first, second), Is.GreaterThan(0), "idle animation must move between frames");
        }

        /// <summary>A freshly created player must expose a positioned setup pose ready for measuring and rendering.</summary>
        [Test]
        public void Measure_SetupPose_ReturnsNonEmptyBounds()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var renderer = new SpineFrameRenderer();
            using var player = content.CreatePlayer();

            SpineBounds bounds = renderer.Measure(player.Skeleton);

            Assert.That(bounds.IsEmpty, Is.False);
            Assert.That(bounds.Width, Is.GreaterThan(0f));
            Assert.That(bounds.Height, Is.GreaterThan(0f));
        }

        /// <summary>Transparent atlas areas must stay transparent inside the silhouette (regression: vertex colors bleached them white).</summary>
        [Test]
        public void IdleAnimation_KeepsTransparentPixelsTransparent()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var renderer = new SpineFrameRenderer();
            using var player = content.CreatePlayer();
            player.Play("idle");
            player.Update(1f / 60f);

            SpineBounds bounds = renderer.Measure(player.Skeleton);
            using var frame = RenderFrame(renderer, player, bounds);

            var projection = new FrameProjection(bounds);
            int total = 0;
            int transparent = 0;
            for (int y = projection.Top; y <= projection.Bottom; y++)
            {
                for (int x = projection.Left; x <= projection.Right; x++)
                {
                    total++;
                    if (frame.GetPixel(x, y).Alpha <= 8)
                        transparent++;
                }
            }

            Assert.That(total, Is.GreaterThan(0), "measured bounds must cover pixels");
            double transparentRatio = transparent / (double)total;
            Assert.That(transparentRatio, Is.GreaterThan(0.15), "transparent atlas areas inside the silhouette must stay transparent");
            Assert.That(transparentRatio, Is.LessThan(0.9), "the character must actually be painted");
        }

        private static SKBitmap RenderFrame(SpineFrameRenderer renderer, SpinePlayer player, SpineBounds bounds)
        {
            var projection = new FrameProjection(bounds);
            using var surface = SKSurface.Create(new SKImageInfo(FrameSize, FrameSize, SKColorType.Bgra8888, SKAlphaType.Premul));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            renderer.Render(canvas, player.Skeleton, projection.Scale, projection.OriginX, projection.OriginY);
            canvas.Flush();

            using var image = surface.Snapshot();
            return SKBitmap.FromImage(image);
        }

        private static int CountPainted(SKBitmap bitmap)
        {
            int painted = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).Alpha > 8)
                        painted++;
                }
            }

            return painted;
        }

        private static int CountDifferences(SKBitmap first, SKBitmap second)
        {
            int differences = 0;
            for (int y = 0; y < first.Height; y++)
            {
                for (int x = 0; x < first.Width; x++)
                {
                    if (first.GetPixel(x, y) != second.GetPixel(x, y))
                        differences++;
                }
            }

            return differences;
        }

        private sealed class FrameProjection
        {
            public FrameProjection(SpineBounds bounds)
            {
                Scale = Math.Min(FrameSize / bounds.Width, FrameSize / bounds.Height) * 0.92f;
                OriginX = (FrameSize * 0.5f) - (bounds.CenterX * Scale);
                OriginY = (FrameSize * 0.5f) + (bounds.CenterY * Scale);
                Left = Clamp((int)Math.Floor(OriginX + (bounds.MinX * Scale)));
                Right = Clamp((int)Math.Ceiling(OriginX + (bounds.MaxX * Scale)));
                Top = Clamp((int)Math.Floor(OriginY - (bounds.MaxY * Scale)));
                Bottom = Clamp((int)Math.Ceiling(OriginY - (bounds.MinY * Scale)));
            }

            public float Scale { get; }

            public float OriginX { get; }

            public float OriginY { get; }

            public int Left { get; }

            public int Right { get; }

            public int Top { get; }

            public int Bottom { get; }

            private static int Clamp(int value) => Math.Max(0, Math.Min(FrameSize - 1, value));
        }
    }
}
