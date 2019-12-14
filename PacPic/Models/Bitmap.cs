namespace PacPic.Models
{
    public class Bitmap
    {
        public int RawOffset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitsPerPixel { get; set; }
        public int RawSize { get; set; }
        public byte[] RawImage { get; set; }
        public Pixel[] Image { get; set; }
    }
}
