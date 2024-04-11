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
            try{ NeteaseCryptClass = CreateNeteaseCrypt(FileName); }
            catch(Exception ex) { MessageBox.Show($"未在运行目录找到\"taurusxin.LibNcmDump.dll\"文件 这是ncm格式转为mp3的重要文件 请从压缩包内完整解压至同一目录 \r\n\r\n详情错误信息:\r\n{ex}","错误"); return; }
        }
        /// <summary>
        /// 启动转换过程。
        /// </summary>
        /// <returns>返回一个整数，指示转储过程的结果。如果成功，返回0；如果失败，返回1。</returns>
        public int Dump()
        {
            try { return Dump(NeteaseCryptClass); }
            catch { return 1; }
        }
        /// <summary>
        /// 修复音乐文件元数据。
        /// </summary>
        public void FixMetadata()
        {
            try { FixMetadata(NeteaseCryptClass); }
            catch { return; }
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
