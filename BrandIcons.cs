using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
namespace TailscaleRouting
{
    public static class BrandIcons
    {
        [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
        public static Bitmap Draw(int size, bool active)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent); g.ScaleTransform(size / 256f, size / 256f);
                for (int row=0;row<3;row++) for(int col=0;col<3;col++)
                {
                    float x=25+col*77, y=25+row*77;
                    if(active)
                        for(int spread=18;spread>=2;spread-=2)
                            using(var glow = new SolidBrush(Color.FromArgb(13,0,132,255)))
                                g.FillEllipse(glow,x-spread,y-spread,52+spread*2,52+spread*2);
                    using(var fill = new SolidBrush(active ? Color.FromArgb(49,169,255) : Color.FromArgb(137,150,166)))
                        g.FillEllipse(fill,x,y,52,52);
                    if(active) using(var edge = new Pen(Color.FromArgb(166,225,255),2)) g.DrawEllipse(edge,x+1,y+1,50,50);
                }
            }
            return bitmap;
        }
        public static Icon Create(bool active)
        {
            using(var bitmap=Draw(64,active))
            {
                IntPtr handle=bitmap.GetHicon();
                try { using(var icon=Icon.FromHandle(handle)) return (Icon)icon.Clone(); }
                finally { DestroyIcon(handle); }
            }
        }
    }
}
