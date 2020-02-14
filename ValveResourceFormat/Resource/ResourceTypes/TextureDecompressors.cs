using System;
using System.IO;
using SkiaSharp;

namespace ValveResourceFormat.ResourceTypes
{
    internal static class TextureDecompressors
    {
        public static SKBitmap ReadI8(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var color = r.ReadByte();

                    res.SetPixel(x, y, new SKColor(color, color, color, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadIA88(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var color = r.ReadByte();
                    var alpha = r.ReadByte();

                    res.SetPixel(x, y, new SKColor(color, color, color, alpha));
                }
            }

            return res;
        }

        public static SKBitmap ReadUIntPixels(BinaryReader r, int w, int h, SKColorType colorType)
        {
            var res = new SKBitmap(w, h, colorType, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    res.SetPixel(x, y, new SKColor(r.ReadUInt32()));
                }
            }

            return res;
        }

        public static SKBitmap ReadR16(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(r.ReadUInt16() / 256);

                    res.SetPixel(x, y, new SKColor(hr, 0, 0, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadRG1616(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(r.ReadUInt16() / 256);
                    var hg = (byte)(r.ReadUInt16() / 256);

                    res.SetPixel(x, y, new SKColor(hr, hg, 0, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadR16F(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(HalfTypeHelper.Convert(r.ReadUInt16()) * 255);

                    res.SetPixel(x, y, new SKColor(hr, 0, 0, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadRG1616F(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(HalfTypeHelper.Convert(r.ReadUInt16()) * 255);
                    var hg = (byte)(HalfTypeHelper.Convert(r.ReadUInt16()) * 255);

                    res.SetPixel(x, y, new SKColor(hr, hg, 0, 255));
                }
            }

            return res;
        }

        public static void ReadRGBA16161616(SKImageInfo imageInfo, BinaryReader r, Span<byte> data)
        {
            var bytes = r.ReadBytes(imageInfo.Width * imageInfo.Height * 8);
            var log = 0d;

            for (int i = 0, j = 0; i < bytes.Length; i += 8, j += 4)
            {
                var hr = BitConverter.ToUInt16(bytes, i + 0) / 256f;
                var hg = BitConverter.ToUInt16(bytes, i + 2) / 256f;
                var hb = BitConverter.ToUInt16(bytes, i + 4) / 256f;
                var lum = (hr * 0.299f) + (hg * 0.587f) + (hb * 0.114f);
                log += Math.Log(0.0000000001d + lum);
            }

            log = Math.Exp(log / (imageInfo.Width * imageInfo.Height));

            for (int i = 0, j = 0; i < bytes.Length; i += 8, j += 4)
            {
                var hr = BitConverter.ToUInt16(bytes, i + 0) / 256f;
                var hg = BitConverter.ToUInt16(bytes, i + 2) / 256f;
                var hb = BitConverter.ToUInt16(bytes, i + 4) / 256f;
                var ha = BitConverter.ToUInt16(bytes, i + 6) / 256f;

                var y = (hr * 0.299f) + (hg * 0.587f) + (hb * 0.114f);
                var u = (hb - y) * 0.565f;
                var v = (hr - y) * 0.713f;

                var mul = 4.0f * y / log;
                mul = mul / (1.0f + mul);
                mul /= y;

                hr = (float)Math.Pow((y + (1.403f * v)) * mul, 2.25f);
                hg = (float)Math.Pow((y - (0.344f * u) - (0.714f * v)) * mul, 2.25f);
                hb = (float)Math.Pow((y + (1.770f * u)) * mul, 2.25f);

#pragma warning disable SA1503
                if (hr < 0) hr = 0;
                if (hr > 1) hr = 1;
                if (hg < 0) hg = 0;
                if (hg > 1) hg = 1;
                if (hb < 0) hb = 0;
                if (hb > 1) hb = 1;
#pragma warning restore SA1503

                data[j + 0] = (byte)(hr * 255); // r
                data[j + 1] = (byte)(hg * 255); // g
                data[j + 2] = (byte)(hb * 255); // b
                data[j + 3] = (byte)(ha * 255); // a
            }
        }

        public static void ReadRGBA16161616F(SKImageInfo imageInfo, BinaryReader r, Span<byte> data)
        {
            var bytes = r.ReadBytes(imageInfo.Width * imageInfo.Height * 8);
            var log = 0d;

            for (int i = 0, j = 0; i < bytes.Length; i += 8, j += 4)
            {
                var hr = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 0));
                var hg = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 2));
                var hb = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 4));
                var lum = (hr * 0.299f) + (hg * 0.587f) + (hb * 0.114f);
                log += Math.Log(0.0000000001d + lum);
            }

            log = Math.Exp(log / (imageInfo.Width * imageInfo.Height));

            for (int i = 0, j = 0; i < bytes.Length; i += 8, j += 4)
            {
                var hr = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 0));
                var hg = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 2));
                var hb = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 4));
                var ha = HalfTypeHelper.Convert(BitConverter.ToUInt16(bytes, i + 6));

                var y = (hr * 0.299f) + (hg * 0.587f) + (hb * 0.114f);
                var u = (hb - y) * 0.565f;
                var v = (hr - y) * 0.713f;

                var mul = 4.0f * y / log;
                mul = mul / (1.0f + mul);
                mul /= y;

                hr = (float)Math.Pow((y + (1.403f * v)) * mul, 2.25f);
                hg = (float)Math.Pow((y - (0.344f * u) - (0.714f * v)) * mul, 2.25f);
                hb = (float)Math.Pow((y + (1.770f * u)) * mul, 2.25f);

#pragma warning disable SA1503
                if (hr < 0) hr = 0;
                if (hr > 1) hr = 1;
                if (hg < 0) hg = 0;
                if (hg > 1) hg = 1;
                if (hb < 0) hb = 0;
                if (hb > 1) hb = 1;
#pragma warning restore SA1503

                data[j + 0] = (byte)(hr * 255); // r
                data[j + 1] = (byte)(hg * 255); // g
                data[j + 2] = (byte)(hb * 255); // b
                data[j + 3] = (byte)(ha * 255); // a
            }
        }

        public static SKBitmap ReadR32F(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(r.ReadSingle() * 255);

                    res.SetPixel(x, y, new SKColor(hr, 0, 0, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadRG3232F(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(r.ReadSingle() * 255);
                    var hg = (byte)(r.ReadSingle() * 255);

                    res.SetPixel(x, y, new SKColor(hr, hg, 0, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadRGB323232F(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(r.ReadSingle() * 255);
                    var hg = (byte)(r.ReadSingle() * 255);
                    var hb = (byte)(r.ReadSingle() * 255);

                    res.SetPixel(x, y, new SKColor(hr, hg, hb, 255));
                }
            }

            return res;
        }

        public static SKBitmap ReadRGBA32323232F(BinaryReader r, int w, int h)
        {
            var res = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var hr = (byte)(r.ReadSingle() * 255);
                    var hg = (byte)(r.ReadSingle() * 255);
                    var hb = (byte)(r.ReadSingle() * 255);
                    var ha = (byte)(r.ReadSingle() * 255);

                    res.SetPixel(x, y, new SKColor(hr, hg, hb, ha));
                }
            }

            return res;
        }

        public static void UncompressATI1N(BinaryReader r, Span<byte> data, int w, int h)
        {
            byte InterpolateColor(byte index, sbyte red0, sbyte red1)
            {
                float red;
                if (index == 0)
                {
                    red = red0;
                }
                else if (index == 1)
                {
                    red = red1;
                }
                else
                {
                    if (red0 > red1)
                    {
                        index -= 1;
                        red = ((red0 * (7 - index)) + (red1 * index)) / 7.0f;
                    }
                    else
                    {
                        if (index == 6)
                        {
                            red = -127.0f;
                        }
                        else if (index == 7)
                        {
                            red = 127.0f;
                        }
                        else
                        {
                            index -= 1;
                            red = ((red0 * (5 - index)) + (red1 * index)) / 5.0f;
                        }
                    }
                }

                return (byte)(((red + 127) * (255.0f / 254.0f)) + 0.5f);
            }

            var dataIndex = 0;
            var blockCountX = (w + 3) / 4;
            var blockCountY = (h + 3) / 4;

            for (var j = 0; j < blockCountY; j++)
            {
                for (var i = 0; i < blockCountX; i++)
                {
                    var blockStorage = r.ReadBytes(8);
                    sbyte red0 = (sbyte)blockStorage[0];
                    sbyte red1 = (sbyte)blockStorage[1];
                    red0 = (red0 == -128) ? (sbyte)-127 : red0;
                    red1 = (red1 == -128) ? (sbyte)-127 : red1;

                    ulong rIndex = blockStorage[2];
                    rIndex |= (ulong)blockStorage[3] << 8;
                    rIndex |= (ulong)blockStorage[4] << 16;
                    rIndex |= (ulong)blockStorage[5] << 24;
                    rIndex |= (ulong)blockStorage[6] << 32;
                    rIndex |= (ulong)blockStorage[7] << 40;

                    for (int p = 0; p < 16; p++)
                    {
                        uint index = (byte)((uint)(rIndex >> (3 * p)) & 0x07);

                        data[dataIndex++] = InterpolateColor((byte)index, red0, red1);

                        // Is mult 4?
                        if (((p + 1) & 0x3) == 0)
                        {
                            dataIndex += blockCountX - 4;
                        }
                    }
                }
            }
        }

        public static void UncompressDXT1(SKImageInfo imageInfo, BinaryReader r, Span<byte> data, int w, int h)
        {
            var blockCountX = (w + 3) / 4;
            var blockCountY = (h + 3) / 4;

            for (var j = 0; j < blockCountY; j++)
            {
                for (var i = 0; i < blockCountX; i++)
                {
                    var blockStorage = r.ReadBytes(8);
                    DecompressBlockDXT1(i * 4, j * 4, imageInfo.Width, blockStorage, data, imageInfo.RowBytes);
                }
            }
        }

        private static void DecompressBlockDXT1(int x, int y, int width, byte[] blockStorage, Span<byte> pixels, int stride)
        {
            var color0 = (ushort)(blockStorage[0] | blockStorage[1] << 8);
            var color1 = (ushort)(blockStorage[2] | blockStorage[3] << 8);

            ConvertRgb565ToRgb888(color0, out var r0, out var g0, out var b0);
            ConvertRgb565ToRgb888(color1, out var r1, out var g1, out var b1);

            uint c1 = blockStorage[4];
            var c2 = (uint)blockStorage[5] << 8;
            var c3 = (uint)blockStorage[6] << 16;
            var c4 = (uint)blockStorage[7] << 24;
            var code = c1 | c2 | c3 | c4;

            for (var j = 0; j < 4; j++)
            {
                for (var i = 0; i < 4; i++)
                {
                    var positionCode = (byte)((code >> (2 * ((4 * j) + i))) & 0x03);

                    byte finalR = 0, finalG = 0, finalB = 0;

                    switch (positionCode)
                    {
                        case 0:
                            finalR = r0;
                            finalG = g0;
                            finalB = b0;
                            break;
                        case 1:
                            finalR = r1;
                            finalG = g1;
                            finalB = b1;
                            break;
                        case 2:
                            if (color0 > color1)
                            {
                                finalR = (byte)(((2 * r0) + r1) / 3);
                                finalG = (byte)(((2 * g0) + g1) / 3);
                                finalB = (byte)(((2 * b0) + b1) / 3);
                            }
                            else
                            {
                                finalR = (byte)((r0 + r1) / 2);
                                finalG = (byte)((g0 + g1) / 2);
                                finalB = (byte)((b0 + b1) / 2);
                            }

                            break;
                        case 3:
                            if (color0 < color1)
                            {
                                break;
                            }

                            finalR = (byte)(((2 * r1) + r0) / 3);
                            finalG = (byte)(((2 * g1) + g0) / 3);
                            finalB = (byte)(((2 * b1) + b0) / 3);
                            break;
                    }

                    var pixelIndex = ((y + j) * stride) + ((x + i) * 4);

                    if (x + i < width && pixels.Length > pixelIndex + 3)
                    {
                        pixels[pixelIndex] = finalB;
                        pixels[pixelIndex + 1] = finalG;
                        pixels[pixelIndex + 2] = finalR;
                        pixels[pixelIndex + 3] = byte.MaxValue;
                    }
                }
            }
        }

        public static void UncompressDXT5(SKImageInfo imageInfo, BinaryReader r, Span<byte> data, int w, int h, bool yCoCg, bool normalize, bool invert)
        {
            var blockCountX = (w + 3) / 4;
            var blockCountY = (h + 3) / 4;

            for (var j = 0; j < blockCountY; j++)
            {
                for (var i = 0; i < blockCountX; i++)
                {
                    var blockStorage = r.ReadBytes(16);
                    DecompressBlockDXT5(i * 4, j * 4, imageInfo.Width, blockStorage, data, imageInfo.RowBytes, yCoCg, normalize, invert);
                }
            }
        }

        private static void DecompressBlockDXT5(int x, int y, int width, byte[] blockStorage, Span<byte> pixels, int stride, bool yCoCg, bool normalize, bool invert)
        {
            var alpha0 = blockStorage[0];
            var alpha1 = blockStorage[1];

            uint a1 = blockStorage[4];
            var a2 = (uint)blockStorage[5] << 8;
            var a3 = (uint)blockStorage[6] << 16;
            var a4 = (uint)blockStorage[7] << 24;
            var alphaCode1 = a1 | a2 | a3 | a4;

            var alphaCode2 = (ushort)(blockStorage[2] | (blockStorage[3] << 8));

            var color0 = (ushort)(blockStorage[8] | blockStorage[9] << 8);
            var color1 = (ushort)(blockStorage[10] | blockStorage[11] << 8);

            ConvertRgb565ToRgb888(color0, out var r0, out var g0, out var b0);
            ConvertRgb565ToRgb888(color1, out var r1, out var g1, out var b1);

            uint c1 = blockStorage[12];
            var c2 = (uint)blockStorage[13] << 8;
            var c3 = (uint)blockStorage[14] << 16;
            var c4 = (uint)blockStorage[15] << 24;
            var code = c1 | c2 | c3 | c4;

            for (var j = 0; j < 4; j++)
            {
                for (var i = 0; i < 4; i++)
                {
                    var pixelIndex = ((y + j) * stride) + ((x + i) * 4);

                    // TODO: Why are we skipping so poorly
                    if (x + i >= width || pixels.Length < pixelIndex + 3)
                    {
                        continue;
                    }

                    var alphaCodeIndex = 3 * ((4 * j) + i);
                    int alphaCode;

                    if (alphaCodeIndex <= 12)
                    {
                        alphaCode = (alphaCode2 >> alphaCodeIndex) & 0x07;
                    }
                    else if (alphaCodeIndex == 15)
                    {
                        alphaCode = (int)(((uint)alphaCode2 >> 15) | ((alphaCode1 << 1) & 0x06));
                    }
                    else
                    {
                        alphaCode = (int)((alphaCode1 >> (alphaCodeIndex - 16)) & 0x07);
                    }

                    byte finalAlpha;
                    if (alphaCode == 0)
                    {
                        finalAlpha = alpha0;
                    }
                    else if (alphaCode == 1)
                    {
                        finalAlpha = alpha1;
                    }
                    else
                    {
                        if (alpha0 > alpha1)
                        {
                            finalAlpha = (byte)((((8 - alphaCode) * alpha0) + ((alphaCode - 1) * alpha1)) / 7);
                        }
                        else
                        {
                            if (alphaCode == 6)
                            {
                                finalAlpha = 0;
                            }
                            else if (alphaCode == 7)
                            {
                                finalAlpha = 255;
                            }
                            else
                            {
                                finalAlpha = (byte)((((6 - alphaCode) * alpha0) + ((alphaCode - 1) * alpha1)) / 5);
                            }
                        }
                    }

                    var colorCode = (byte)((code >> (2 * ((4 * j) + i))) & 0x03);

                    byte finalR = 0, finalG = 0, finalB = 0;

                    switch (colorCode)
                    {
                        case 0:
                            finalR = r0;
                            finalG = g0;
                            finalB = b0;
                            break;
                        case 1:
                            finalR = r1;
                            finalG = g1;
                            finalB = b1;
                            break;
                        case 2:
                            finalR = (byte)(((2 * r0) + r1) / 3);
                            finalG = (byte)(((2 * g0) + g1) / 3);
                            finalB = (byte)(((2 * b0) + b1) / 3);
                            break;
                        case 3:
                            finalR = (byte)(((2 * r1) + r0) / 3);
                            finalG = (byte)(((2 * g1) + g0) / 3);
                            finalB = (byte)(((2 * b1) + b0) / 3);
                            break;
                    }

                    if (yCoCg)
                    {
                        var s = (finalB >> 3) + 1;
                        var co = (finalR - 128) / s;
                        var cg = (finalG - 128) / s;

                        finalR = ClampColor(finalAlpha + co - cg);
                        finalG = ClampColor(finalAlpha + cg);
                        finalB = ClampColor(finalAlpha - co - cg);
                        finalAlpha = 255; // TODO: yCoCg should have an alpha too?
                    }

                    if (normalize)
                    {
                        var swizzleA = (finalAlpha * 2) - 255;     // premul A
                        var swizzleG = (finalG * 2) - 255;         // premul G
                        var deriveB = (int)System.Math.Sqrt((255 * 255) - (swizzleA * swizzleA) - (swizzleG * swizzleG));
                        finalR = ClampColor((swizzleA / 2) + 128); // unpremul A and normalize (128 = forward, or facing viewer)
                        finalG = ClampColor((swizzleG / 2) + 128); // unpremul G and normalize
                        finalB = ClampColor((deriveB / 2) + 128);  // unpremul B and normalize
                        finalAlpha = 255;
                    }

                    if (invert)
                    {
                        finalG = (byte)(~finalG);  // LegacySource1InvertNormals
                    }

                    pixels[pixelIndex] = finalB;
                    pixels[pixelIndex + 1] = finalG;
                    pixels[pixelIndex + 2] = finalR;
                    pixels[pixelIndex + 3] = finalAlpha;
                }
            }
        }

        private static byte ClampColor(int a)
        {
            if (a > 255)
            {
                return 255;
            }

            return a < 0 ? (byte)0 : (byte)a;
        }

        private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
        {
            int temp;

            temp = ((color >> 11) * 255) + 16;
            r = (byte)(((temp / 32) + temp) / 32);
            temp = (((color & 0x07E0) >> 5) * 255) + 32;
            g = (byte)(((temp / 64) + temp) / 64);
            temp = ((color & 0x001F) * 255) + 16;
            b = (byte)(((temp / 32) + temp) / 32);
        }
    }
}
