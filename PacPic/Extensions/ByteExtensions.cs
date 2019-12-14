using System.Text;

namespace PacPic.Extensions
{
    public static class ByteExtensions
    {
        public static string GetString(this byte[] bytes)
        {
            var stringBuilder = new StringBuilder();
            for (int j = 0; j < bytes.Length; ++j)
            {
                var byteString = string.Format("{0:X2}", (int)bytes[j]);
                stringBuilder.Append(string.Format("{0:C}{1:C}", byteString[0], byteString[1]));
            }
            return stringBuilder.ToString();
        }
    }
}
