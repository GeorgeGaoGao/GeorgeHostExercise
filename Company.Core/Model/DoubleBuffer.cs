using Company.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Company.Core.Model
{
    enum BufferType
    {
        Buffer1,
        Buffer2
    }
    public sealed class DoubleBuffer<T> : INotifyPropertyChanged, IDisposable where T : struct
    {
        private UnmanagedArray2D<T> Buffer1 { get; set; }
        private UnmanagedArray2D<T> Buffer2 { get; set; }
        private UnmanagedArray2D<T> _currentBuffer;

        public UnmanagedArray2D<T> CurrentBuffer
        {
            get { return _currentBuffer; }
            set { _currentBuffer = value; RaisePropertyChanged(); }
        }

        private BufferType CurrentBufferType { get; set; } = BufferType.Buffer1;
        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Write(UnmanagedArray2D<T> buffer)
        {
            if (Buffer1==null)
            {
                CurrentBuffer = null;
                return;
            }
            Write(buffer.Header, buffer.Width, buffer.Height);
        }

        public void Write(nint header, int width, int height)
        {
            UnmanagedArray2D<T> target = null;
            BufferType targetType = BufferType.Buffer1;
            switch (CurrentBufferType)
            {
                case BufferType.Buffer1:
                    if (Buffer2==null)
                    {
                        Buffer2 = new UnmanagedArray2D<T>(width, height);
                    }
                    target = Buffer2;
                    targetType = BufferType.Buffer2;
                    break;
                case BufferType.Buffer2:
                    if (Buffer1==null)
                    {
                        Buffer1 = new UnmanagedArray2D<T>(width, height);
                    }
                    target = Buffer1;
                    targetType = BufferType.Buffer1;
                    break;
                default:
                    break;
            }
            MemoryHelper.CopyMemory(target.Header, header, target.Length);
            CurrentBufferType = targetType;
        }

        public void Dispose()
        {
            Buffer1.Dispose();
            Buffer2.Dispose();
        }
    }
}
