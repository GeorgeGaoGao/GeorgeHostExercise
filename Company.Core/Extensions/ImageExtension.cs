using Company.Core.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace Company.Core.Extensions
{
    public static class ImageExtension
    {
        public static Bitmap ToBitmap(this BitmapSource bitmapSource)
        {
            Assert.NotNull(bitmapSource);
            PixelFormat pixelFormat;
            if (bitmapSource.Format==System.Windows.Media.PixelFormats.Gray8)
            {
                pixelFormat = PixelFormat.Format8bppIndexed;

            }
            else if (bitmapSource.Format == System.Windows.Media.PixelFormats.Bgr24)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }
            else if (bitmapSource.Format == System.Windows.Media.PixelFormats.Bgr32)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }
            else if (bitmapSource.Format == System.Windows.Media.PixelFormats.Gray16)
            {
                pixelFormat=PixelFormat.Format16bppGrayScale;
            }
            else
            {
                throw new Exception($"未实现{bitmapSource.Format}格式的转换");
            }

            Bitmap bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, pixelFormat);
            BitmapData data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            bitmapSource.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height*data.Stride,
                data.Stride
                );
             bitmap.UnlockBits( data );
            return bitmap; 
        }


        public static BitmapData LockBits(this Bitmap bitmap)
        {
            return bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }

    }


}
