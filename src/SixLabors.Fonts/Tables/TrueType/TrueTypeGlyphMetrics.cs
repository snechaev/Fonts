// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts.Tables.General;
using SixLabors.Fonts.Tables.TrueType.Glyphs;
using SixLabors.Fonts.Unicode;

namespace SixLabors.Fonts.Tables.TrueType
{
    /// <summary>
    /// Represents a glyph metric from a particular TrueType font face.
    /// </summary>
    public class TrueTypeGlyphMetrics : GlyphMetrics
    {
        private static readonly Vector2 MirrorScale = new(1, -1);
        private readonly GlyphVector vector;
        private readonly Dictionary<float, GlyphVector> scaledVector = new();

        internal TrueTypeGlyphMetrics(
            StreamFontMetrics font,
            ushort glyphId,
            CodePoint codePoint,
            GlyphVector vector,
            ushort advanceWidth,
            ushort advanceHeight,
            short leftSideBearing,
            short topSideBearing,
            ushort unitsPerEM,
            TextAttributes textAttributes,
            TextDecorations textDecorations,
            GlyphType glyphType = GlyphType.Standard,
            GlyphColor? glyphColor = null)
            : base(
                  font,
                  glyphId,
                  codePoint,
                  vector.GetBounds(),
                  advanceWidth,
                  advanceHeight,
                  leftSideBearing,
                  topSideBearing,
                  unitsPerEM,
                  textAttributes,
                  textDecorations,
                  glyphType,
                  glyphColor)
            => this.vector = vector;

        internal TrueTypeGlyphMetrics(
            StreamFontMetrics font,
            ushort glyphId,
            CodePoint codePoint,
            GlyphVector vector,
            ushort advanceWidth,
            ushort advanceHeight,
            short leftSideBearing,
            short topSideBearing,
            ushort unitsPerEM,
            Vector2 offset,
            Vector2 scaleFactor,
            TextAttributes textAttributes,
            TextDecorations textDecorations,
            GlyphType glyphType = GlyphType.Standard,
            GlyphColor? glyphColor = null)
            : base(
                  font,
                  glyphId,
                  codePoint,
                  vector.GetBounds(),
                  advanceWidth,
                  advanceHeight,
                  leftSideBearing,
                  topSideBearing,
                  unitsPerEM,
                  offset,
                  scaleFactor,
                  textAttributes,
                  textDecorations,
                  glyphType,
                  glyphColor)
            => this.vector = vector;

        /// <inheritdoc/>
        internal override GlyphMetrics CloneForRendering()
            => new TrueTypeGlyphMetrics(
                this.FontMetrics,
                this.GlyphId,
                this.CodePoint,
                GlyphVector.DeepClone(this.vector),
                this.AdvanceWidth,
                this.AdvanceHeight,
                this.LeftSideBearing,
                this.TopSideBearing,
                this.UnitsPerEm,
                this.Offset,
                this.ScaleFactor,
                this.TextAttributes,
                this.TextDecorations,
                this.GlyphType,
                this.GlyphColor);

        /// <summary>
        /// Gets the outline for the current glyph.
        /// </summary>
        /// <returns>The <see cref="GlyphOutline"/>.</returns>
        public GlyphOutline GetOutline() => this.vector.GetOutline();

        /// <inheritdoc/>
        internal override void RenderTo(IGlyphRenderer renderer, float pointSize, Vector2 location, TextOptions options)
        {
            // https://www.unicode.org/faq/unsup_char.html
            if (ShouldSkipGlyphRendering(this.CodePoint))
            {
                return;
            }

            // TODO: Move to base class
            float dpi = options.Dpi;
            location *= dpi;
            float scaledPPEM = dpi * pointSize;
            bool forcePPEMToInt = (this.FontMetrics.HeadFlags & HeadTable.HeadFlags.ForcePPEMToInt) != 0;

            if (forcePPEMToInt)
            {
                scaledPPEM = MathF.Round(scaledPPEM);
            }

            FontRectangle box = this.GetBoundingBox(location, scaledPPEM);
            GlyphRendererParameters parameters = new(this, this.TextAttributes, this.TextDecorations, pointSize, dpi);

            if (renderer.BeginGlyph(box, parameters))
            {
                if (!ShouldRenderWhiteSpaceOnly(this.CodePoint))
                {
                    if (this.GlyphColor.HasValue && renderer is IColorGlyphRenderer colorSurface)
                    {
                        colorSurface.SetColor(this.GlyphColor.Value);
                    }

                    if (!this.scaledVector.TryGetValue(scaledPPEM, out GlyphVector scaledVector))
                    {
                        // Create a scaled deep copy of the vector so that we do not alter
                        // the globally cached instance.
                        scaledVector = GlyphVector.DeepClone(this.vector);

                        Vector2 scale = new Vector2(scaledPPEM) / this.ScaleFactor;
                        var matrix = Matrix3x2.CreateScale(scale);
                        matrix.Translation = this.Offset * scale;
                        GlyphVector.TransformInPlace(ref scaledVector, matrix);

                        float pixelSize = scaledPPEM / 72F;
                        this.FontMetrics.ApplyTrueTypeHinting(options.HintingMode, this, ref scaledVector, scale, pixelSize);

                        this.scaledVector[scaledPPEM] = scaledVector;
                    }

                    GlyphOutline outline = scaledVector.GetOutline();
                    ReadOnlySpan<Vector2> controlPoints = outline.ControlPoints.Span;
                    ReadOnlySpan<ushort> endPoints = outline.EndPoints.Span;
                    ReadOnlySpan<bool> onCurves = outline.OnCurves.Span;

                    int endOfContour = -1;
                    for (int i = 0; i < outline.EndPoints.Length; i++)
                    {
                        renderer.BeginFigure();
                        int startOfContour = endOfContour + 1;
                        endOfContour = endPoints[i];

                        Vector2 prev;
                        Vector2 curr = (MirrorScale * controlPoints[endOfContour]) + location;
                        Vector2 next = (MirrorScale * controlPoints[startOfContour]) + location;

                        if (onCurves[endOfContour])
                        {
                            renderer.MoveTo(curr);
                        }
                        else
                        {
                            if (onCurves[startOfContour])
                            {
                                renderer.MoveTo(next);
                            }
                            else
                            {
                                // If both first and last points are off-curve, start at their middle.
                                Vector2 startPoint = (curr + next) * .5F;
                                renderer.MoveTo(startPoint);
                            }
                        }

                        int length = endOfContour - startOfContour + 1;
                        for (int p = 0; p < length; p++)
                        {
                            prev = curr;
                            curr = next;
                            int currentIndex = startOfContour + p;
                            int nextIndex = startOfContour + ((p + 1) % length);
                            int prevIndex = startOfContour + ((length + p - 1) % length);
                            next = (MirrorScale * controlPoints[nextIndex]) + location;

                            if (onCurves[currentIndex])
                            {
                                // This is a straight line.
                                renderer.LineTo(curr);
                            }
                            else
                            {
                                Vector2 prev2 = prev;
                                Vector2 next2 = next;

                                if (!onCurves[prevIndex])
                                {
                                    prev2 = (curr + prev) * .5F;
                                    renderer.LineTo(prev2);
                                }

                                if (!onCurves[nextIndex])
                                {
                                    next2 = (curr + next) * .5F;
                                }

                                renderer.LineTo(prev2);
                                renderer.QuadraticBezierTo(curr, next2);
                            }
                        }

                        renderer.EndFigure();
                    }
                }

                this.RenderDecorationsTo(renderer, location, scaledPPEM);
            }

            renderer.EndGlyph();
        }
    }
}
