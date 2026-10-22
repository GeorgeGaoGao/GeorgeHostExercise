using Company.Core.Helper;
using Company.Core.Model;
using Company.Logger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Company.Hardware.Camera.None
{
    public class CameraNone : CameraBase
    {
        private UnmanagedArray2D<ColorBGRA> _data;
        public override bool DoClose()
        {
            return true;
        }

       

        public override bool DoInitial()
        {

            _data = new UnmanagedArray2D<ColorBGRA>(CameraConfig.Width, CameraConfig.Height);
            string filename = $"data\\{CameraConfig.Photo}";
            if (File.Exists(filename))
            {
                System.Drawing.Bitmap bitmap = ImageHelper.Load(filename);
                if (_data.Width != bitmap.Width || _data.Height != bitmap.Height)
                {
                    Logs.LogError(new Exception("系统设置中相机尺寸与实际相机分辨率不一致"));
                }
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                MemoryHelper.CopyMemory(_data.Header, data.Scan0, _data.Length);

                bitmap.UnlockBits(data);
                return true;
            }

            Thread.Sleep(1000);

            return false;

        }

        public override bool Trigger()
        {
            Task.Delay(50).ContinueWith(t =>
            {
                OnImageCaptured(new Photo(_data));
            });
            return true;
        }
    }
}
