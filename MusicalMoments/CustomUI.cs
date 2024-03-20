using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MusicalMoments
{
    internal class CustomUI
    {
        public class CoverTabControl : TabControl
        {
            /// <summary>
            /// 解决系统TabControl多余边距问题
            /// </summary>
            public override Rectangle DisplayRectangle
            {
                get
                {
                    Rectangle rect = base.DisplayRectangle;
                    return new Rectangle(rect.Left - 4, rect.Top - 4, rect.Width + 8, rect.Height + 8);
                }
            }
        }
        public class BufferedListView : ListView
        {
            public BufferedListView()
            {
                // 开启双缓冲技术，减少闪烁
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                // 重绘背景，减少闪烁
                this.SetStyle(ControlStyles.ResizeRedraw, true);
                // 使用自定义的绘制
                this.OwnerDraw = true;
            }
        }
    }
}
