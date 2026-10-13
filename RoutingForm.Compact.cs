using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TailscaleRouting
{
    public sealed partial class RoutingForm
    {
        TableLayoutPanel compactLayout;
        Button journalToggle;
        Label summary;
        ContextMenuStrip ruleMenu;
        bool journalExpanded;
        int journalHeight;
        Size compactMinimum;

        void ApplyCompactLayout()
        {
            SuspendLayout();
            var oldLayout=Controls[0];
            editor.Parent=null; log.Parent=null; state.Parent=null;
            var fields=(TableLayoutPanel)editor.Controls[0];
            var oldButtons=fields.GetControlFromPosition(0,1);
            fields.Controls.Remove(oldButtons); oldButtons.Dispose();
            fields.RowStyles[1].Height=38;

            ruleMenu=new ContextMenuStrip();
            ruleMenu.Items.Add("Переместить выше",null,delegate { MoveRule(-1); });
            ruleMenu.Items.Add("Переместить ниже",null,delegate { MoveRule(1); });
            ruleMenu.Items.Add("Добавить .ru / .рф / .su",null,delegate { foreach(string domain in new[]{"ru","рф","su"}) Add(new RoutingRule {Value=domain}); });
            ruleMenu.Items.Add(new ToolStripSeparator());
            ruleMenu.Items.Add("Помощь",null,delegate { HelpForm.Open(this); });
            var more=MakeButton("Ещё ···",delegate {});
            more.Click+=delegate { ruleMenu.Show(more,new Point(0,more.Height)); };
            fields.Controls.Add(UiLayout.ButtonRow(
                MakeButton("+ Добавить",delegate { Add(new RoutingRule()); }),
                MakeButton("Удалить",delegate { if(grid.CurrentRow!=null) grid.Rows.Remove(grid.CurrentRow); }),
                MakeButton("Импорт…",delegate { ImportRules(); }),more),0,1);

            compactLayout=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(14,10,14,8),ColumnCount=1,RowCount=7};
            compactLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            foreach(float height in new[]{94f,38f,-1f,27f,0f,44f,25f})
                compactLayout.RowStyles.Add(new RowStyle(height<0 ? SizeType.Percent : SizeType.Absolute,height<0 ? 100 : height));
            var banner=new RoutingBanner {Dock=DockStyle.Fill,Margin=Padding.Empty};
            var brand=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(12,10,10,10),ColumnCount=3,RowCount=2,Margin=Padding.Empty};
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,55));
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,158));
            brand.RowStyles.Add(new RowStyle(SizeType.Percent,65));brand.RowStyles.Add(new RowStyle(SizeType.Percent,35));
            logo=new PictureBox {Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,Margin=new Padding(0,8,9,10)};
            brand.Controls.Add(logo,0,0);brand.SetRowSpan(logo,2);
            var name=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.LeftToRight,WrapContents=false,Margin=Padding.Empty,Padding=new Padding(0,4,0,0)};
            var first=new Label {Text="Tailscale",AutoSize=true,Font=new Font("Segoe UI",19,FontStyle.Bold),Margin=Padding.Empty};
            var second=new Label {Text="Routing",AutoSize=true,Font=new Font("Segoe UI",19,FontStyle.Bold),Margin=new Padding(4,0,0,0)};
            name.Controls.Add(first);name.Controls.Add(second);brand.Controls.Add(name,1,0);
            var tagline=new Label {Text="Твой трафик. Твои маршруты.",Dock=DockStyle.Fill,Font=new Font("Segoe UI",9),Margin=Padding.Empty};
            brand.Controls.Add(tagline,1,1);
            state.Margin=new Padding(3,6,0,6);state.TextAlign=ContentAlignment.MiddleCenter;
            state.Font=new Font("Segoe UI",9,FontStyle.Bold);brand.Controls.Add(state,2,0);
            var author=new Label {Text="v1.0.0   /   pb_coder",Dock=DockStyle.Fill,TextAlign=ContentAlignment.TopCenter,Font=new Font("Segoe UI",8.5F),Margin=Padding.Empty};
            brand.Controls.Add(author,2,1);banner.Controls.Add(brand);compactLayout.Controls.Add(banner,0,0);
            var explanation=new Label {Text="Исключения → напрямую    ·    Остальное → Tailscale",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Margin=Padding.Empty};
            compactLayout.Controls.Add(explanation,0,1);
            editor.Margin=Padding.Empty;compactLayout.Controls.Add(editor,0,2);
            journalToggle=MakeButton("›  Журнал",delegate { SetJournalExpanded(!journalExpanded); });
            journalToggle.Dock=DockStyle.Fill;journalToggle.TextAlign=ContentAlignment.MiddleLeft;journalToggle.Margin=Padding.Empty;
            compactLayout.Controls.Add(journalToggle,0,3);
            log.Dock=DockStyle.Fill;log.Margin=new Padding(0,3,0,3);log.Visible=false;compactLayout.Controls.Add(log,0,4);
            save.Parent=null;check.Parent=null;start.Parent=null;stop.Parent=null;
            compactLayout.Controls.Add(UiLayout.ButtonRow(check,save,stop,start),0,5);
            summary=new Label {Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true,Margin=Padding.Empty};
            compactLayout.Controls.Add(summary,0,6);
            Controls.Remove(oldLayout);oldLayout.Dispose();Controls.Add(compactLayout);
            AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);
            MinimumSize=new Size(700,430);Size=new Size(740,470);compactMinimum=MinimumSize;
            grid.Columns["enabled"].FillWeight=10;grid.Columns["enabled"].MinimumWidth=40;
            grid.Columns["kind"].FillWeight=37;grid.Columns["kind"].MinimumWidth=151;
            grid.Columns["value"].FillWeight=76;
            grid.Columns["pickProcess"].FillWeight=20;grid.Columns["pickProcess"].MinimumWidth=86;
            AppTheme.Apply(this);AppTheme.Primary(start);AppTheme.Menu(ruleMenu);
            grid.Columns["value"].DefaultCellStyle.BackColor=Color.FromArgb(9,30,48);
            log.ForeColor=AppTheme.Muted;
            SetTransparent(brand);first.ForeColor=Color.White;second.ForeColor=AppTheme.Blue;
            tagline.ForeColor=author.ForeColor=AppTheme.Muted;
            state.BackColor=Color.FromArgb(8,31,49);state.ForeColor=AppTheme.Blue;
            summary.ForeColor=AppTheme.Muted;explanation.ForeColor=Color.FromArgb(172,199,219);
            journalToggle.FlatAppearance.BorderSize=0;journalToggle.BackColor=AppTheme.Background;journalToggle.ForeColor=AppTheme.Muted;
            grid.Paint+=delegate(object sender,PaintEventArgs e) {
                if(grid.Rows.Count==0) TextRenderer.DrawText(e.Graphics,"Добавь сайт, IP или программу\nдля прямого подключения",Font,
                    new Rectangle(0,grid.ColumnHeadersHeight+16,grid.Width,Math.Max(48,grid.Height-grid.ColumnHeadersHeight-28)),AppTheme.Muted,
                    TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
            };
            ResumeLayout(true);
        }
        static void SetTransparent(Control parent)
        {
            parent.BackColor=Color.Transparent;
            foreach(Control child in parent.Controls) SetTransparent(child);
        }
        void SetJournalExpanded(bool expanded)
        {
            if(compactLayout==null || journalExpanded==expanded) return;
            if(journalHeight==0) journalHeight=(int)Math.Round(96*CreateGraphicsDpi()/96f);
            journalExpanded=expanded;
            int targetHeight=Height+(expanded ? journalHeight : -journalHeight);
            MinimumSize=new Size(compactMinimum.Width,compactMinimum.Height+(expanded ? journalHeight : 0));
            compactLayout.RowStyles[4].Height=expanded ? journalHeight : 0;
            log.Visible=expanded;journalToggle.Text=expanded ? "⌄  Журнал" : "›  Журнал";
            if(WindowState==FormWindowState.Normal) Height=Math.Max(MinimumSize.Height,targetHeight);
        }
        float CreateGraphicsDpi() { using(var graphics=CreateGraphics()) return graphics.DpiY; }
    }
    sealed class RoutingBanner:Panel
    {
        public RoutingBanner() {DoubleBuffered=true;ResizeRedraw=true;}
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
            using(var fill=new LinearGradientBrush(ClientRectangle,Color.FromArgb(4,15,28),Color.FromArgb(5,28,48),25))g.FillRectangle(fill,ClientRectangle);
            float w=Width,h=Height;
            using(var line=new Pen(Color.FromArgb(75,18,142,221),1))
            {
                for(int i=0;i<4;i++) g.DrawBezier(line,w*.43f,h+i*14,w*.58f,h*.2f,w*.76f,h*1.3f,w,h*(.12f+i*.14f));
                g.DrawArc(line,w*.5f,h*.67f,w*.8f,h*2.7f,180,180);
            }
            foreach(var point in new[]{new PointF(w*.56f,h*.87f),new PointF(w*.84f,h*.78f),new PointF(w*.97f,h*.34f)})
            {
                for(int r=9;r>1;r-=2)using(var halo=new SolidBrush(Color.FromArgb(17,0,155,255)))g.FillEllipse(halo,point.X-r,point.Y-r,r*2,r*2);
                using(var dot=new SolidBrush(Color.FromArgb(131,219,255)))g.FillEllipse(dot,point.X-2,point.Y-2,4,4);
            }
            using(var border=new Pen(Color.FromArgb(20,70,104)))g.DrawRectangle(border,0,0,Width-1,Height-1);
        }
    }
}
