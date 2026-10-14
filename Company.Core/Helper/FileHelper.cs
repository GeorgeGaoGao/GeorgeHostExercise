using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Company.Core.Helper
{
    public static class FileHelper
    {
        public static byte[] Load(string path)
        {
            if (File.Exists(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }
            else
            {
                throw new Exception($"{path}不存在。");
            }

        }
    }
}
