using System;
using System.Drawing;
using System.Windows.Forms;
namespace TailscaleRouting
{
    public sealed partial class RoutingForm
    {
        NotifyIcon tray;
        Icon inactiveIcon, activeIcon;
        Bitmap inactiveLogo, activeLogo;
        PictureBox logo;
        ToolStripMenuItem toggleItem, exitItem;
        ContextMenuStrip trayMenu;
        bool exitRequested;
        bool? brandedActive;
        void InitializeTray()
        {
            inactiveIcon=BrandIcons.Create(false); activeIcon=BrandIcons.Create(true);
            inactiveLogo=BrandIcons.Draw(80,false); activeLogo=BrandIcons.Draw(80,true);
            logo.Image=inactiveLogo;
            trayMenu=new ContextMenuStrip();
            trayMenu.Items.Add("Открыть Tailscale Routing",null,delegate { RestoreWindow(); });
            toggleItem=new ToolStripMenuItem("Включить маршрутизацию",null,delegate { ToggleRouting(); });
            trayMenu.Items.Add(toggleItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            exitItem=new ToolStripMenuItem("Выход",null,delegate { if(!busy && !externalBusy) { exitRequested=true; Close(); } });
            trayMenu.Items.Add(exitItem); AppTheme.Menu(trayMenu);
            tray=new NotifyIcon {Icon=inactiveIcon,Text="Tailscale Routing · выключено",ContextMenuStrip=trayMenu,Visible=true};
            tray.DoubleClick+=delegate { RestoreWindow(); };
            Resize+=delegate { if(WindowState==FormWindowState.Minimized) Hide(); };
        }
        void RestoreWindow() { Show(); WindowState=FormWindowState.Normal; Activate(); }
        void UpdateBrand()
        {
            if(tray==null) return;
            bool active=controller.Tunnel && controller.Alive;
            if(brandedActive!=active)
            {
                Icon=active ? activeIcon : inactiveIcon; tray.Icon=Icon;
                logo.Image=active ? activeLogo : inactiveLogo; brandedActive=active;
            }
            tray.Text=active ? "Tailscale Routing · правила работают" : "Tailscale Routing · выключено";
            toggleItem.Text=controller.Active ? "Остановить маршрутизацию" : "Включить маршрутизацию";
            toggleItem.Enabled=exitItem.Enabled=!busy && !externalBusy;
        }
        void DisposeTray()
        {
            if(tray!=null) { tray.Visible=false; tray.Dispose(); tray=null; }
            if(trayMenu!=null) trayMenu.Dispose();
            if(logo!=null) logo.Image=null;
            Icon=null;
            if(inactiveIcon!=null) inactiveIcon.Dispose(); if(activeIcon!=null) activeIcon.Dispose();
            if(inactiveLogo!=null) inactiveLogo.Dispose(); if(activeLogo!=null) activeLogo.Dispose();
        }
    }
}
