// Title: Dithering Effect for Minecraft
// Author: mms0316 using code from BoltBait and ColorMinePortable
// Name: Dithering for Minecraft
// Submenu: Stylize
// Force Single Threaded
// Keywords: Dithering|Dither|Error|Diffusion
// Desc: Dither selected pixels
// URL: https://github.com/mms0316/Paint.NET-dithering-plugin-for-Minecraft-map-art

/*
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

https://github.com/muak/ColorMinePortable

Dithering algorithms from
https://forums.getpaint.net/topic/29428-floyd-steinberg-dithering-including-source/
https://web.archive.org/web/20070927122512/http://www.efg2.com/Lab/Library/ImageProcessing/DHALF.TXT

Worth checking:
https://shihn.ca/posts/2020/dithering/
https://github.com/redstonehelper/MapConverter/blob/main/MapConverter.java
https://github.com/rebane2001/mapartcraft/blob/master/src/components/mapart/workers/mapCanvas.jsworker

v9
*/

#region UICode
ListBoxControl InputColorMethod = 4; // Color comparison method|Weighted Euclidean|Euclidean|CIE2000|CIE94|CIE76|CMC I:c
ListBoxControl InputDitheringMethod = 0; // Dithering method|Floyd-Steinberg (1/16)|None (Approximate colors)|Jarvis-Judice-Ninke (1/48)|Burkes (1/32)|Sierra-2-4A (1/4)|Sierra2 (1/16)|Sierra3 (1/32)|Stucki (1/42)|Custom (1/32)
ListBoxControl InputErrorCalcMethod = 0; // Dithering error calculation method|RGB|LAB
ListBoxControl InputErrorRoundingMethod = 0; // Dithering rounding method|Nearest|Floor|Ceiling
CheckboxControl InputPalette3d = true; // Palette: 3D colors
CheckboxControl InputPaletteSkinTweak = false; // {InputPalette3d} Palette: Tweak for skin
CheckboxControl InputPaletteSandDark = true; // {InputPaletteSkinTweak} Sand (dark)
CheckboxControl InputPaletteSandMedium = true; // {InputPaletteSkinTweak} Sand (medium)
CheckboxControl InputPaletteSandLight = true; // {InputPaletteSkinTweak} Sand (light)
CheckboxControl InputPaletteWhiteDark = false; // {InputPaletteSkinTweak} White (dark)
CheckboxControl InputPaletteWhiteMedium = false; // {InputPaletteSkinTweak} White (medium)
CheckboxControl InputPaletteWhiteLight = true; // {InputPaletteSkinTweak} White (light)
CheckboxControl InputPalettePinkDark = true; // {InputPaletteSkinTweak} Pink (dark)
CheckboxControl InputPalettePinkMedium = true; // {InputPaletteSkinTweak} Pink (medium)
CheckboxControl InputPalettePinkLight = true; // {InputPaletteSkinTweak} Pink (light)
CheckboxControl InputPaletteOrangeDark = true; // {InputPaletteSkinTweak} Orange (dark)
CheckboxControl InputPaletteOrangeMedium = true; // {InputPaletteSkinTweak} Orange (medium)
CheckboxControl InputPaletteOrangeLight = true; // {InputPaletteSkinTweak} Orange (light)
CheckboxControl InputPalettePrismarine = true; // {!InputPaletteSkinTweak} Prismarine
CheckboxControl InputPaletteTNT = true; // {!InputPaletteSkinTweak} TNT
CheckboxControl InputPaletteIce = true; // {!InputPaletteSkinTweak} Ice
CheckboxControl InputPaletteDirt = true; // {!InputPaletteSkinTweak} Dirt
CheckboxControl InputPaletteTerracotta = true; // {!InputPaletteSkinTweak} Terracotta (all colors)
CheckboxControl InputPaletteWhiteTerracottaDark = true; // {InputPaletteSkinTweak} White Terracotta (dark)
CheckboxControl InputPaletteWhiteTerracottaMedium = true; // {InputPaletteSkinTweak} White Terracotta (medium)
CheckboxControl InputPaletteWhiteTerracottaLight = true; // {InputPaletteSkinTweak} White Terracotta (light)
CheckboxControl InputPaletteLapis = false; // {!InputPaletteSkinTweak} Lapis Lazuli
CheckboxControl InputPaletteMushroomStem = false; // {!InputPaletteSkinTweak} Mushroom Stem
CheckboxControl InputPaletteMushroomStemDark = false; // {InputPaletteSkinTweak} Mushroom Stem (dark)
CheckboxControl InputPaletteMushroomStemMedium = false; // {InputPaletteSkinTweak} Mushroom Stem (medium)
CheckboxControl InputPaletteMushroomStemLight = false; // {InputPaletteSkinTweak} Mushroom Stem (light)

IntSliderControl InputHue = 0; // [-180,180,2] Hue
IntSliderControl InputSaturation = 100; // [0,400,3] Saturation
IntSliderControl InputLightness = 0; // [-100,100,5] Lightness
CheckboxControl InputFixChunk = true; // Apply fix for plugins' 128x128 subchunk limitation
#endregion

enum ColorMethod
{
    WeightedEuclidean = 0,
    Euclidean,
    CIE2000,
    CIE94,
    CIE76,
    CMCIC
}

enum DitheringMethod
{
    FloydSteinberg = 0,
    None,
    JarvisJudiceNinke,
    Burkes,
    Sierra2_4A,
    Sierra2,
    Sierra3,
    Stucki,
    Custom_1_32,
}

enum ErrorCalcMethod
{
    RGB = 0,
    LAB
}

enum ErrorRoundingMethod
{
    Nearest = 0,
    Floor,
    Ceiling
}

IColorSpaceComparison comparer = null;
List<IColorSpace> palette = null;
bool skipNextRenders = false;
Dictionary<IColorSpace, IColorSpace> cache = null;

void PreRender(Surface dst, Surface src)
{
    skipNextRenders = false;

    switch ((ColorMethod)InputColorMethod)
    {
        case ColorMethod.WeightedEuclidean:
        default:
            comparer = new WeightedEuclidianComparison();
            break;
        case ColorMethod.Euclidean:
            comparer = new EuclidianComparison();
            break;
        case ColorMethod.CIE2000:
            comparer = new CieDe2000Comparison();
            break;
        case ColorMethod.CIE94:
            comparer = new Cie94Comparison();
            break;
        case ColorMethod.CIE76:
            comparer = new Cie1976Comparison();
            break;
        case ColorMethod.CMCIC:
            comparer = new CmcComparison();
            break;
    }

    if (palette == null)
        palette = new List<IColorSpace>();
    else
        palette.Clear();

    if (InputPaletteSkinTweak)
    {
        if (InputPaletteSandDark)
            palette.Add(new Rgb { R = 174, G = 164, B = 115 });
        if (InputPaletteSandMedium)
            palette.Add(new Rgb { R = 213, G = 201, B = 140 });
        if (InputPaletteSandLight)
            palette.Add(new Rgb { R = 247, G = 233, B = 163 });

        if (InputPaletteWhiteDark)
            palette.Add(new Rgb { R = 180, G = 180, B = 180 });
        if (InputPaletteWhiteMedium)
            palette.Add(new Rgb { R = 220, G = 220, B = 220 });
        if (InputPaletteWhiteLight)
            palette.Add(new Rgb { R = 255, G = 255, B = 255 });

        if (InputPalettePinkDark)
            palette.Add(new Rgb { R = 170, G = 89, B = 116 });
        if (InputPalettePinkMedium)
            palette.Add(new Rgb { R = 208, G = 109, B = 142 });
        if (InputPalettePinkLight)
            palette.Add(new Rgb { R = 242, G = 127, B = 165 });

        if (InputPaletteOrangeDark)
            palette.Add(new Rgb { R = 152, G = 89, B = 36 });
        if (InputPaletteOrangeMedium)
            palette.Add(new Rgb { R = 186, G = 109, B = 44 });
        if (InputPaletteOrangeLight)
            palette.Add(new Rgb { R = 216, G = 127, B = 51 });

        if (InputPaletteWhiteTerracottaDark)
            palette.Add(new Rgb { R = 147, G = 124, B = 113 });
        if (InputPaletteWhiteTerracottaMedium)
            palette.Add(new Rgb { R = 180, G = 152, B = 138 });
        if (InputPaletteWhiteTerracottaLight)
            palette.Add(new Rgb { R = 209, G = 177, B = 161 });

        if (InputPaletteMushroomStemDark)
            palette.Add(new Rgb { R = 140, G = 140, B = 140 });
        if (InputPaletteMushroomStemMedium)
            palette.Add(new Rgb { R = 171, G = 171, B = 171 });
        if (InputPaletteMushroomStemLight)
            palette.Add(new Rgb { R = 199, G = 199, B = 199 });
    }
    else
    {
        //Grass/Slime
        AddPalette(palette, 127, 178, 56);

        //Sand
        AddPalette(palette, 247, 233, 163);

        if (InputPaletteMushroomStem)
            AddPalette(palette, 199, 199, 199);

        if (InputPaletteTNT)
            AddPalette(palette, 255, 0, 0);

        if (InputPaletteIce)
            AddPalette(palette, 160, 160, 255);

        //Iron
        AddPalette(palette, 167, 167, 167);
        
        //Clay
        AddPalette(palette, 164, 168, 184);

        if (InputPaletteDirt)
            AddPalette(palette, 151, 109, 77);

        //Stone
        AddPalette(palette, 112, 112, 112);

        //Water
        //

        //Oak
        AddPalette(palette, 143, 119, 72);

        //Diorite/Quartz
        //

        //Wool
        AddPalette(palette, 255, 255, 255);
        AddPalette(palette, 216, 127, 51);
        AddPalette(palette, 178, 76, 216);
        AddPalette(palette, 102, 153, 216);
        AddPalette(palette, 229, 229, 51);
        AddPalette(palette, 127, 204, 25);
        AddPalette(palette, 242, 127, 165);
        AddPalette(palette, 76, 76, 76);
        AddPalette(palette, 153, 153, 153);
        AddPalette(palette, 76, 127, 153);
        AddPalette(palette, 127, 63, 178);
        AddPalette(palette, 51, 76, 178);
        AddPalette(palette, 102, 76, 51);
        AddPalette(palette, 102, 127, 51);
        AddPalette(palette, 153, 51, 51);
        AddPalette(palette, 25, 25, 25);

        //Gold
        //

        if (InputPalettePrismarine)
            AddPalette(palette, 92, 219, 213);

        if (InputPaletteLapis)
            AddPalette(palette, 74, 128, 255);

        //Emerald
        //

        //Spruce
        AddPalette(palette, 129, 86, 49);

        //Netherrack
        AddPalette(palette, 112, 2, 0);

        if (InputPaletteTerracotta)
        {
            AddPalette(palette, 209, 177, 161);
            AddPalette(palette, 159, 82, 36);
            AddPalette(palette, 149, 87, 108);
            AddPalette(palette, 112, 108, 138);
            AddPalette(palette, 186, 133, 36);
            AddPalette(palette, 103, 117, 53);
            AddPalette(palette, 160, 77, 78);
            AddPalette(palette, 57, 41, 35);
            AddPalette(palette, 135, 107, 98);
            AddPalette(palette, 87, 92, 92);
            AddPalette(palette, 122, 73, 88);
            AddPalette(palette, 76, 62, 92);
            AddPalette(palette, 76, 50, 35);
            AddPalette(palette, 76, 82, 42);
            AddPalette(palette, 142, 60, 46);
            AddPalette(palette, 37, 22, 16);
        }

        //Crimson Nylium
        //

        //Crimson Slab
        AddPalette(palette, 148, 63, 97);

        //Crimson Hyphae
        AddPalette(palette, 92, 25, 29);

        //Warped Nylium
        //

        //Warped Slab
        AddPalette(palette, 58, 142, 140);

        //Warped Hyphae
        AddPalette(palette, 86, 44, 62);

        //Warped Wart Block
        //

        //Deepslate
        //

        //Block of Raw Iron
        //

        //Glow Lichen
        //
    }

    if (cache == null)
        cache = new Dictionary <IColorSpace, IColorSpace>();
    else
        cache.Clear();
}

void AddPalette(IList<IColorSpace> palette, int R, int G, int B)
{
    if (InputPalette3d)
        palette.Add(new Rgb { R = R * 180 / 255, G = G * 180 / 255, B = B * 180 / 255 });
    palette.Add(new Rgb { R = R * 220 / 255, G = G * 220 / 255, B = B * 220 / 255 });
    if (InputPalette3d)
        palette.Add(new Rgb { R = R, G = G, B = B });
}

protected override void OnDispose(bool disposing)
{
    comparer = null;

    if (palette != null)
        palette.Clear();
    palette = null;

    base.OnDispose(disposing);
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
    if (skipNextRenders) return;

    // Preprocessing: Hue, Saturation Lightness
    if (InputHue != 0 || InputSaturation != 100 || InputLightness != 0)
    {
        UnaryPixelOp pixelOp = new UnaryPixelOps.HueSaturationLightness(InputHue, InputSaturation, InputLightness);
        pixelOp.Apply(dst, src, src.Bounds);
    }
    else
        dst.CopySurface(src, src.Bounds);

    DitheringMethod ditheringMethod = (DitheringMethod)InputDitheringMethod;

    // Paint.NET plugin system subdivides chunks in 128x128 areas, which may interfere with dithering
    // If fix is enabled, the first call to Render() will work on all chunks and subsequent calls to Render() will do nothing
    if (InputFixChunk && ditheringMethod != DitheringMethod.None)
    {
        if (rect.X % 128 == 0 &&
            rect.Y % 128 == 0 &&
            rect.Width % 128 == 0 &&
            rect.Height % 128 == 0 && (
                src.Bounds.Width != rect.Width ||
                src.Bounds.Height != rect.Height
            ))
        {
            rect = new Rectangle(src.Bounds.Location, src.Size);
            skipNextRenders = true;
        }
    }

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            ColorBgra currentPixel = dst[x, y];
            IColorSpace currentColor = new Rgb { R = currentPixel.R, G = currentPixel.G, B = currentPixel.B };
            IColorSpace bestColor = FindNearestColor(currentColor, palette, comparer, cache);

            Rgb bestRgb = bestColor.To<Rgb>();
            dst[x, y] = ColorBgra.FromBgra((byte)bestRgb.B, (byte)bestRgb.G, (byte)bestRgb.R, currentPixel.A);

            if (ditheringMethod != DitheringMethod.None)
            {
                ErrorCalcMethod errorCalcMethod = (ErrorCalcMethod)InputErrorCalcMethod;
                double error1, error2, error3; //RGB or L*a*b*
                int div;

                if (errorCalcMethod == ErrorCalcMethod.LAB)
                {
                    Lab currentLab = currentColor.To<Lab>();
                    Lab bestLab = bestColor.To<Lab>();

                    error1 = currentLab.L - bestLab.L;
                    error2 = currentLab.A - bestLab.A;
                    error3 = currentLab.B - bestLab.B;
                }
                else
                {
                    error1 = currentPixel.R - bestRgb.R;
                    error2 = currentPixel.G - bestRgb.G;
                    error3 = currentPixel.B - bestRgb.B;
                }

                switch (ditheringMethod)
                {
                    case DitheringMethod.FloydSteinberg:
                    default:
                        //  - * 7    where *=pixel being processed, -=previously processed pixel
                        //  3 5 1    and pixel difference is distributed to neighbor pixels
                        //           Note: 7+3+5+1=16 so we divide by 16 before adding.
                        div = 16;
                        
                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 7, div);

                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 3, div);

                            ApplyDitherMulDiv(dst, x - 0, y + 1, error1, error2, error3, 5, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 1, div);
                        }
                        break;

                    case DitheringMethod.Custom_1_32:
                        // Custom 1/32 Dithering
                        //  - - # 8 4    where *=pixel being processed, -=previously processed pixel
                        //  0 4 8 4 1    and pixel difference is distributed to neighbor pixels
                        //  0 0 2 1 0 
                        div = 32;

                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 8, div);

                        if (x + 2 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 2, y + 0, error1, error2, error3, 4, div);

                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 4, div);

                            ApplyDitherMulDiv(dst, x - 0, y + 1, error1, error2, error3, 8, div);
                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 4, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 1, error1, error2, error3, 1, div);
                        }
                        if (y + 2 < rect.Bottom)
                        {
                            ApplyDitherMulDiv(dst, x - 0, y + 2, error1, error2, error3, 2, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 2, error1, error2, error3, 1, div);
                        }
                        break;

                    case DitheringMethod.Burkes:
                        //  - - # 8 4    where *=pixel being processed, -=previously processed pixel
                        //  2 4 8 4 2    and pixel difference is distributed to neighbor pixels
                        //               (1/32)
                        div = 32;

                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 8, div);
                        if (x + 2 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 2, y + 0, error1, error2, error3, 4, div);
                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 1, error1, error2, error3, 2, div);

                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 4, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 1, error1, error2, error3, 8, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 4, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 1, error1, error2, error3, 2, div);
                        }
                        break;

                    case DitheringMethod.Sierra2_4A:
                        //  - - # 2      where *=pixel being processed, -=previously processed pixel
                        //  - 1 1        and pixel difference is distributed to neighbor pixels
                        //               (1/4)
                        div = 4;

                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 2, div);
                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 1, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 1, error1, error2, error3, 1, div);
                        }
                        break;

                    case DitheringMethod.Sierra2:
                        //  - - # 4 3    where *=pixel being processed, -=previously processed pixel
                        //  1 2 3 2 1    and pixel difference is distributed to neighbor pixels
                        //               (1/16)
                        div = 16;
                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 4, div);
                        if (x + 2 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 2, y + 0, error1, error2, error3, 3, div);

                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 1, error1, error2, error3, 1, div);
                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 2, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 1, error1, error2, error3, 3, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 2, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 1, error1, error2, error3, 1, div);
                        }
                        break;

                    case DitheringMethod.Sierra3:
                        //  - - # 5 3    where *=pixel being processed, -=previously processed pixel
                        //  2 4 5 4 2    and pixel difference is distributed to neighbor pixels
                        //    2 3 2      (1/32)
                        div = 32;
                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 5, div);
                        if (x + 2 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 2, y + 0, error1, error2, error3, 3, div);

                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 1, error1, error2, error3, 2, div);
                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 4, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 1, error1, error2, error3, 5, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 4, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 1, error1, error2, error3, 2, div);
                        }

                        if (y + 2 < rect.Bottom)
                        {
                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 2, error1, error2, error3, 2, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 2, error1, error2, error3, 3, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 2, error1, error2, error3, 2, div);

                        }
                        break;

                    case DitheringMethod.Stucki:
                        //  - - # 8 4    where *=pixel being processed, -=previously processed pixel
                        //  2 4 8 4 2    and pixel difference is distributed to neighbor pixels
                        //  1 2 4 2 1    (1/42)
                        div = 42;

                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 8, div);
                        if (x + 2 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 2, y + 0, error1, error2, error3, 4, div);
                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 1, error1, error2, error3, 2, div);

                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 4, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 1, error1, error2, error3, 8, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 4, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 1, error1, error2, error3, 2, div);
                        }
                        if (y + 2 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 2, error1, error2, error3, 1, div);

                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 2, error1, error2, error3, 2, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 2, error1, error2, error3, 4, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 2, error1, error2, error3, 2, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 2, error1, error2, error3, 1, div);
                        }
                        break;

                    case DitheringMethod.JarvisJudiceNinke:
                        //  - - # 7 5    where *=pixel being processed, -=previously processed pixel
                        //  3 5 7 5 3    and pixel difference is distributed to neighbor pixels
                        //  1 3 5 3 1    (1/48)
                        div = 48;

                        if (x + 1 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 1, y + 0, error1, error2, error3, 7, div);
                        if (x + 2 < rect.Right)
                            ApplyDitherMulDiv(dst, x + 2, y + 0, error1, error2, error3, 5, div);
                        if (y + 1 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 1, error1, error2, error3, 3, div);

                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 1, error1, error2, error3, 5, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 1, error1, error2, error3, 7, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 1, error1, error2, error3, 5, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 1, error1, error2, error3, 3, div);
                        }
                        if (y + 2 < rect.Bottom)
                        {
                            if (x - 2 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 2, y + 2, error1, error2, error3, 1, div);

                            if (x - 1 >= rect.Left)
                                ApplyDitherMulDiv(dst, x - 1, y + 2, error1, error2, error3, 3, div);

                            ApplyDitherMulDiv(dst, x + 0, y + 2, error1, error2, error3, 5, div);

                            if (x + 1 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 1, y + 2, error1, error2, error3, 3, div);

                            if (x + 2 < rect.Right)
                                ApplyDitherMulDiv(dst, x + 2, y + 2, error1, error2, error3, 1, div);
                        }
                        break;
                }
            }
        }
    }
}

IColorSpace FindNearestColor(IColorSpace color, IList<IColorSpace> palette, IColorSpaceComparison comparer, IDictionary<IColorSpace, IColorSpace> cache)
{
    IColorSpace result;

    if (cache.TryGetValue(color, out result))
        return result;

    var colorRgb = color.To<Rgb>();
    double minDistance = Double.MaxValue;
    int bestIndex = 0;

    for (int i = 0; i < palette.Count; i++)
    {
        double distance = comparer.Compare(colorRgb, palette[i]);

        if (distance < minDistance)
        {
            minDistance = distance;
            bestIndex = i;
            if (minDistance <= Double.Epsilon) break;
        }
    }

    result = palette[bestIndex];
    cache.Add(color, result);

    return result;
}

void ApplyDitherMulDiv(Surface dst, int x, int y, double error1, double error2, double error3, int mul, int div)
{
    ColorBgra currentPixel = dst[x, y];
    IColorSpace currentColor = new Rgb { R = currentPixel.R, G = currentPixel.G, B = currentPixel.B };
    var weight = (double)mul / div;

    if ((ErrorCalcMethod)InputErrorCalcMethod == ErrorCalcMethod.LAB)
    {
        Lab lab = currentColor.To<Lab>();
        lab.L += error1 * weight;
        lab.A += error2 * weight;
        lab.B += error3 * weight;

        lab.L = ClampL(lab.L);

        Rgb rgb = lab.To<Rgb>();
        dst[x, y] = ColorBgra.FromBgra(Round(rgb.B), Round(rgb.G), Round(rgb.R), currentPixel.A);
    }
    else
    {
        double R, G, B;

        R = currentPixel.R + error1 * weight;
        G = currentPixel.G + error2 * weight;
        B = currentPixel.B + error3 * weight;

        dst[x, y] = ColorBgra.FromBgra(Round(ClampRgb(B)), Round(ClampRgb(G)), Round(ClampRgb(R)), currentPixel.A);
    }
}

double ClampRgb(double val)
{
    if (val < 0) return 0;
    if (val > 255) return 255;
    return val;
}

double ClampL(double val)
{
    if (val < 0) return 0;
    if (val > 100) return 100;
    return val;
}

byte Round(double val)
{
    switch ((ErrorRoundingMethod)InputErrorRoundingMethod)
    {
        case ErrorRoundingMethod.Nearest:
        default:
            return Convert.ToByte(val);
        case ErrorRoundingMethod.Floor:
            return (byte)(val);
        case ErrorRoundingMethod.Ceiling:
            return (byte)Math.Ceiling(val);
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

// Code lovingly copied from StackOverflow (and tweaked a bit)
// Question/Answer: http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv/1626175#1626175
// Submitter: Greg http://stackoverflow.com/users/12971/greg
// License: http://creativecommons.org/licenses/by-sa/3.0/
internal static class HsvConverter
{
    internal static void ToColorSpace(IRgb color, IHsv item)
    {
        var max = Max(color.R, Max(color.G, color.B));
        var min = Min(color.R, Min(color.G, color.B));

        if (Math.Abs(max - min) <= float.Epsilon) {
            item.H = 0d;
        }
        else {
            double diff = max - min;

            if (Math.Abs(max - color.R) <= float.Epsilon) {
                item.H = 60d * (color.G - color.B) / diff;
            }
            else if (Math.Abs(max - color.G) <= float.Epsilon) {
                item.H = 60d * (color.B - color.R) / diff + 120d;
            }
            else {
                item.H = 60d * (color.R - color.G) / diff + 240d;
            }

            if (item.H < 0d) {
                item.H += 360d;
            }
        }

        item.S = (max <= 0) ? 0 : 1d - (1d * min / max);
        item.V = max / 255d;
    }

    internal static IRgb ToColor(IHsv item)
    {
        var range = Convert.ToInt32(Math.Floor(item.H / 60.0)) % 6;
        var f = item.H / 60.0 - Math.Floor(item.H / 60.0);

        var v = item.V * 255.0;
        var p = v * (1 - item.S);
        var q = v * (1 - f * item.S);
        var t = v * (1 - (1 - f) * item.S);

        switch (range) {
            case 0:
                return NewRgb(v, t, p);
            case 1:
                return NewRgb(q, v, p);
            case 2:
                return NewRgb(p, v, t);
            case 3:
                return NewRgb(p, q, v);
            case 4:
                return NewRgb(t, p, v);
        }
        return NewRgb(v, p, q);
    }

    private static IRgb NewRgb(double r, double g, double b)
    {
        return new Rgb { R = r, G = g, B = b };
    }

    private static double Max(double a, double b)
    {
        return a > b ? a : b;
    }

    private static double Min(double a, double b)
    {
        return a < b ? a : b;
    }
}

public interface IHsl : IColorSpace
{

    double H { get; set; }

    double S { get; set; }

    double L { get; set; }

}

public class Hsl : ColorSpace, IHsl
{

    public double H { get; set; }

    public double S { get; set; }

    public double L { get; set; }


    public override void Initialize(IRgb color)
    {
        HslConverter.ToColorSpace(color,this);
    }

    public override IRgb ToRgb()
    {
        return HslConverter.ToColor(this);
    }
}

internal static class HslConverter
{
    internal static void ToColorSpace(IRgb color, IHsl item)
    {
        var hsl = ToHsl(color);

        item.H = hsl.Item1;
        item.S = hsl.Item2;
        item.L = hsl.Item3;
    }

    private static Tuple<double, double, double> ToHsl(IRgb color)
    {
        color.R = Math.Round(color.R, 0);
        color.G = Math.Round(color.G, 0);
        color.B = Math.Round(color.B, 0);
        var max = Max(color.R, Max(color.G, color.B));
        var min = Min(color.R, Min(color.G, color.B));

        double h, s, l;

        //saturation
        var cnt = (max + min) / 2d;
        if (cnt <= 127d) {
            s = ((max - min) / (max + min));
        }
        else {
            s = ((max - min) / (510d - max - min));
        }

        //lightness
        l = ((max + min) / 2d) / 255d;

        //hue
        if (Math.Abs(max - min) <= float.Epsilon) {
            h = 0d;
            s = 0d;
        }
        else {
            double diff = max - min;

            if (Math.Abs(max - color.R) <= float.Epsilon) {
                h = 60d * (color.G - color.B) / diff;
            }
            else if (Math.Abs(max - color.G) <= float.Epsilon) {
                h = 60d * (color.B - color.R) / diff + 120d;
            }
            else {
                h = 60d * (color.R - color.G) / diff + 240d;
            }

            if (h < 0d) {
                h += 360d;
            }
        }

        return new Tuple<double, double, double>(h, s, l);
    }

    private static double Max(double a, double b)
    {
        return a > b ? a : b;
    }

    private static double Min(double a, double b)
    {
        return a < b ? a : b;
    }

    internal static IRgb ToColor(IHsl item)
    {
        var rangedH = item.H / 360.0;
        var r = 0.0;
        var g = 0.0;
        var b = 0.0;
        var s = item.S;
        var l = item.L;

        if (l > 0.0001) {
            if (s <= 0.0001) {
                r = g = b = l;
            }
            else {
                var temp2 = (l < 0.5) ? l * (1.0 + s) : l + s - (l * s);
                var temp1 = 2.0 * l - temp2;

                r = GetColorComponent(temp1, temp2, rangedH + 1.0 / 3.0);
                g = GetColorComponent(temp1, temp2, rangedH);
                b = GetColorComponent(temp1, temp2, rangedH - 1.0 / 3.0);
            }
        }
        return new Rgb {
            R = 255.0 * r,
            G = 255.0 * g,
            B = 255.0 * b
        };
    }

    private static double GetColorComponent(double temp1, double temp2, double temp3)
    {
        temp3 = MoveIntoRange(temp3);
        if (temp3 < 1.0 / 6.0) {
            return temp1 + (temp2 - temp1) * 6.0 * temp3;
        }

        if (temp3 < 0.5) {
            return temp2;
        }

        if (temp3 < 2.0 / 3.0) {
            return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
        }

        return temp1;
    }

    private static double MoveIntoRange(double temp3)
    {
        if (temp3 < 0.0) return temp3 + 1.0;
        if (temp3 > 1.0) return temp3 - 1.0;
        return temp3;
    }
}

public interface IHsv : IColorSpace
{

    double H { get; set; }

    double S { get; set; }

    double V { get; set; }

}

public class Hsv : ColorSpace, IHsv
{

    public double H { get; set; }

    public double S { get; set; }

    public double V { get; set; }


    public override void Initialize(IRgb color)
    {
        HsvConverter.ToColorSpace(color,this);
    }

    public override IRgb ToRgb()
    {
        return HsvConverter.ToColor(this);
    }
}
