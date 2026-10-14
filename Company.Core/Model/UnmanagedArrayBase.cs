using Company.Core.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Company.Core.Model
{
    public abstract class UnmanagedArrayBase : IDisposable
    {
        /// <summary>
        /// 图像数据的指针
        /// </summary>
        public IntPtr Header { get; private set; }
        /// <summary>
        /// 元素的数量
        /// </summary>
        public long Count { get; }
        /// <summary>
        /// 元素的大小
        /// </summary>
        private readonly int Size;

        public UnmanagedArrayBase(long count, int size,bool isResetMemory=true)
        {
            if (count <= 0) throw new ArgumentException("count必须大于零。");
            if (size <= 0) throw new ArgumentException("size必须大于零。");
           
            Count = count;
            Size = size;
            Header=Marshal.AllocHGlobal(new IntPtr(Length));
            if (isResetMemory)
            {
                MemoryHelper.ZeroMemory(Header, Length);
            }
        }
        ~UnmanagedArrayBase()
        {
            Dispose();
        }
        public long Length => Count * Size;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected bool IsDisposed;
        protected void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                //todo 清理托管内存。
            }

            if (Header != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Header);
                Header = IntPtr.Zero;
            }
            IsDisposed = true;
        }
    }
}
