using System;
using System.IO;
using PacPic.Helpers;
using PacPic.Extensions;

namespace PacPic
{
    class Program
    {
        static void Main(string[] args)
        {
            //Program2.Main2(args);

            if (args.Length > 0)
            {
                var directory = args[1];
                var index = 0;
                var files = Directory.GetFiles(directory, "*.bmp");

                if (files.Length == 0)
                {
                    throw new Exception("Error: no *.bmp files found in provided directory.");
                }

                Console.WriteLine(files[index]);

                var bitmap = BitmapHelper.ReadBitmap(files[index]);

                var bytes = ConvertHelper.Convert(bitmap.Image, bitmap.Width, bitmap.RawSize, Models.Type.Uncompressed1Bit);
                Console.WriteLine(bytes.GetString());

                //bytes = ConvertHelper.sub_41112C(bitmap.Image, bitmap.Width, bitmap.RawSize, 0x0100);
                //Console.WriteLine(bytes.GetString());

                bytes = ConvertHelper.Convert(bitmap.Image, bitmap.Width, bitmap.RawSize, Models.Type.Compressed1Bit);
                Console.WriteLine(bytes.GetString());

                return;
            }

            Console.WriteLine("Bitmap reader v1 by Hotter");
            Console.WriteLine("Displays 2 color bitmap images and compresed code");
            Console.WriteLine("First argument must be directori where bitmap image can be found");

            return;
        }
    }
}
