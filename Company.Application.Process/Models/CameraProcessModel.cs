using Company.Application.Share.Interface;
using Company.Core.IOC;
using Company.Core.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Process.Models
{
    [ExposedService(Lifetime.Singleton,typeof(ICameraProcessModel))]
    public class CameraProcessModel : ReactiveObject, ICameraProcessModel
    {
        public DoubleBuffer<ColorBGRA> Buffer { get; set; } = new DoubleBuffer<ColorBGRA>();
        public UnmanagedArray2D<ColorBGRA> ImageData => Buffer.CurrentBuffer;
        public IObservable<UnmanagedArray2D<ColorBGRA>> ImageObserver { get; }
        public CameraProcessModel()
        {
            ImageObserver = this.WhenAnyValue(p => p.Buffer.CurrentBuffer);
        }
    }
}
