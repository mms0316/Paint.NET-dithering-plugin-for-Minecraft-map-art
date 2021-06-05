// Title: Floyd-Steinberg Dithering Effect for Minecraft
// Author: mms0316 using code from BoltBait and ColorMinePortable
// Name: Floyd-Steinberg Dithering for Minecraft
// Submenu: Stylize
// Force Single Threaded
// Keywords: Floyd|Steinberg|Dithering|Dither|Error|Diffusion
// Desc: Dither selected pixels

/*
URL: http://www.BoltBait.com/pdn
URL: https://github.com/muak/ColorMinePortable

ColorMinePortable is licensed as:

The MIT License (MIT)

Copyright (c) 2013 ColorMine.org

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

#region UICode
ListBoxControl Amount1 = 0; // Color comparison method|CIE2000|CIE94|CIE76|CMC I:c|Weighted Euclidean|Euclidean
ListBoxControl Amount2 = 0; // Dithering method|Floyd-Steinberg (1/16)|Custom (1/32)

CheckboxControl AmountSand = true; // Palette: Sand
CheckboxControl AmountPrismarine = true; // Palette: Prismarine
CheckboxControl AmountTNT = true; // Palette: TNT
CheckboxControl AmountIce = true; // Palette: Ice
CheckboxControl AmountDirt = true; // Palette: Dirt
CheckboxControl AmountWhiteTerracotta = false; // Palette: White Terracotta
#endregion

byte PlusTruncate(byte a, int b)
{
    int c = a + b;
    if (c < 0) return 0;
    if (c > 255) return 255;
    return (byte)c;
}

private IColorSpaceComparison comparer;

Color FindNearestColor(Color color, IList<Color> palette)
{
    var colorRgb = new Rgb { R = color.R, G = color.G, B = color.B};
    double minDistance = Double.MaxValue;
    int bestIndex = 0;

    for (int i = 0; i < palette.Count; i++)
    {
        var colorOther = new Rgb { R = palette[i].R, G = palette[i].G, B = palette[i].B };
        double distance = comparer.Compare(colorRgb, colorOther);

        if (distance < minDistance)
        {
            minDistance = distance;
            bestIndex = i;
            if (minDistance <= Double.Epsilon) break;
        }
    }
    return palette[bestIndex];
}

// Setup for using pixel op
private UnaryPixelOps.Desaturate desaturateOp = new UnaryPixelOps.Desaturate();

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
    // Call the copy function
    dst.CopySurface(src,rect.Location,rect);

    switch (Amount1)
    {
        case 0:
        default:
            comparer = new CieDe2000Comparison();
            break;
        case 1:
            comparer = new Cie94Comparison();
            break;
        case 2:
            comparer = new Cie1976Comparison();
            break;
        case 3:
            comparer = new CmcComparison();
            break;
        case 4:
            comparer = new WeightedEuclidianComparison();
            break;
        case 5:
            comparer = new EuclidianComparison();
            break;
    }

    var palette = new List<Color>();

    if (AmountSand)
    {
        palette.Add(Color.FromArgb(174, 164, 115));
        palette.Add(Color.FromArgb(213, 201, 140));
        palette.Add(Color.FromArgb(247, 233, 163));
    }
    if (AmountPrismarine)
    {
        palette.Add(Color.FromArgb(64, 154, 150));
        palette.Add(Color.FromArgb(79, 188, 183));
        palette.Add(Color.FromArgb(92, 219, 213));
    }
    if (AmountTNT)
    {
        palette.Add(Color.FromArgb(180, 0, 0));
        palette.Add(Color.FromArgb(220, 0, 0));
        palette.Add(Color.FromArgb(255, 0, 0));
    }
    if (AmountIce)
    {
        palette.Add(Color.FromArgb(112, 112, 180));
        palette.Add(Color.FromArgb(138, 138, 220));
        palette.Add(Color.FromArgb(160, 160, 255));
    }
    if (AmountDirt)
    {
        palette.Add(Color.FromArgb(106, 76, 54));
        palette.Add(Color.FromArgb(130, 94, 66));
        palette.Add(Color.FromArgb(151, 109, 77));
    }
    if (AmountWhiteTerracotta)
    {
        palette.Add(Color.FromArgb(147, 124, 113));
        palette.Add(Color.FromArgb(180, 152, 138));
        palette.Add(Color.FromArgb(209, 177, 161));
    }

    palette.Add(Color.FromArgb(0xB4B4B4));
    palette.Add(Color.FromArgb(0xDCDCDC));
    palette.Add(Color.FromArgb(0xFFFFFF));
    palette.Add(Color.FromArgb(0x985924));
    palette.Add(Color.FromArgb(0xBA6D2C));
    palette.Add(Color.FromArgb(0xD87F33));
    palette.Add(Color.FromArgb(0x7D3598));
    palette.Add(Color.FromArgb(0x9941BA));
    palette.Add(Color.FromArgb(0xB24CD8));
    palette.Add(Color.FromArgb(0x486C98));
    palette.Add(Color.FromArgb(0x5884BA));
    palette.Add(Color.FromArgb(0x6699D8));
    palette.Add(Color.FromArgb(0xA1A124));
    palette.Add(Color.FromArgb(0xC5C52C));
    palette.Add(Color.FromArgb(0xE5E533));
    palette.Add(Color.FromArgb(0x599011));
    palette.Add(Color.FromArgb(0x6DB015));
    palette.Add(Color.FromArgb(0x7FCC19));
    palette.Add(Color.FromArgb(0xAA5974));
    palette.Add(Color.FromArgb(0xD06D8E));
    palette.Add(Color.FromArgb(0xF27FA5));
    palette.Add(Color.FromArgb(0x353535));
    palette.Add(Color.FromArgb(0x414141));
    palette.Add(Color.FromArgb(0x4C4C4C));
    palette.Add(Color.FromArgb(0x6C6C6C));
    palette.Add(Color.FromArgb(0x848484));
    palette.Add(Color.FromArgb(0x999999));
    palette.Add(Color.FromArgb(0x35596C));
    palette.Add(Color.FromArgb(0x416D84));
    palette.Add(Color.FromArgb(0x4C7F99));
    palette.Add(Color.FromArgb(0x592C7D));
    palette.Add(Color.FromArgb(0x6D3699));
    palette.Add(Color.FromArgb(0x7F3FB2));
    palette.Add(Color.FromArgb(0x24357D));
    palette.Add(Color.FromArgb(0x2C4199));
    palette.Add(Color.FromArgb(0x334CB2));
    palette.Add(Color.FromArgb(0x483524));
    palette.Add(Color.FromArgb(0x58412C));
    palette.Add(Color.FromArgb(0x664C33));
    palette.Add(Color.FromArgb(0x485924));
    palette.Add(Color.FromArgb(0x586D2C));
    palette.Add(Color.FromArgb(0x667F33));
    palette.Add(Color.FromArgb(0x6C2424));
    palette.Add(Color.FromArgb(0x842C2C));
    palette.Add(Color.FromArgb(0x993333));
    palette.Add(Color.FromArgb(0x111111));
    palette.Add(Color.FromArgb(0x151515));
    palette.Add(Color.FromArgb(0x191919));
    
    Color BestColor;
    ColorBgra BestColora;
    
    // Now in the main render loop, the dst canvas has a copy of the src canvas
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (Amount2==0) // Floyd-Steinberg Dithering
            {
                ColorBgra CurrentPixel = dst[x,y];
                byte A = CurrentPixel.A;
                Color currentPixel = CurrentPixel.ToColor();

                BestColor = FindNearestColor(currentPixel, palette);
                BestColora = ColorBgra.FromColor(BestColor);
                BestColora.A = A;

                // Floyd-Steinberg Dithering
                
                int errorR = currentPixel.R - BestColor.R;
                int errorG = currentPixel.G - BestColor.G;
                int errorB = currentPixel.B - BestColor.B;

                //  - * 7    where *=pixel being processed, -=previously processed pixel
                //  3 5 1    and pixel difference is distributed to neighbor pixels
                //           Note: 7+3+5+1=16 so we divide by 16 (>>4) before adding.

                if (x + 1 < rect.Right)
                {
                    dst[x + 1, y + 0] = ColorBgra.FromBgra(
                        PlusTruncate(dst[x + 1, y + 0].B, (errorB * 7) >> 4),
                        PlusTruncate(dst[x + 1, y + 0].G, (errorG * 7) >> 4),
                        PlusTruncate(dst[x + 1, y + 0].R, (errorR * 7) >> 4),
                        dst[x+1,y].A
                    );
                }
                if (y + 1 < rect.Bottom)
                {
                    if (x - 1 > rect.Left)
                    {
                        dst[x - 1, y + 1] = ColorBgra.FromBgra(
                            PlusTruncate(dst[x - 1, y + 1].B, (errorB * 3) >> 4),
                            PlusTruncate(dst[x - 1, y + 1].G, (errorG * 3) >> 4),
                            PlusTruncate(dst[x - 1, y + 1].R, (errorR * 3) >> 4),
                            dst[x-1,y+1].A
                        );
                    }
                    dst[x - 0, y + 1] = ColorBgra.FromBgra(
                        PlusTruncate(dst[x - 0, y + 1].B, (errorB * 5) >> 4),
                        PlusTruncate(dst[x - 0, y + 1].G, (errorG * 5) >> 4),
                        PlusTruncate(dst[x - 0, y + 1].R, (errorR * 5) >> 4),
                        dst[x-0,y+1].A
                    );
                    if (x + 1 < rect.Right)
                    {
                        dst[x + 1, y + 1] = ColorBgra.FromBgra(
                            PlusTruncate(dst[x + 1, y + 1].B, (errorB * 1) >> 4),
                            PlusTruncate(dst[x + 1, y + 1].G, (errorG * 1) >> 4),
                            PlusTruncate(dst[x + 1, y + 1].R, (errorR * 1) >> 4),
                            dst[x+1,y+1].A
                        );
                    }
                }
            }
            else // Custom Dithering
            {
                ColorBgra CurrentPixel = dst[x,y];
                byte A = CurrentPixel.A;
                Color currentPixel = CurrentPixel.ToColor();

                BestColor = FindNearestColor(currentPixel, palette);
                BestColora = ColorBgra.FromColor(BestColor);
                BestColora.A = A;

                // Custom 1/32 Dithering
                
                int errorR = currentPixel.R - BestColor.R;
                int errorG = currentPixel.G - BestColor.G;
                int errorB = currentPixel.B - BestColor.B;

                //  - - # 8 4    where *=pixel being processed, -=previously processed pixel
                //  0 4 8 4 1    and pixel difference is distributed to neighbor pixels
                //  0 0 2 1 0 

                if (x + 1 < rect.Right)
                {
                    dst[x + 1, y + 0] = ColorBgra.FromBgra(
                        PlusTruncate(dst[x + 1, y + 0].B, errorB >> 2),
                        PlusTruncate(dst[x + 1, y + 0].G, errorG >> 2),
                        PlusTruncate(dst[x + 1, y + 0].R, errorR >> 2),
                        dst[x+1,y].A
                    );
                }
                if (x + 2 < rect.Right)
                {
                    dst[x + 2, y + 0] = ColorBgra.FromBgra(
                        PlusTruncate(dst[x + 2, y + 0].B, errorB >> 3),
                        PlusTruncate(dst[x + 2, y + 0].G, errorG >> 3),
                        PlusTruncate(dst[x + 2, y + 0].R, errorR >> 3),
                        dst[x+2,y].A
                    );
                }
                if (y + 1 < rect.Bottom)
                {
                    if (x - 1 > rect.Left)
                    {
                        dst[x - 1, y + 1] = ColorBgra.FromBgra(
                            PlusTruncate(dst[x - 1, y + 1].B, errorB >> 3),
                            PlusTruncate(dst[x - 1, y + 1].G, errorG >> 3),
                            PlusTruncate(dst[x - 1, y + 1].R, errorR >> 3),
                            dst[x-1,y+1].A
                        );
                    }
                    dst[x, y + 1] = ColorBgra.FromBgra(
                        PlusTruncate(dst[x, y + 1].B, errorB >> 2),
                        PlusTruncate(dst[x, y + 1].G, errorG >> 2),
                        PlusTruncate(dst[x, y + 1].R, errorR >> 2),
                        dst[x,y+1].A
                    );
                    if (x + 1 < rect.Right)
                    {
                        dst[x + 1, y + 1] = ColorBgra.FromBgra(
                            PlusTruncate(dst[x + 1, y + 1].B, errorB >> 3),
                            PlusTruncate(dst[x + 1, y + 1].G, errorG >> 3),
                            PlusTruncate(dst[x + 1, y + 1].R, errorR >> 3),
                            dst[x+1,y+1].A
                        );
                    }
                    if (x + 2 < rect.Right)
                    {
                        dst[x + 2, y + 1] = ColorBgra.FromBgra(
                            PlusTruncate(dst[x + 2, y + 1].B, errorB >> 5),
                            PlusTruncate(dst[x + 2, y + 1].G, errorG >> 5),
                            PlusTruncate(dst[x + 2, y + 1].R, errorR >> 5),
                            dst[x+2,y+1].A
                        );
                    }
                }
                if (y + 2 < rect.Bottom)
                {
                    dst[x, y + 2] = ColorBgra.FromBgra(
                        PlusTruncate(dst[x, y + 2].B, errorB >> 4),
                        PlusTruncate(dst[x, y + 2].G, errorG >> 4),
                        PlusTruncate(dst[x, y + 2].R, errorR >> 4),
                        dst[x,y+2].A
                    );
                    if (x + 1 < rect.Right)
                    {
                        dst[x + 1, y + 2] = ColorBgra.FromBgra(
                            PlusTruncate(dst[x + 1, y + 2].B, errorB >> 5),
                            PlusTruncate(dst[x + 1, y + 2].G, errorG >> 5),
                            PlusTruncate(dst[x + 1, y + 2].R, errorR >> 5),
                            dst[x+1,y+2].A
                        );
                    }
                }
            }
            dst[x,y] = BestColora;
        }
    }
}

public interface IRgb : IColorSpace
{

    double R { get; set; }

    double G { get; set; }

    double B { get; set; }

}

internal static class RgbConverter
{
    internal static void ToColorSpace (IRgb color, IRgb item)
    {
        item.R = color.R;
        item.G = color.G;
        item.B = color.B;
    }

    internal static IRgb ToColor (IRgb item)
    {
        return item;
    }
}

public class Rgb : ColorSpace, IRgb
{

    public double R { get; set; }

    public double G { get; set; }

    public double B { get; set; }


    public override void Initialize(IRgb color)
    {
        RgbConverter.ToColorSpace(color,this);
    }

    public override IRgb ToRgb()
    {
        return RgbConverter.ToColor(this);
    }
}

/// <summary>
/// Defines how comparison methods may be called
/// </summary>
public interface IColorSpaceComparison
{
    /// <summary>
    /// Returns the difference between two colors given based on the specified defined in the called class.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Score based on similarity, the lower the score the closer the colors</returns>
    double Compare(IColorSpace a, IColorSpace b);
}

/// <summary>
/// Defines the public methods for all color spaces
/// </summary>
public interface IColorSpace
{
    /// <summary>
    /// Initialize settings from an Rgb object
    /// </summary>
    /// <param name="color"></param>
    void Initialize(IRgb color);

    /// <summary>
    /// Convert the color space to Rgb, you should probably using the "To" method instead. Need to figure out a way to "hide" or otherwise remove this method from the public interface.
    /// </summary>
    /// <returns></returns>
    IRgb ToRgb();

    /// <summary>
    /// Convert any IColorSpace to any other IColorSpace.
    /// </summary>
    /// <typeparam name="T">IColorSpace type to convert to</typeparam>
    /// <returns></returns>
    T To<T>() where T : IColorSpace, new();

    /// <summary>
    /// Determine how close two IColorSpaces are to each other using a passed in algorithm
    /// </summary>
    /// <param name="compareToValue">Other IColorSpace to compare to</param>
    /// <param name="comparer">Algorithm to use for comparison</param>
    /// <returns>Distance in 3d space as double</returns>
    double Compare(IColorSpace compareToValue, IColorSpaceComparison comparer);
}

/// <summary>
/// Abstract ColorSpace class, defines the To method that converts between any IColorSpace.
/// </summary>
public abstract class ColorSpace : IColorSpace
{
    public abstract void Initialize(IRgb color);
    public abstract IRgb ToRgb();

    /// <summary>
    /// Convienience method for comparing any IColorSpace
    /// </summary>
    /// <param name="compareToValue"></param>
    /// <param name="comparer"></param>
    /// <returns>Single number representing the difference between two colors</returns>
    public double Compare(IColorSpace compareToValue, IColorSpaceComparison comparer)
    {
        return comparer.Compare(this, compareToValue);
    }

    /// <summary>
    /// Convert any IColorSpace to any other IColorSpace
    /// </summary>
    /// <typeparam name="T">Must implement IColorSpace, new()</typeparam>
    /// <returns></returns>
    public T To<T>() where T : IColorSpace, new()
    {
        if (typeof(T) == GetType())
        {
            return (T)MemberwiseClone();
        }

        var newColorSpace = new T();
        newColorSpace.Initialize(ToRgb());

        return newColorSpace;
    }
}

public interface ILab : IColorSpace
{

    double L { get; set; }

    double A { get; set; }

    double B { get; set; }

}

public class Lab : ColorSpace, ILab
{

    public double L { get; set; }

    public double A { get; set; }

    public double B { get; set; }


    public override void Initialize(IRgb color)
    {
        LabConverter.ToColorSpace(color,this);
    }

    public override IRgb ToRgb()
    {
        return LabConverter.ToColor(this);
    }
}

internal static class LabConverter
{
    internal static void ToColorSpace(IRgb color, ILab item)
    {
        var xyz = new Xyz();
        xyz.Initialize(color);

        var white = XyzConverter.WhiteReference;
        var x = PivotXyz(xyz.X / white.X);
        var y = PivotXyz(xyz.Y / white.Y);
        var z = PivotXyz(xyz.Z / white.Z);

        item.L = Math.Max(0, 116 * y - 16);
        item.A = 500 * (x - y);
        item.B = 200 * (y - z);
    }

    internal static IRgb ToColor(ILab item)
    {
        var y = (item.L + 16.0) / 116.0;
        var x = item.A / 500.0 + y;
        var z = y - item.B / 200.0;

        var white = XyzConverter.WhiteReference;
        var x3 = x * x * x;
        var z3 = z * z * z;
        var xyz = new Xyz
            {
                X = white.X * (x3 > XyzConverter.Epsilon ? x3 : (x - 16.0 / 116.0) / 7.787),
                Y = white.Y * (item.L > (XyzConverter.Kappa * XyzConverter.Epsilon) ? Math.Pow(((item.L + 16.0) / 116.0), 3) : item.L / XyzConverter.Kappa),
                Z = white.Z * (z3 > XyzConverter.Epsilon ? z3 : (z - 16.0 / 116.0) / 7.787)
            };

        return xyz.ToRgb();
    }

    private static double PivotXyz(double n)
    {
        return n > XyzConverter.Epsilon ? CubicRoot(n) : (XyzConverter.Kappa * n + 16) / 116;
    }

    private static double CubicRoot(double n)
    {
        return Math.Pow(n, 1.0 / 3.0);
    }
}

public interface IXyz : IColorSpace
{

    double X { get; set; }

    double Y { get; set; }

    double Z { get; set; }

}

public class Xyz : ColorSpace, IXyz
{

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }


    public override void Initialize(IRgb color)
    {
        XyzConverter.ToColorSpace(color,this);
    }

    public override IRgb ToRgb()
    {
        return XyzConverter.ToColor(this);
    }
}


internal static class XyzConverter
{
    #region Constants/Helper methods for Xyz related spaces
    internal static IXyz WhiteReference { get; private set; } // TODO: Hard-coded!
    internal const double Epsilon = 0.008856; // Intent is 216/24389
    internal const double Kappa = 903.3; // Intent is 24389/27
    static XyzConverter()
    {
        WhiteReference = new Xyz
        {
            X = 95.047,
            Y = 100.000,
            Z = 108.883
        };
    }

    internal static double CubicRoot(double n)
    {
        return Math.Pow(n, 1.0 / 3.0);
    }
    #endregion

    internal static void ToColorSpace(IRgb color, IXyz item)
    {
        var r = PivotRgb(color.R / 255.0);
        var g = PivotRgb(color.G / 255.0);
        var b = PivotRgb(color.B / 255.0);

        // Observer. = 2°, Illuminant = D65
        item.X = r * 0.4124 + g * 0.3576 + b * 0.1805;
        item.Y = r * 0.2126 + g * 0.7152 + b * 0.0722;
        item.Z = r * 0.0193 + g * 0.1192 + b * 0.9505;
    }

    internal static IRgb ToColor(IXyz item)
    {
        // (Observer = 2°, Illuminant = D65)
        var x = item.X / 100.0;
        var y = item.Y / 100.0;
        var z = item.Z / 100.0;

        var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
        var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
        var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

        r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
        g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
        b = b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

        return new Rgb
        {
            R = ToRgb(r),
            G = ToRgb(g),
            B = ToRgb(b)
        };
    }

    private static double ToRgb(double n)
    {
        var result = 255.0 * n;
        if (result < 0) return 0;
        if (result > 255) return 255;
        return result;
    }

    private static double PivotRgb(double n)
    {
        return (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
    }
}
/// <summary>
/// Implements the DE2000 method of delta-e: http://en.wikipedia.org/wiki/Color_difference#CIEDE2000
/// Correct implementation provided courtesy of Jonathan Hofinger, jaytar42
/// </summary>
public class CieDe2000Comparison : IColorSpaceComparison
{
    /// <summary>
    /// Calculates the DE2000 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIEDE2000
    /// Correct implementation provided courtesy of Jonathan Hofinger, jaytar42
    /// </summary>
    public double Compare(IColorSpace c1, IColorSpace c2)
    {
        //Set weighting factors to 1
        double k_L = 1.0d;
        double k_C = 1.0d;
        double k_H = 1.0d;


        //Change Color Space to L*a*b:
        Lab lab1 = c1.To<Lab>();
        Lab lab2 = c2.To<Lab>();

        //Calculate Cprime1, Cprime2, Cabbar
        double c_star_1_ab = Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
        double c_star_2_ab = Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
        double c_star_average_ab = (c_star_1_ab + c_star_2_ab) / 2;

        double c_star_average_ab_pot7 = c_star_average_ab * c_star_average_ab * c_star_average_ab;
        c_star_average_ab_pot7 *= c_star_average_ab_pot7 * c_star_average_ab;

        double G = 0.5d * (1 - Math.Sqrt(c_star_average_ab_pot7 / (c_star_average_ab_pot7 + 6103515625))); //25^7
        double a1_prime = (1 + G) * lab1.A;
        double a2_prime = (1 + G) * lab2.A;

        double C_prime_1 = Math.Sqrt(a1_prime * a1_prime + lab1.B * lab1.B);
        double C_prime_2 = Math.Sqrt(a2_prime * a2_prime + lab2.B * lab2.B);
        //Angles in Degree.
        double h_prime_1 = ((Math.Atan2(lab1.B, a1_prime) * 180d / Math.PI) + 360) % 360d;
        double h_prime_2 = ((Math.Atan2(lab2.B, a2_prime) * 180d / Math.PI) + 360) % 360d;

        double delta_L_prime = lab2.L - lab1.L;
        double delta_C_prime = C_prime_2 - C_prime_1;

        double h_bar = Math.Abs(h_prime_1 - h_prime_2);
        double delta_h_prime;
        if (C_prime_1 * C_prime_2 == 0) delta_h_prime = 0;
        else
        {
            if (h_bar <= 180d)
            {
                delta_h_prime = h_prime_2 - h_prime_1;
            }
            else if (h_bar > 180d && h_prime_2 <= h_prime_1)
            {
                delta_h_prime = h_prime_2 - h_prime_1 + 360.0;
            }
            else
            {
                delta_h_prime = h_prime_2 - h_prime_1 - 360.0;
            }
        }
        double delta_H_prime = 2 * Math.Sqrt(C_prime_1 * C_prime_2) * Math.Sin(delta_h_prime * Math.PI / 360d);

        // Calculate CIEDE2000
        double L_prime_average = (lab1.L + lab2.L) / 2d;
        double C_prime_average = (C_prime_1 + C_prime_2) / 2d;

        //Calculate h_prime_average

        double h_prime_average;
        if (C_prime_1 * C_prime_2 == 0) h_prime_average = 0;
        else
        {
            if (h_bar <= 180d)
            {
                h_prime_average = (h_prime_1 + h_prime_2) / 2;
            }
            else if (h_bar > 180d && (h_prime_1 + h_prime_2) < 360d)
            {
                h_prime_average = (h_prime_1 + h_prime_2 + 360d) / 2;
            }
            else
            {
                h_prime_average = (h_prime_1 + h_prime_2 - 360d) / 2;
            }
        }
        double L_prime_average_minus_50_square = (L_prime_average - 50);
        L_prime_average_minus_50_square *= L_prime_average_minus_50_square;

        double S_L = 1 + ((.015d * L_prime_average_minus_50_square) / Math.Sqrt(20 + L_prime_average_minus_50_square));
        double S_C = 1 + .045d * C_prime_average;
        double T = 1
            - .17 * Math.Cos(DegToRad(h_prime_average - 30))
            + .24 * Math.Cos(DegToRad(h_prime_average * 2))
            + .32 * Math.Cos(DegToRad(h_prime_average * 3 + 6))
            - .2 * Math.Cos(DegToRad(h_prime_average * 4 - 63));
        double S_H = 1 + .015 * T * C_prime_average;
        double h_prime_average_minus_275_div_25_square = (h_prime_average - 275) / (25);
        h_prime_average_minus_275_div_25_square *= h_prime_average_minus_275_div_25_square;
        double delta_theta = 30 * Math.Exp(-h_prime_average_minus_275_div_25_square);

        double C_prime_average_pot_7 = C_prime_average * C_prime_average * C_prime_average;
        C_prime_average_pot_7 *= C_prime_average_pot_7 * C_prime_average;
        double R_C = 2 * Math.Sqrt(C_prime_average_pot_7 / (C_prime_average_pot_7 + 6103515625));

        double R_T = -Math.Sin(DegToRad(2 * delta_theta)) * R_C;

        double delta_L_prime_div_k_L_S_L = delta_L_prime / (S_L * k_L);
        double delta_C_prime_div_k_C_S_C = delta_C_prime / (S_C * k_C);
        double delta_H_prime_div_k_H_S_H = delta_H_prime / (S_H * k_H);

        double CIEDE2000 = (
            delta_L_prime_div_k_L_S_L * delta_L_prime_div_k_L_S_L
            + delta_C_prime_div_k_C_S_C * delta_C_prime_div_k_C_S_C
            + delta_H_prime_div_k_H_S_H * delta_H_prime_div_k_H_S_H
            + R_T * delta_C_prime_div_k_C_S_C * delta_H_prime_div_k_H_S_H
            );

        return CIEDE2000;
    }
    private double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
/// <summary>
/// Implements the Cie94 method of delta-e: http://en.wikipedia.org/wiki/Color_difference#CIE94
/// </summary>
public class Cie94Comparison : IColorSpaceComparison
{
    /// <summary>
    /// Application type defines constants used in the Cie94 comparison
    /// </summary>
    public enum Application
    {
        GraphicArts,
        Textiles
    }


    internal ApplicationConstants Constants { get; private set; }

    /// <summary>
    /// Create new Cie94Comparison. Defaults to GraphicArts application type.
    /// </summary>
    public Cie94Comparison()
    {
        Constants = new ApplicationConstants(Application.GraphicArts);
    }

    /// <summary>
    /// Create new Cie94Comparison for specific application type.
    /// </summary>
    /// <param name="application"></param>
    public Cie94Comparison(Application application)
    {
        Constants = new ApplicationConstants(application);
    }

    /// <summary>
    /// Compare colors using the Cie94 algorithm. The first color (a) will be used as the reference color.
    /// </summary>
    /// <param name="a">Reference color</param>
    /// <param name="b">Comparison color</param>
    /// <returns></returns>
    public double Compare(IColorSpace a, IColorSpace b)
    {
        var labA = a.To<Lab>();
        var labB = b.To<Lab>();

        var deltaL = labA.L - labB.L;
        var deltaA = labA.A - labB.A;
        var deltaB = labA.B - labB.B;

        var c1 = Math.Sqrt(labA.A * labA.A + labA.B * labA.B);
        var c2 = Math.Sqrt(labB.A * labB.A + labB.B * labB.B);
        var deltaC = c1 - c2;

        var deltaH = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
        deltaH = deltaH < 0 ? 0 : Math.Sqrt(deltaH);

        const double sl = 1.0;
        const double kc = 1.0;
        const double kh = 1.0;

        var sc = 1.0 + Constants.K1 * c1;
        var sh = 1.0 + Constants.K2 * c1;

        var deltaLKlsl = deltaL / (Constants.Kl * sl);
        var deltaCkcsc = deltaC / (kc * sc);
        var deltaHkhsh = deltaH / (kh * sh);
        var i = deltaLKlsl * deltaLKlsl + deltaCkcsc * deltaCkcsc + deltaHkhsh * deltaHkhsh;
        return i < 0 ? 0 : i;
    }

    internal class ApplicationConstants
    {
        internal double Kl { get; private set; }
        internal double K1 { get; private set; }
        internal double K2 { get; private set; }

        public ApplicationConstants(Application application)
        {
            switch (application)
            {
                case Application.GraphicArts:
                    Kl = 1.0;
                    K1 = .045;
                    K2 = .015;
                    break;
                case Application.Textiles:
                    Kl = 2.0;
                    K1 = .048;
                    K2 = .014;
                    break;
            }
        }
    }
}

/// <summary>
/// Implements the CIE76 method of delta-e: http://en.wikipedia.org/wiki/Color_difference#CIE76
/// </summary>
public class Cie1976Comparison : IColorSpaceComparison
{
    /// <summary>
    /// Calculates the CIE76 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIE76
    /// </summary>
    public double Compare(IColorSpace colorA, IColorSpace colorB)
    {
        var a = colorA.To<Lab>();
        var b = colorB.To<Lab>();

        var differences = Distance(a.L, b.L) + Distance(a.A, b.A) + Distance(a.B, b.B);
        return differences;
    }

    private static double Distance(double a, double b)
    {
        return (a - b) * (a - b);
    }
}

/// <summary>
/// Implements the CMC l:c (1984) method of delta-e: http://en.wikipedia.org/wiki/Color_difference#CMC_l:c_.281984.29
/// </summary>
public class CmcComparison : IColorSpaceComparison
{
    public const double DefaultLightness = 2.0;
    public const double DefaultChroma = 1.0;

    private readonly double _lightness;
    private readonly double _chroma;

    /// <summary>
    /// Create CMC l:c comparison with DefaultLightness and DefaultChroma values.
    /// </summary>
    public CmcComparison()
    {
        _lightness = DefaultLightness;
        _chroma = DefaultChroma;
    }

    /// <summary>
    /// Create CMC l:c comparison with specific lightness (l) and chroma (c) values.
    /// </summary>
    /// <param name="lightness"></param>
    /// <param name="chroma"></param>
    public CmcComparison(double lightness = DefaultLightness, double chroma = DefaultChroma)
    {
        _lightness = lightness;
        _chroma = chroma;
    }

    /// <summary>
    /// Calculates the CMC l:c (1984) delta-e value: http://en.wikipedia.org/wiki/Color_difference#CMC_l:c_.281984.29
    /// </summary>
    /// <param name="colorA"></param>
    /// <param name="colorB"></param>
    /// <returns></returns>
    public double Compare(IColorSpace colorA, IColorSpace colorB)
    {
        var aLab = colorA.To<Lab>();
        var bLab = colorB.To<Lab>();

        var deltaL = aLab.L - bLab.L;
        var h = Math.Atan2(aLab.B, aLab.A);

        var c1 = Math.Sqrt(aLab.A * aLab.A + aLab.B * aLab.B);
        var c2 = Math.Sqrt(bLab.A * bLab.A + bLab.B * bLab.B);
        var deltaC = c1 - c2;

        var deltaH = Math.Sqrt(
            (aLab.A - bLab.A) * (aLab.A - bLab.A) +
            (aLab.B - bLab.B) * (aLab.B - bLab.B) - 
            deltaC * deltaC);

        var c1_4 = c1 * c1;
        c1_4 *= c1_4;
        var t = 164 <= h && h <= 345
                    ? .56 + Math.Abs(.2 * Math.Cos(h + 168.0))
                    : .36 + Math.Abs(.4 * Math.Cos(h + 35.0));
        var f = Math.Sqrt(c1_4 / (c1_4 + 1900.0));

        var sL = aLab.L < 16 ? .511 : (.040975 * aLab.L) / (1.0 + .01765 * aLab.L);
        var sC = (.0638 * c1) / (1 + .0131 * c1) + .638;
        var sH = sC * (f * t + 1 - f);

        var differences = DistanceDivided(deltaL, _lightness * sL) +
                          DistanceDivided(deltaC, _chroma * sC) +
                          DistanceDivided(deltaH, sH);

        return differences;
    }

    private static double DistanceDivided(double a, double dividend)
    {
        var adiv = a / dividend;
        return adiv * adiv;
    }
}


public class WeightedEuclidianComparison : IColorSpaceComparison
{
    /// <summary>
    /// Calculates the CIE76 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIE76
    /// </summary>
    public double Compare(IColorSpace colorA, IColorSpace colorB)
    {
        var a = colorA.To<Rgb>();
        var b = colorB.To<Rgb>();

        return Distance(a.R, b.R) * 0.09 + Distance(a.G, b.G) * 0.3481 + Distance(a.B, b.B) * 0.0121;
    }

    private static double Distance(double a, double b)
    {
        return (a - b) * (a - b);
    }
}

public class EuclidianComparison : IColorSpaceComparison
{
    /// <summary>
    /// Calculates the CIE76 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIE76
    /// </summary>
    public double Compare(IColorSpace colorA, IColorSpace colorB)
    {
        var a = colorA.To<Rgb>();
        var b = colorB.To<Rgb>();

        return Distance(a.R, b.R) + Distance(a.G, b.G) + Distance(a.B, b.B);
    }

    private static double Distance(double a, double b)
    {
        return (a - b) * (a - b);
    }
}