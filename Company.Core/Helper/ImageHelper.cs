using Company.Core.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Company.Core.Helper
{
    public static class ImageHelper
    {
        public static Bitmap Load(string path)
        {
            byte[] bytes = FileHelper.Load(path);
            using(MemoryStream ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms) as Bitmap;
            }
        }

        public static UnmanagedArray2D<ushort> LoadTiff(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
