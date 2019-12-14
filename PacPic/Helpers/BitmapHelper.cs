using PacPic.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PacPic.Helpers
{
    public class BitmapHelper
    {
        public static Bitmap ReadBitmap(string file)
        {
            using (var fileStream = File.OpenRead(file))
            {
                if (fileStream.ReadByte() != 'B' || fileStream.ReadByte() != 'M')
                {
                    throw new Exception("Error: not a bmp file.");
                }

                fileStream.Seek(10, 0);
                var b4 = fileStream.ReadByte();
                var b3 = fileStream.ReadByte();
                var b2 = fileStream.ReadByte();
                var b1 = fileStream.ReadByte();
                var rawOffset = (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;

                fileStream.Seek(18, 0);
                b4 = fileStream.ReadByte();
                b3 = fileStream.ReadByte();
                b2 = fileStream.ReadByte();
                b1 = fileStream.ReadByte();
                var width = (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;

                fileStream.Seek(22, 0);
                b4 = fileStream.ReadByte();
                b3 = fileStream.ReadByte();
                b2 = fileStream.ReadByte();
                b1 = fileStream.ReadByte();
                var height = (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;

                fileStream.Seek(28, 0);
                b2 = fileStream.ReadByte();
                b1 = fileStream.ReadByte();
                var bitsPerPixel = (b1 << 8) + b2;

                fileStream.Seek(34, 0);
                b4 = fileStream.ReadByte();
                b3 = fileStream.ReadByte();
                b2 = fileStream.ReadByte();
                b1 = fileStream.ReadByte();
                var rawSize = (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;

                var rawImage = new byte[rawSize];
                fileStream.Seek(rawOffset, 0);
                fileStream.Read(rawImage, 0, rawSize);

                var image = GetImagePixels(rawImage, bitsPerPixel, width, height);

                return new Bitmap
                {
                    RawOffset = rawOffset,
                    Width = width,
                    Height = height,
                    BitsPerPixel = bitsPerPixel,
                    RawSize = rawSize,
                    RawImage = rawImage,
                    Image = image
                };
            }
        }

        private static Pixel[] GetImagePixels(byte[] rawImage, int bitsPerPixel, int width, int height)
        {
            if (bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                return null;
            }

            var image = bitsPerPixel == 24
                ? Get24BitImagePixels(rawImage, width)
                : Get32BitImagePixels(rawImage);

            return Rotate(image, height);
        }

        private static Pixel[] Rotate(Pixel[] image, int height)
        {
            int bytesPerPixelRow = image.Length / height;
            var result = new Pixel[image.Length];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < bytesPerPixelRow; j++)
                {
                    result[bytesPerPixelRow * i + j] = image[bytesPerPixelRow * (height - 1 - i) + j];
                }
            }
            return result;
        }

        private static Pixel[] Get24BitImagePixels(byte[] rawImage, int width)
        {
            var result = new List<Pixel>();
            
            int i = 0;
            while (i < rawImage.Length)
            {
                if (result.Count != 0 && result.Count % width == 0 && i % 4 != 0)
                {
                    while (i % 4 != 0)
                    {
                        i++;
                    }
                }
                else
                {
                    result.Add(new Pixel
                    {
                        B = rawImage[i],
                        G = rawImage[i + 1],
                        R = rawImage[i + 2]
                    });
                    i += 3;
                }
            }

            return result.ToArray();
        }

        private static Pixel[] Get32BitImagePixels(byte[] rawImage)
        {
            var result = new List<Pixel>();
            
            for (int i = 0; i < rawImage.Length; i += 4)
            {
                result.Add(new Pixel
                {
                    B = rawImage[i],
                    G = rawImage[i + 1],
                    R = rawImage[i + 2]
                });
            }

            return result.ToArray();
        }
    }
}
