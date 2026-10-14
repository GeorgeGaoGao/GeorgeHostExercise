using Company.Core.Helper;
using Company.Core.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Imaging.Effects;
using System.IO;
using System.Text;

namespace Company.Hardware.Detector.None
{
    public class DetectorNone : DetectorBase
    {
        private Task _task;
        private UnmanagedArray2D<ushort> _data;
        public override bool DoClose()
        {
            IsGrabbing = false;
            return true;
        }

        public override bool DoGrab()
        {
            if (_task==null||_task.IsCompleted)
            {
                IsGrabbing = true;
                _task = Task.Factory.StartNew(RunGrab, TaskCreationOptions.LongRunning);
            }
            return true;
        }

        private void RunGrab()
        {
            while (IsGrabbing)
            {
                if (_data.Width!=DetectorConfig.Width||_data.Height!=DetectorConfig.Height)
                {
                    throw new Exception($"系统设置中的平板探测器的分辨率与实际分辨率不相同");
                }
                using (var temp=_data.DeepClone())
                {
                    OnImageCaptured(new DetectorImage(temp));
                }
                Thread.Sleep(50);
            }
        }

        public override bool DoInitial()
        {
            
            string filename = $"data\\{DetectorConfig.Photo}";
            if (File.Exists(filename))
            {
             _data=ImageHelper.LoadTiff(filename);
            }
            else
            {
                throw new Exception($"File {filename} does not exist.");
            }
            _data=_data ?? new UnmanagedArray2D<ushort>(DetectorConfig.Width, DetectorConfig.Height);
            Thread.Sleep(1000);
            return true;
        }
    }
}
