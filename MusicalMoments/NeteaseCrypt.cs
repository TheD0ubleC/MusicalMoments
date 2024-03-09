using System;
using System.Runtime.InteropServices;
namespace MusicalMoments
{
    public class NeteaseCrypt : IDisposable
    {
        const string DLL_PATH = $"taurusxin.LibNcmDump.dll";
        [DllImport(DLL_PATH)]
        private static extern IntPtr CreateNeteaseCrypt(string path);
        [DllImport(DLL_PATH)]
        private static extern int Dump(IntPtr NeteaseCrypt);
        [DllImport(DLL_PATH)]
        private static extern void FixMetadata(IntPtr NeteaseCrypt);
        [DllImport(DLL_PATH)]
        private static extern void DestroyNeteaseCrypt(IntPtr NeteaseCrypt);
        private IntPtr NeteaseCryptClass = IntPtr.Zero;
        /// <summary>
        /// 创建 NeteaseCrypt 类的实例。
        /// </summary>
        /// <param name="FileName">网易云音乐 ncm 加密文件路径</param>
        public NeteaseCrypt(string FileName)
        {
            NeteaseCryptClass = CreateNeteaseCrypt(FileName);
        }
        /// <summary>
        /// 启动转换过程。
        /// </summary>
        /// <returns>返回一个整数，指示转储过程的结果。如果成功，返回0；如果失败，返回1。</returns>
        public int Dump()
        {
            return Dump(NeteaseCryptClass);
        }
        /// <summary>
        /// 修复音乐文件元数据。
        /// </summary>
        public void FixMetadata()
        {
            FixMetadata(NeteaseCryptClass);
        }
        // 实现 IDisposable 接口，释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (NeteaseCryptClass != IntPtr.Zero)
            {
                DestroyNeteaseCrypt(NeteaseCryptClass);
                NeteaseCryptClass = IntPtr.Zero;
            }
        }
        ~NeteaseCrypt()
        {
            Dispose(false);
        }
    }
}
