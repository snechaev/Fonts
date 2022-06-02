// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

// TODO: Remove and cleanup
// There are several evaluation features missing here that are implemented in
// PDFPig's Type2CharStringParse.cs and FontKit CFFGlyph.js
// <auto-generated/>
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SixLabors.Fonts.Tables.Cff
{
    internal ref struct Type2EvaluationStack
    {
        private RefStack<double> argStack;
        private bool isDisposed;

        public Type2EvaluationStack(IGlyphRenderer glyphRenderer, double x, double y)
        {
            this.GlyphRenderer = glyphRenderer;
            this.argStack = new RefStack<double>(50);
            this.CurrentX = x;
            this.CurrentY = y;
            this.isDisposed = false;
        }

        public IGlyphRenderer GlyphRenderer { get; }

        public double CurrentX { get; private set; }

        public double CurrentY { get; private set; }

        public void Push(double value) => this.argStack.Push(value);

        public void Return() => this.argStack.Clear();

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.argStack.Dispose();
            this.isDisposed = true;
        }

        // Many operators take their arguments from the bottom-most
        // entries in the Type 2 argument stack; this behavior is indicated
        // by the stack bottom symbol ‘| -’ appearing to the left of the first
        // argument.Operators that clear the argument stack are
        // indicated by the stack bottom symbol ‘| -’ in the result position
        // of the operator definition

        // [NOTE4]:
        // The first stack - clearing operator, which must be one of...

        // hstem, hstemhm, vstem, vstemhm, cntrmask,
        // hintmask, hmoveto, vmoveto, rmoveto, or endchar,

        // ...
        // takes an additional argument — the width(as
        // described earlier), which may be expressed as zero or one numeric
        // argument

        //-------------------------
        // 4.1: Path Construction Operators

        /// <summary>
        /// rmoveto
        /// </summary>
        public void R_MoveTo()
        {
            // |- dx1 dy1 rmoveto(21) |-

            // moves the current point to
            // a position at the relative coordinates(dx1, dy1)
            // see [NOTE4]
            this.GlyphRenderer.EndFigure();

            // TODO: FK does width check.
            this.CurrentX += this.argStack.Shift();
            this.CurrentY += this.argStack.Shift();
            this.GlyphRenderer.MoveTo(new Vector2((float)this.CurrentX, (float)this.CurrentY));
            this.argStack.Clear();
        }

        /// <summary>
        /// hmoveto
        /// </summary>
        public void H_MoveTo()
        {
            // |- dx1 hmoveto(22) |-

            // moves the current point
            // dx1 units in the horizontal direction
            // see [NOTE4]

            // TODO: FK does width check.
            this.CurrentX += this.argStack.Shift();
            this.GlyphRenderer.MoveTo(new Vector2((float)this.CurrentX, (float)this.CurrentY));
            this.argStack.Clear();
        }

        public void V_MoveTo()
        {
            // |- dy1 vmoveto (4) |-
            // moves the current point
            // dy1 units in the vertical direction.
            // see [NOTE4]

            // TODO: FK does width check.
            this.CurrentY += this.argStack.Shift();
            this.GlyphRenderer.MoveTo(new Vector2((float)this.CurrentX, (float)this.CurrentY));
            this.argStack.Clear();
        }

        public void R_LineTo()
        {
            // |- {dxa dya}+  rlineto (5) |-

            // appends a line from the current point to
            // a position at the relative coordinates dxa, dya.

            // Additional rlineto operations are
            // performed for all subsequent argument pairs.

            // The number of
            // lines is determined from the number of arguments on the stack
            while (this.argStack.Length >= 2)
            {
                this.CurrentX += this.argStack.Shift();
                this.CurrentY += this.argStack.Shift();
                this.GlyphRenderer.LineTo(new Vector2((float)this.CurrentX, (float)this.CurrentY));
            }

            this.argStack.Clear();
        }

        public void H_LineTo()
        {
            // |- dx1 {dya dxb}*  hlineto (6) |-
            // |- {dxa dyb}+  hlineto (6) |-

            // appends a horizontal line of length
            // dx1 to the current point.

            // With an odd number of arguments, subsequent argument pairs
            // are interpreted as alternating values of
            // dy and dx, for which additional lineto
            // operators draw alternating vertical and
            // horizontal lines.

            // With an even number of arguments, the
            // arguments are interpreted as alternating horizontal and
            // vertical lines. The number of lines is determined from the
            // number of arguments on the stack.
            bool phase = true;
            while (this.argStack.Length >= 1)
            {
                if (phase)
                {
                    this.CurrentX += this.argStack.Shift();
                }
                else
                {
                    this.CurrentY += this.argStack.Shift();
                }

                this.GlyphRenderer.LineTo(new Vector2((float)this.CurrentX, (float)this.CurrentY));
                phase = !phase;
            }

            this.argStack.Clear();
        }

        public void V_LineTo()
        {
            // |- dy1 {dxa dyb}*  vlineto (7) |-
            // |- {dya dxb}+  vlineto (7) |-

            // appends a vertical line of length
            // dy1 to the current point.

            // With an odd number of arguments, subsequent argument pairs are
            // interpreted as alternating values of dx and dy, for which additional
            // lineto operators draw alternating horizontal and
            // vertical lines.

            // With an even number of arguments, the
            // arguments are interpreted as alternating vertical and
            // horizontal lines. The number of lines is determined from the
            // number of arguments on the stack.
            // first elem
            bool phase = false;
            while (this.argStack.Length >= 1)
            {
                if (phase)
                {
                    this.CurrentX += this.argStack.Shift();
                }
                else
                {
                    this.CurrentY += this.argStack.Shift();
                }

                this.GlyphRenderer.LineTo(new Vector2((float)this.CurrentX, (float)this.CurrentY));
                phase = !phase;
            }

            this.argStack.Clear();
        }

        public void RR_CurveTo()
        {
            // |- {dxa dya dxb dyb dxc dyc}+  rrcurveto (8) |-

            // appends a Bézier curve, defined by dxa...dyc, to the current point.

            // For each subsequent set of six arguments, an additional
            // curve is appended to the current point.

            // The number of curve segments is determined from
            // the number of arguments on the number stack and
            // is limited only by the size of the number stack

            // All Bézier curve path segments are drawn using six arguments,
            // dxa, dya, dxb, dyb, dxc, dyc; where dxa and dya are relative to
            // the current point, and all subsequent arguments are relative to
            // the previous point.A number of the curve operators take
            // advantage of the situation where some tangent points are
            // horizontal or vertical(and hence the value is zero), thus
            // reducing the number of arguments needed.
            double x = this.CurrentX;
            double y = this.CurrentY;
            while (this.argStack.Length > 0)
            {
                this.GlyphRenderer.CubicBezierTo(
                    new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())),
                    new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())),
                    new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())));
            }

            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void HH_CurveTo()
        {
            // |- dy1? {dxa dxb dyb dxc}+ hhcurveto (27) |-

            // appends one or more Bézier curves, as described by the
            // dxa...dxc set of arguments, to the current point.
            // For each curve, if there are 4 arguments,
            // the curve starts and ends horizontal.

            // The first curve need not start horizontal (the odd argument
            // case). Note the argument order for the odd argument case
            double x = this.CurrentX;
            double y = this.CurrentY;
            if (this.argStack.Length % 2 != 0)
            {
                y += this.argStack.Shift();
            }

            while (this.argStack.Length >= 4)
            {
                double c1x = x + this.argStack.Shift();
                double c1y = y;
                double c2x = c1x + this.argStack.Shift();
                double c2y = c1y + this.argStack.Shift();
                x = c2x + this.argStack.Shift();
                y = c2y;

                this.GlyphRenderer.CubicBezierTo(
                    new Vector2((float)c1x, (float)c1y),
                    new Vector2((float)c2x, (float)c2y),
                    new Vector2((float)x, (float)y));
            }

            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void HV_CurveTo()
        {
            // |- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf? hvcurveto (31) |-

            // |- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf? hvcurveto (31) |-

            // appends one or more Bézier curves to the current point.

            // The tangent for the first Bézier must be horizontal, and the second
            // must be vertical (except as noted below).

            // If there is a multiple of four arguments, the curve starts
            // horizontal and ends vertical.Note that the curves alternate
            // between start horizontal, end vertical, and start vertical, and
            // end horizontal.The last curve(the odd argument case) need not
            // end horizontal/ vertical.
            bool phase = true;
            double x = this.CurrentX;
            double y = this.CurrentY;
            double c1x;
            double c1y;
            double c2x;
            double c2y;
            while (this.argStack.Length >= 4)
            {
                if (phase)
                {
                    c1x = x + this.argStack.Shift();
                    c1y = y;
                    c2x = c1x + this.argStack.Shift();
                    c2y = c1y + this.argStack.Shift();
                    y = c2y + this.argStack.Shift();
                    x = c2x + (this.argStack.Length == 1 ? this.argStack.Shift() : 0);
                }
                else
                {
                    c1x = x;
                    c1y = y + this.argStack.Shift();
                    c2x = c1x + this.argStack.Shift();
                    c2y = c1y + this.argStack.Shift();
                    x = c2x + this.argStack.Shift();
                    y = c2y + (this.argStack.Length == 1 ? this.argStack.Shift() : 0);
                }

                this.GlyphRenderer.CubicBezierTo(
                    new Vector2((float)c1x, (float)c1y),
                    new Vector2((float)c2x, (float)c2y),
                    new Vector2((float)x, (float)y));

                phase = !phase;
            }

            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void R_CurveLine()
        {
            // |- { dxa dya dxb dyb dxc dyc} +dxd dyd rcurveline(24) |-
            // is equivalent to one rrcurveto for each set of six arguments
            // dxa...dyc, followed by exactly one rlineto using
            // the dxd, dyd arguments.

            // The number of curves is determined from the count
            // on the argument stack.
            double x = this.CurrentX;
            double y = this.CurrentY;

            while (this.argStack.Length >= 8)
            {
                this.GlyphRenderer.CubicBezierTo(
                    new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())),
                    new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())),
                    new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())));
            }

            this.GlyphRenderer.LineTo(new Vector2((float)(x += this.argStack.Shift()), (float)(y += this.argStack.Shift())));
            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void R_LineCurve()
        {
            // |- { dxa dya} +dxb dyb dxc dyc dxd dyd rlinecurve(25) |-

            // is equivalent to one rlineto for each pair of arguments beyond
            // the six arguments dxb...dyd needed for the one
            // rrcurveto command.The number of lines is determined from the count of
            // items on the argument stack.
            double x = this.CurrentX;
            double y = this.CurrentY;

            while (this.argStack.Length >= 8)
            {
                x += this.argStack.Shift();
                y += this.argStack.Shift();
                this.GlyphRenderer.LineTo(new Vector2((float)x, (float)y));
            }

            double c1x = x + this.argStack.Shift();
            double c1y = y + this.argStack.Shift();
            double c2x = c1x + this.argStack.Shift();
            double c2y = c1y + this.argStack.Shift();
            x = c2x + this.argStack.Shift();
            y = c2y + this.argStack.Shift();

            this.GlyphRenderer.CubicBezierTo(
                new Vector2((float)c1x, (float)c1y),
                new Vector2((float)c2x, (float)c2y),
                new Vector2((float)x, (float)y));

            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void VH_CurveTo()
        {
            // |- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf? vhcurveto (30) |-

            // |- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf? vhcurveto (30) |-

            // appends one or more Bézier curves to the current point, where
            // the first tangent is vertical and the second tangent is horizontal.

            // This command is the complement of
            // hvcurveto;

            // see the description of hvcurveto for more information.
            bool phase = false;
            double x = this.CurrentX;
            double y = this.CurrentY;
            double c1x;
            double c1y;
            double c2x;
            double c2y;
            while (this.argStack.Length >= 4)
            {
                if (phase)
                {
                    c1x = x + this.argStack.Shift();
                    c1y = y;
                    c2x = c1x + this.argStack.Shift();
                    c2y = c1y + this.argStack.Shift();
                    y = c2y + this.argStack.Shift();
                    x = c2x + (this.argStack.Length == 1 ? this.argStack.Shift() : 0);
                }
                else
                {
                    c1x = x;
                    c1y = y + this.argStack.Shift();
                    c2x = c1x + this.argStack.Shift();
                    c2y = c1y + this.argStack.Shift();
                    x = c2x + this.argStack.Shift();
                    y = c2y + (this.argStack.Length == 1 ? this.argStack.Shift() : 0);
                }

                this.GlyphRenderer.CubicBezierTo(new((float)c1x, (float)c1y), new((float)c2x, (float)c2y), new((float)x, (float)y));
                phase = !phase;
            }

            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void VV_CurveTo()
        {
            // |- dx1? {dya dxb dyb dyc}+  vvcurveto (26) |-
            // appends one or more curves to the current point.
            // If the argument count is a multiple of four, the curve starts and ends vertical.
            // If the argument count is odd, the first curve does not begin with a vertical tangent.
            double x = this.CurrentX;
            double y = this.CurrentY;
            if (this.argStack.Length % 2 != 0)
            {
                x += this.argStack.Shift();
            }

            while (this.argStack.Length >= 4)
            {
                double c1x = x;
                double c1y = y + this.argStack.Shift();
                double c2x = c1x + this.argStack.Shift();
                double c2y = c1y + this.argStack.Shift();
                x = c2x;
                y = c2y + this.argStack.Shift();

                this.GlyphRenderer.CubicBezierTo(new((float)c1x, (float)c1y), new((float)c2x, (float)c2y), new((float)x, (float)y));
            }

            this.CurrentX = x;
            this.CurrentY = y;
            this.argStack.Clear();
        }

        public void EndChar() => this.argStack.Clear();

        // |- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 dy6 fd flex (12 35) |-
        // causes two Bézier curves, as described by the arguments(as
        // shown in Figure 2 below), to be rendered as a straight line when
        // the flex depth is less than fd / 100 device pixels, and as curved lines
        // when the flex depth is greater than or equal to fd/ 100 device pixels
        public void Flex() => this.argStack.Clear(); // clear stack

        // |- dx1 dx2 dy2 dx3 dx4 dx5 dx6 hflex (12 34) |-
        // causes the two curves described by the arguments
        // dx1...dx6  to be rendered as a straight line when
        // the flex depth is less than 0.5(that is, fd is 50) device pixels,
        // and as curved lines when the flex depth is greater than or equal to 0.5 device pixels.

        // hflex is used when the following are all true:
        // a) the starting and ending points, first and last control points
        // have the same y value.
        // b) the joining point and the neighbor control points have
        // the same y value.
        // c) the flex depth is 50.
        public void H_Flex() => this.argStack.Clear(); // clear stack

        // |- dx1 dy1 dx2 dy2 dx3 dx4 dx5 dy5 dx6 hflex1 (12 36) |-

        // causes the two curves described by the arguments to be
        // rendered as a straight line when the flex depth is less than 0.5
        // device pixels, and as curved lines when the flex depth is greater
        // than or equal to 0.5 device pixels.

        // hflex1 is used if the conditions for hflex
        // are not met but all of the following are true:

        // a) the starting and ending points have the same y value,
        // b) the joining point and the neighbor control points have
        // the same y value.
        // c) the flex depth is 50.
        public void H_Flex1() => this.argStack.Clear(); // clear stack

        // |- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 d6 flex1 (12 37) |

        // causes the two curves described by the arguments to be
        // rendered as a straight line when the flex depth is less than 0.5
        // device pixels, and as curved lines when the flex depth is greater
        // than or equal to 0.5 device pixels.

        // The d6 argument will be either a dx or dy value, depending on
        // the curve(see Figure 3). To determine the correct value,
        // compute the distance from the starting point(x, y), the first
        // point of the first curve, to the last flex control point(dx5, dy5)
        // by summing all the arguments except d6; call this(dx, dy).If
        // abs(dx) > abs(dy), then the last point’s x-value is given by d6, and
        // its y - value is equal to y.
        //  Otherwise, the last point’s x-value is equal to x and its y-value is given by d6.
        public void Flex1() => this.argStack.Clear(); // clear stack

        //-------------------------------------------------------------------
        // 4.3 Hint Operators
        public void H_Stem()
        {
            // |- y dy {dya dyb}*  hstem (1) |-

            // hintCount += _currentIndex / 2;
            this.argStack.Clear(); // clear stack
        }

        public void V_Stem()
        {
            // |- x dx {dxa dxb}*  vstem (3) |-

            // hintCount += _currentIndex / 2;
            this.argStack.Clear(); // clear stack
        }

        public void V_StemHM()
        {
            // |- x dx {dxa dxb}* vstemhm (23) |-

            // hintCount += _currentIndex / 2;
            this.argStack.Clear(); // clear stack
        }

        public void H_StemHM()
        {
            // |- y dy {dya dyb}*  hstemhm (18) |-

            // hintCount += _currentIndex / 2;
            // has the same meaning as
            // hstem (1),
            // except that it must be used
            // in place of hstem  if the charstring contains one or more
            // hintmask operators.
            this.argStack.Clear(); // clear stack
        }

        //----------------------------------------
        // hintmask | -hintmask(19 + mask) | -
        // The mask data bytes are defined as follows:
        // • The number of data bytes is exactly the number needed, one
        // bit per hint, to reference the number of stem hints declared
        // at the beginning of the charstring program.
        // • Each bit of the mask, starting with the most-significant bit of
        // the first byte, represents the corresponding hint zone in the
        // order in which the hints were declared at the beginning of
        // the charstring.
        // • For each bit in the mask, a value of ‘1’ specifies that the
        // corresponding hint shall be active. A bit value of ‘0’ specifies
        // that the hint shall be inactive.
        // • Unused bits in the mask, if any, must be zero.
        public void HintMask1(int hintMaskValue)
        {
            // specifies which hints are active and which are not active. If any
            // hints overlap, hintmask must be used to establish a nonoverlapping
            // subset of hints.
            // hintmask may occur any number of
            // times in a charstring. Path operators occurring after a hintmask
            // are influenced by the new hint set, but the current point is not
            // moved. If stem hint zones overlap and are not properly
            // managed by use of the hintmask operator, the results are
            // undefined.

            // |- hintmask (19 + mask) |-
            this.argStack.Clear(); // clear stack
        }

        public void HintMask2(int hintMaskValue)
        {
            this.argStack.Clear(); // clear stack
        }

        public void HintMask3(int hintMaskValue)
        {
            this.argStack.Clear(); // clear stack
        }

        public void HintMask4(int hintMaskValue)
        {
            this.argStack.Clear(); // clear stack
        }

        public void HintMaskBits(int bitCount)
        {
            // calculate bytes need by
            // bytes need = (bitCount+7)/8
            this.argStack.Clear(); // clear stack
        }

        //----------------------------------------
        // |- cntrmask(20 + mask) |-

        // specifies the counter spaces to be controlled, and their relative
        // priority.The mask bits in the bytes, following the operator,
        // reference the stem hint declarations; the most significant bit of
        // the first byte refers to the first stem hint declared, through to
        // the last hint declaration.The counters to be controlled are
        // those that are delimited by the referenced stem hints.Bits set to
        // 1 in the first cntrmask command have top priority; subsequent
        // cntrmask commands specify lower priority counters(see Figure
        // 1 and the accompanying example).
        public void CounterSpaceMask1(int cntMaskValue) => this.argStack.Clear(); // clear stack

        public void CounterSpaceMask2(int cntMaskValue) => this.argStack.Clear(); // clear stack

        public void CounterSpaceMask3(int cntMaskValue) => this.argStack.Clear(); // clear stack

        public void CounterSpaceMask4(int cntMaskValue) => this.argStack.Clear(); // clear stack

        public void CounterSpaceMaskBits(int bitCount)
        {
            // calculate bytes need by
            // bytes need = (bitCount+7)/8

            this.argStack.Clear(); // clear stack
        }

        //----------------------------------------

        // 4.4: Arithmetic Operators

        // case Type2Operator2.abs:
        //                case Type2Operator2.add:
        //                case Type2Operator2.sub:
        //                case Type2Operator2.div:
        //                case Type2Operator2.neg:
        //                case Type2Operator2.random:
        //                case Type2Operator2.mul:
        //                case Type2Operator2.sqrt:
        //                case Type2Operator2.drop:
        //                case Type2Operator2.exch:
        //                case Type2Operator2.index:
        //                case Type2Operator2.roll:
        //                case Type2Operator2.dup:
        public void Op_Abs() => Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Abs));

        public void Op_Add() => Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Add));

        public void Op_Sub()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Sub));
        }

        public void Op_Div()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Div));
        }

        public void Op_Neg()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Neg));
        }

        public void Op_Random()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Random));
        }

        public void Op_Mul()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Mul));
        }

        public void Op_Sqrt()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Sqrt));
        }

        public void Op_Drop()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Drop));
        }

        public void Op_Exch()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Exch));
        }

        public void Op_Index()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Index));
        }

        public void Op_Roll()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Roll));
        }

        public void Op_Dup()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Dup));
        }

        //-------------------------
        // 4.5: Storage Operators

        // The storage operators utilize a transient array and provide
        // facilities for storing and retrieving transient array data.

        // The transient array provides non-persistent storage for
        // intermediate values.
        // There is no provision to initialize this array,
        // except explicitly using the put operator,
        // and values stored in the
        // array do not persist beyond the scope of rendering an individual
        // character.
        public void Put()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Put));
        }

        public void Get()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Get));
        }

        //-------------------------
        // 4.6: Conditional
        public void Op_And()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_And));
        }

        public void Op_Or()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Or));
        }

        public void Op_Not()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Not));
        }

        public void Op_Eq()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Eq));
        }

        public void Op_IfElse()
        {
            Debug.WriteLine("NOT_IMPLEMENT:" + nameof(Op_IfElse));
        }
    }

    /// <summary>
    /// A ref struct stack implementation that uses a pooled span to store the data.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    internal ref struct RefStack<T>
        where T : struct
    {
        private const int maxLength = 0X7FFFFFC7;
        private Buffer<T> buffer;
        private Span<T> stack;
        private bool isDisposed;
        private int size;
        private int currentIndex;

        public RefStack(int capacity)
        {
            if (capacity < 1)
            {
                capacity = 4;
            }

            this.buffer = new Buffer<T>(capacity);
            this.stack = this.buffer.GetSpan();
            this.isDisposed = false;
            this.size = 0;
            this.currentIndex = -1;
        }

        public int Length => this.size;

        /// <summary>
        /// Adds an item to the stack.
        /// </summary>
        /// <param name="value">The item to add.</param>
        public void Push(T value)
        {
            if ((uint)this.size < (uint)this.stack.Length)
            {
                this.size++;
                this.stack[++this.currentIndex] = value;
            }
            else
            {
                int capacity = this.size * 2;
                if ((uint)capacity > maxLength)
                {
                    capacity = maxLength;
                }

                var newBuffer = new Buffer<T>(capacity);
                var newStack = newBuffer.GetSpan();

                this.stack.CopyTo(newStack);
                this.buffer.Dispose();

                this.buffer = newBuffer;
                this.stack = newStack;

                this.size++;
                this.stack[++this.currentIndex] = value;
            }
        }

        /// <summary>
        /// Removes the first element of the stack.
        /// </summary>
        /// <returns>The <typeparamref name="T"/> element.</returns>
        public T Shift()
        {
            int newSize = this.size - 1;
            if (newSize < 0)
            {
                ThrowForEmptyStack();
            }

            int start = this.currentIndex - newSize;
            this.size = newSize;
            return this.stack[start];
        }

        /// <summary>
        /// Removes the last element of the stack.
        /// </summary>
        /// <returns>The <typeparamref name="T"/> element.</returns>
        public T Pop()
        {
            int newSize = this.size - 1;
            if (newSize < 0)
            {
                ThrowForEmptyStack();
            }

            this.size = newSize;
            return this.stack[this.currentIndex--];
        }

        /// <summary>
        /// Clears the current stack.
        /// </summary>
        public void Clear()
        {
            this.size = 0;
            this.currentIndex = 0;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.buffer.Dispose();
            this.isDisposed = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForEmptyStack() => throw new InvalidOperationException("Empty stack!");
    }
}
