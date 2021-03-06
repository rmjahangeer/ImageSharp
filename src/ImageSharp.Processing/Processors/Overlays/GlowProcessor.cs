﻿// <copyright file="GlowProcessor.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Processing.Processors
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IImageProcessor{TColor}"/> that applies a radial glow effect an <see cref="Image{TColor}"/>.
    /// </summary>
    /// <typeparam name="TColor">The pixel format.</typeparam>
    public class GlowProcessor<TColor> : ImageProcessor<TColor>
        where TColor : struct, IPixel<TColor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlowProcessor{TColor}" /> class.
        /// </summary>
        /// <param name="color">The color or the glow.</param>
        public GlowProcessor(TColor color)
        {
            this.GlowColor = color;
        }

        /// <summary>
        /// Gets or sets the glow color to apply.
        /// </summary>
        public TColor GlowColor { get; set; }

        /// <summary>
        /// Gets or sets the the radius.
        /// </summary>
        public float Radius { get; set; }

        /// <inheritdoc/>
        protected override void OnApply(ImageBase<TColor> source, Rectangle sourceRectangle)
        {
            int startY = sourceRectangle.Y;
            int endY = sourceRectangle.Bottom;
            int startX = sourceRectangle.X;
            int endX = sourceRectangle.Right;
            TColor glowColor = this.GlowColor;
            Vector2 centre = Rectangle.Center(sourceRectangle).ToVector2();
            float maxDistance = this.Radius > 0 ? Math.Min(this.Radius, sourceRectangle.Width * .5F) : sourceRectangle.Width * .5F;

            // Align start/end positions.
            int minX = Math.Max(0, startX);
            int maxX = Math.Min(source.Width, endX);
            int minY = Math.Max(0, startY);
            int maxY = Math.Min(source.Height, endY);

            // Reset offset if necessary.
            if (minX > 0)
            {
                startX = 0;
            }

            if (minY > 0)
            {
                startY = 0;
            }

            using (PixelAccessor<TColor> sourcePixels = source.Lock())
            {
                Parallel.For(
                    minY,
                    maxY,
                    this.ParallelOptions,
                    y =>
                        {
                            int offsetY = y - startY;
                            for (int x = minX; x < maxX; x++)
                            {
                                int offsetX = x - startX;
                                float distance = Vector2.Distance(centre, new Vector2(offsetX, offsetY));
                                Vector4 sourceColor = sourcePixels[offsetX, offsetY].ToVector4();
                                TColor packed = default(TColor);
                                packed.PackFromVector4(Vector4BlendTransforms.PremultipliedLerp(sourceColor, glowColor.ToVector4(), 1 - (.95F * (distance / maxDistance))));
                                sourcePixels[offsetX, offsetY] = packed;
                            }
                        });
            }
        }
    }
}