using PacPic.Models;
using System;
using System.Collections.Generic;

namespace PacPic.Helpers
{
    public class ConvertHelper
    {
        public static byte[] Convert(Pixel[] image, int width, int rawSize, Models.Type type)
        {
            switch (type)
            {
                case Models.Type.Uncompressed1Bit:
                    {
                        List<byte> result = new List<byte>();

                        byte pixel = 0;
                        int index = 0;

                        for (int i = 0; i < image.Length; i++)
                        {
                            if (image[i].R == 0 && image[i].G == 0 && image[i].B == 0)
                                pixel = (byte)(pixel << 1 | 1);
                            else
                                pixel = (byte)(pixel << 1);

                            index++;

                            if ((i + 1) % width == 0)
                            {
                                while (index < 8)
                                {
                                    pixel = (byte)(pixel << 1 | 1);
                                    index++;
                                }
                            }

                            if (index >= 8)
                            {
                                result.Add(pixel);
                                pixel = 0;
                                index = 0;
                            }
                        }

                        return result.ToArray();
                    }
                case Models.Type.Uncompressed8Bit:
                    {
                        List<byte> result = new List<byte>();

                        for (int i = 0; i < image.Length; i++)
                        {
                            result.Add(sub_411172(image[i].B, image[i].G, image[i].R));
                        }

                        return result.ToArray();
                    }
                case Models.Type.Uncompressed16Bit:
                    {
                        List<byte> result = new List<byte>();

                        for (int i = 0; i < image.Length; i++)
                        {
                            result.Add(sub_411113(image[i].B, image[i].G, image[i].R, ref rawSize));
                            result.Add((byte)rawSize);
                        }

                        return result.ToArray();
                    }
                case Models.Type.Compressed1Bit:
                    {
                        List<byte> result = new List<byte>();

                        byte uncompressedPixel = 0;
                        byte compressedPixel = 1;
                        bool useCompressedPixel = true;
                        int index = 0;

                        Action<byte> AddToResult = (value) =>
                        {
                            result.Add(value);
                            uncompressedPixel = 0;
                            compressedPixel = 1;
                            useCompressedPixel = true;
                            index = 0;
                        };

                        for (int i = 0; i < image.Length; i++)
                        {
                            if (index != 0 && useCompressedPixel)
                            {
                                if (image[i].R == 0 && image[i].G == 0 && image[i].B == 0 && compressedPixel != 3 ||
                                    (image[i].R != 0 || image[i].G != 0 || image[i].B != 0) && compressedPixel != 2)
                                {
                                    if (index < 7)
                                    {
                                        useCompressedPixel = false;
                                    }
                                    else
                                    {
                                        AddToResult((byte)(compressedPixel << 6 | index));
                                    }
                                }
                            }

                            if (image[i].R == 0 && image[i].G == 0 && image[i].B == 0)
                                uncompressedPixel = (byte)(uncompressedPixel << 1 | 1);
                            else
                                uncompressedPixel = (byte)(uncompressedPixel << 1);

                            if (index == 0)
                            {
                                if (image[i].R == 0 && image[i].G == 0 && image[i].B == 0)
                                    compressedPixel = (byte)(compressedPixel << 1 | 1);
                                else
                                    compressedPixel = (byte)(compressedPixel << 1);
                            }

                            index++;

                            if (index >= 7 && !useCompressedPixel)
                            {
                                AddToResult(uncompressedPixel);
                            }

                            if (index >= 63)
                            {
                                AddToResult((byte)(compressedPixel << 6 | index));
                            }
                        }

                        return result.ToArray();
                    }
                case Models.Type.Compressed8Bit:
                case Models.Type.Compressed16Bit:
                    {
                        List<byte> result = new List<byte>();
                        
                        int v20 = -1;
                        int v16 = 0;

                        for (int i = 0; i < image.Length; i++)
                        {
                            var b = image[i].B;
                            var g = image[i].G;
                            var r = image[i].R;

                            int j = 0;
                            for (j = 0; b == image[i].B && g == image[i].G && r == image[i].R && j < 127; ++j)
                            {
                                image[i].B = image[j + i].B;
                                image[i].G = image[j + i].G;
                                image[i].R = image[j + i].R;
                            }

                            if (j > 1 || v16 < 129 && v16 != 0)
                            {
                                result.Add((byte)j);

                                if (type == Models.Type.Compressed8Bit)
                                {
                                    result.Add(sub_411172(b, g, r));
                                }
                                else if (type == Models.Type.Compressed16Bit)
                                {
                                    result.Add(sub_411113(b, g, r, ref rawSize));
                                    result.Add((byte)rawSize);
                                }

                                i += j - 1;
                                v20 = -1;
                                v16 = 0;
                            }
                            else
                            {
                                --v16;
                                if (v20 == -1)
                                {
                                    v20 = result.Count;
                                    result.Add((byte)v16);
                                }
                                else
                                {
                                    result[v20] = (byte)v16;
                                }

                                if (type == Models.Type.Compressed8Bit)
                                {
                                    result.Add(sub_411172(b, g, r));
                                }
                                else if (type == Models.Type.Compressed16Bit)
                                {
                                    result.Add(sub_411113(b, g, r, ref rawSize));
                                    result.Add((byte)rawSize);
                                }
                            }
                        }

                        return result.ToArray();
                    }
            }

            return null;// sub_41112C(image, width, rawSize, type);
        }

        /*public static byte[] sub_41112C(byte[] Memory, int dword_41A6C4, int dword_41A6C0, int a3)
        {
            int result; // eax
            int v4; // ST160_4
            int v5; // ST148_4
            int v6; // ST148_4
            int v7; // ST148_4
            //char[] Memory; // [esp+D0h] [ebp-98h]
            int i; // [esp+DFh] [ebp-89h]
            byte v10; // [esp+EBh] [ebp-7Dh]
            byte v11; // [esp+F7h] [ebp-71h]
            int v12; // [esp+103h] [ebp-65h]
            byte v13; // [esp+10Fh] [ebp-59h]
            byte v14; // [esp+11Bh] [ebp-4Dh]
            byte v15; // [esp+127h] [ebp-41h]
            int v16; // [esp+133h] [ebp-35h]
            int v17; // [esp+13Ch] [ebp-2Ch]
            int v18; // [esp+148h] [ebp-20h]
            int v19; // [esp+154h] [ebp-14h]
            int v20; // [esp+160h] [ebp-8h]

            v20 = -1;
            v16 = 0;
            v12 = 0;
            v10 = (byte)0;
            //Memory = new char[0x100000u];

            //if (dword_41A6D0 == 24)
            //    v18 = sub_411203(File, Offset, Denominator, dword_41A6C0, dword_41A6C4, Dest, Memory);
            //else
            //    v18 = sub_411208(File, Offset, Denominator, dword_41A6C0, Dest, Memory);
            //j_strcpy(Dest, &Source);

            byte[] Dest = new byte[Memory.Length];

            v18 = Memory.Length;

            v17 = 0;
            v19 = 0;
            while (v19 < v18)
            {
                v15 = Memory[v19];
                v4 = v19 + 1;
                v14 = Memory[v4++];
                v13 = Memory[v4];
                v19 = v4 + 1;
                switch (a3)
                {
                    case 0x0100:
                        if (v15 != 0)
                            v10 = (byte)(v10 * 2);
                        else
                            v10 = (byte)(2 * v10 + 1);
                        ++v12;
                        if ((v19 % (3 * dword_41A6C4)) == 0 && dword_41A6C4 % 8 != 0)
                        {
                            while ((int)v12 < 8)
                            {
                                v10 = (byte)(2 * v10 + 1);
                                ++v12;
                            }
                        }
                        if ((int)v12 >= 8)
                        {
                            Dest[v17++] = v10;
                            v12 = 0;
                            v10 = (byte)0;
                        }
                        break;
                    case 0x0800:
                        Dest[v17] = sub_411113(v15, v14, v13, ref dword_41A6C0);
                        v5 = v17 + 1;
                        Dest[v5] = (byte)dword_41A6C0;
                        v17 = v5 + 1;
                        break;
                    case 0x0500:
                        Dest[v17++] = sub_411172(v15, v14, v13);
                        break;
                    case 0x8100:
                        throw new Exception("not imp[lemented");
                    case 0x8800:
                    case 0x8500:
                        v12 = Memory[v19];
                        v11 = Memory[v19 + 1];
                        v10 = Memory[v19 + 2];
                        for (i = 0; v15 == v12 && v14 == v11 && v13 == v10 && i < 127; ++i)
                        {
                            v12 = Memory[3 * i + v19];
                            v11 = Memory[3 * i + 1 + v19];
                            v10 = Memory[3 * i + 2 + v19];
                        }
                        if (i > 1 || v16 < 129 && v16 != 0)
                        {
                            Dest[v17++] = (byte)i;
                            if (a3 == 34048)
                            {
                                Dest[v17++] = sub_411172(v15, v14, v13);
                            }
                            else if (a3 == 34816)
                            {
                                Dest[v17] = sub_411113(v15, v14, v13, ref dword_41A6C0);
                                v6 = v17 + 1;
                                Dest[v6] = (byte)dword_41A6C0;
                                v17 = v6 + 1;
                            }
                            v19 += (int)(3 * (i - 1));
                            v20 = -1;
                            v16 = 0;
                        }
                        else
                        {
                            --v16;
                            if (v20 == -1)
                            {
                                v20 = v17;
                                Dest[v17++] = (byte)v16;
                            }
                            else
                            {
                                Dest[v20] = (byte)v16;
                            }
                            if (a3 == 34048)
                            {
                                Dest[v17++] = sub_411172(v15, v14, v13);
                            }
                            else if (a3 == 34816)
                            {
                                Dest[v17] = sub_411113(v15, v14, v13, ref dword_41A6C0);
                                v7 = v17 + 1;
                                Dest[v7] = (byte)dword_41A6C0;
                                v17 = v7 + 1;
                            }
                        }
                        break;
                }
            }
            result = v17;

            Dest = Dest.Take(result).ToArray();

            return Dest;
        }*/

        static byte sub_411113(uint a1, uint a2, uint a3, ref int dword_41A6C0)
        {
            int v3 = 0; // eax
            int v4; // rax
            int v5; // esi
            int v6; // rax
            int v7; // rax
            int v8; // esi
            int v9; // rax

            if (a3 != 169 || a2 != 171 || a1 != 169)
            {
                v4 = (int)a3 / 8;
                v5 = 4 * v4;
                v6 = (int)a2 / 32;
                dword_41A6C0 = v6 + 2 * v5;
                v7 = (int)a2 / 4;
                v8 = v7 % 8;
                v9 = (int)a1 / 8;
                v3 = v9 + 32 * v8;
            }
            else
            {
                dword_41A6C0 = 224;
                LoByte(ref v3);
            }
            return (byte)v3;
        }

        static byte sub_411172(int a1, int a2, int a3)
        {
            int v4; // rax
            int v5; // esi
            int v6; // rax
            int v7; // ebx
            int v8; // rax

            if (a3 == 169 && a2 == 171 && a1 == 169)
                return byte.MinValue;//(char)(-64);
            v4 = (int)a3 / 32;
            v5 = 16 * v4;
            v6 = (int)a2 / 32;
            v7 = 4 * v6 + 2 * v5;
            v8 = (int)a1 / 64;
            return (byte)(LoByte(v8) + v7);
        }

        public static void LoByte(ref int nValue)
        {
            nValue = (Byte)(nValue & 0xFF);
        }

        public static byte LoByte(int nValue)
        {
            return (Byte)(nValue & 0xFF);
        }
    }
}
