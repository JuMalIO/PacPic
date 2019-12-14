namespace PacPic.Models
{
    public enum Type
    {
        Uncompressed1Bit = 0x0100,
        Uncompressed8Bit = 0x0500,
        Uncompressed12Bit = 0x0700,
        Uncompressed16Bit = 0x0800,
        Uncompressed18Bit = 0x0A00,
        Compressed1Bit = 0x8100,
        Compressed8Bit = 0x8500,
        Compressed12Bit = 0x8700,
        Compressed16Bit = 0x8800,
        Compressed18Bit = 0x8A00,
    }
}
