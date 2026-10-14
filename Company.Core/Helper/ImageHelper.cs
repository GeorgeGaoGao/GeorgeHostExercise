using Company.Core.Extensions;
using Company.Core.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Company.Core.Helper
{
    public static class ImageHelper
    {
        public static Bitmap Load(string path)
        {
            byte[] bytes = FileHelper.Load(path);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms) as Bitmap;
            }
        }

        public static UnmanagedArray2D<ushort> LoadTiff(string filename)
        {
            if (!File.Exists(filename)) return null;
            Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource source = decoder.Frames[0];
            int stride = source.PixelWidth * source.Format.BitsPerPixel / 8;
            ushort[] pixels = new ushort[source.PixelWidth * source.PixelHeight];
            source.CopyPixels(pixels, stride, 0);
            var bitmap = source.ToBitmap();//转换为位图。
            var data = bitmap.LockBits();
            UnmanagedArray2D<ushort> image=new UnmanagedArray2D<ushort>(source.PixelWidth, source.PixelHeight);

            MemoryHelper.CopyMemory(image.Header, data.Scan0, image.Length);
            bitmap.UnlockBits(data);
            return image;
        }
    }
}
