using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using _2DRP_GUI._2DRP;
using _2DRP_GUI._2DRP.Model;

namespace _2DRP_GUI
{
    class GUIForm : Form
    {
        private Button inputBtn;
        private Label filePathLabel;
        private ListBox rectListBox;
        private Button calBtn;
        private Label totalAreaLabel;
        private Panel stripPanel;
        private Graphics stripCanvas;

        private ReadRects _readRects = new ReadRects();
        private TwoDRPSolver _2DRPSolver = new TwoDRPSolver();
        private BVNS2DRPSolver bVNS2DRPSolver = new BVNS2DRPSolver();
        private List<int> neighbourhoods = new List<int>();

        public delegate void FillRectListBoxDelegate();
        public delegate void PaintStripDelegate(TwoDRPSolver solver);

        private int stripPanelPadding = 10;
        private Font drawFont = new Font("Arial", 12);
        private Label resultLabel;
        private RadioButton heuristicRadioBtn;
        private RadioButton BVNSRadioBtn;
        private TextBox tMaxTextBox;
        private Label tMaxLabel;
        private GroupBox bFOptGroupBox;
        private RadioButton firstRadioBtn;
        private RadioButton bestRadioBtn;
        private GroupBox NeibourhoodGroupBox;
        private CheckBox n3CheckBox;
        private CheckBox n2CheckBox;
        private CheckBox n1CheckBox;
        private StringFormat drawFormat = new StringFormat();

        public int stripCanvasW
        {
            get
            {
                return stripPanel.Width - 2 * stripPanelPadding;
            }
        }

        public int stripCanvasH
        {
            get
            {
                return stripPanel.Height - 2 * stripPanelPadding;
            }
        }


        public GUIForm()
        {
            this.Text = "2DRP_GUI";
            InitializeComponent();
            bVNS2DRPSolver.TaskFinishedCallBack += OnFinishedBVNS;
            _2DRPSolver.TaskFinishedCallBack += OnFinishedRectsPacking;
            _readRects.TaskFinishedCallBack += OnFinistDataRead;//绑定完成任务要调用的委托
        }

        private void InitializeComponent()
        {
            this.inputBtn = new System.Windows.Forms.Button();
            this.calBtn = new System.Windows.Forms.Button();
            this.filePathLabel = new System.Windows.Forms.Label();
            this.rectListBox = new System.Windows.Forms.ListBox();
            this.totalAreaLabel = new System.Windows.Forms.Label();
            this.stripPanel = new System.Windows.Forms.Panel();
            this.resultLabel = new System.Windows.Forms.Label();
            this.heuristicRadioBtn = new System.Windows.Forms.RadioButton();
            this.BVNSRadioBtn = new System.Windows.Forms.RadioButton();
            this.tMaxTextBox = new System.Windows.Forms.TextBox();
            this.tMaxLabel = new System.Windows.Forms.Label();
            this.bFOptGroupBox = new System.Windows.Forms.GroupBox();
            this.firstRadioBtn = new System.Windows.Forms.RadioButton();
            this.bestRadioBtn = new System.Windows.Forms.RadioButton();
            this.NeibourhoodGroupBox = new System.Windows.Forms.GroupBox();
            this.n3CheckBox = new System.Windows.Forms.CheckBox();
            this.n2CheckBox = new System.Windows.Forms.CheckBox();
            this.n1CheckBox = new System.Windows.Forms.CheckBox();
            this.bFOptGroupBox.SuspendLayout();
            this.NeibourhoodGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputBtn
            // 
            this.inputBtn.Location = new System.Drawing.Point(12, 619);
            this.inputBtn.Name = "inputBtn";
            this.inputBtn.Size = new System.Drawing.Size(138, 23);
            this.inputBtn.TabIndex = 0;
            this.inputBtn.Text = "Data Input";
            this.inputBtn.UseVisualStyleBackColor = true;
            this.inputBtn.Click += new System.EventHandler(this.OnInputClick);
            // 
            // calBtn
            // 
            this.calBtn.Location = new System.Drawing.Point(12, 648);
            this.calBtn.Name = "calBtn";
            this.calBtn.Size = new System.Drawing.Size(138, 23);
            this.calBtn.TabIndex = 1;
            this.calBtn.Text = "Calculate";
            this.calBtn.UseVisualStyleBackColor = true;
            this.calBtn.Click += new System.EventHandler(this.OnCalClick);
            // 
            // filePathLabel
            // 
            this.filePathLabel.AutoSize = true;
            this.filePathLabel.Location = new System.Drawing.Point(190, 622);
            this.filePathLabel.Name = "filePathLabel";
            this.filePathLabel.Size = new System.Drawing.Size(0, 17);
            this.filePathLabel.TabIndex = 2;
            // 
            // rectListBox
            // 
            this.rectListBox.FormattingEnabled = true;
            this.rectListBox.ItemHeight = 17;
            this.rectListBox.Location = new System.Drawing.Point(12, 404);
            this.rectListBox.Name = "rectListBox";
            this.rectListBox.Size = new System.Drawing.Size(138, 208);
            this.rectListBox.TabIndex = 3;
            // 
            // totalAreaLabel
            // 
            this.totalAreaLabel.AutoSize = true;
            this.totalAreaLabel.Location = new System.Drawing.Point(12, 378);
            this.totalAreaLabel.Name = "totalAreaLabel";
            this.totalAreaLabel.Size = new System.Drawing.Size(0, 17);
            this.totalAreaLabel.TabIndex = 4;
            // 
            // stripPanel
            // 
            this.stripPanel.Location = new System.Drawing.Point(190, 12);
            this.stripPanel.Name = "stripPanel";
            this.stripPanel.Size = new System.Drawing.Size(800, 600);
            this.stripPanel.TabIndex = 5;
            // 
            // resultLabel
            // 
            this.resultLabel.AutoSize = true;
            this.resultLabel.Location = new System.Drawing.Point(12, 320);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(0, 17);
            this.resultLabel.TabIndex = 6;
            // 
            // heuristicRadioBtn
            // 
            this.heuristicRadioBtn.AutoSize = true;
            this.heuristicRadioBtn.Checked = true;
            this.heuristicRadioBtn.Location = new System.Drawing.Point(12, 12);
            this.heuristicRadioBtn.Name = "heuristicRadioBtn";
            this.heuristicRadioBtn.Size = new System.Drawing.Size(74, 21);
            this.heuristicRadioBtn.TabIndex = 7;
            this.heuristicRadioBtn.TabStop = true;
            this.heuristicRadioBtn.Text = "heuristic";
            this.heuristicRadioBtn.UseVisualStyleBackColor = true;
            // 
            // BVNSRadioBtn
            // 
            this.BVNSRadioBtn.AutoSize = true;
            this.BVNSRadioBtn.Location = new System.Drawing.Point(12, 39);
            this.BVNSRadioBtn.Name = "BVNSRadioBtn";
            this.BVNSRadioBtn.Size = new System.Drawing.Size(59, 21);
            this.BVNSRadioBtn.TabIndex = 8;
            this.BVNSRadioBtn.Text = "BVNS";
            this.BVNSRadioBtn.UseVisualStyleBackColor = true;
            // 
            // tMaxTextBox
            // 
            this.tMaxTextBox.Location = new System.Drawing.Point(57, 67);
            this.tMaxTextBox.Name = "tMaxTextBox";
            this.tMaxTextBox.Size = new System.Drawing.Size(61, 23);
            this.tMaxTextBox.TabIndex = 9;
            this.tMaxTextBox.Text = "5";
            // 
            // tMaxLabel
            // 
            this.tMaxLabel.AutoSize = true;
            this.tMaxLabel.Location = new System.Drawing.Point(12, 70);
            this.tMaxLabel.Name = "tMaxLabel";
            this.tMaxLabel.Size = new System.Drawing.Size(39, 17);
            this.tMaxLabel.TabIndex = 10;
            this.tMaxLabel.Text = "Tmax";
            // 
            // bFOptGroupBox
            // 
            this.bFOptGroupBox.Controls.Add(this.firstRadioBtn);
            this.bFOptGroupBox.Controls.Add(this.bestRadioBtn);
            this.bFOptGroupBox.Location = new System.Drawing.Point(12, 226);
            this.bFOptGroupBox.Name = "bFOptGroupBox";
            this.bFOptGroupBox.Size = new System.Drawing.Size(137, 71);
            this.bFOptGroupBox.TabIndex = 11;
            this.bFOptGroupBox.TabStop = false;
            // 
            // firstRadioBtn
            // 
            this.firstRadioBtn.AutoSize = true;
            this.firstRadioBtn.Location = new System.Drawing.Point(6, 38);
            this.firstRadioBtn.Name = "firstRadioBtn";
            this.firstRadioBtn.Size = new System.Drawing.Size(128, 21);
            this.firstRadioBtn.TabIndex = 0;
            this.firstRadioBtn.Text = "FirstImprovement";
            this.firstRadioBtn.UseVisualStyleBackColor = true;
            // 
            // bestRadioBtn
            // 
            this.bestRadioBtn.AutoSize = true;
            this.bestRadioBtn.Checked = true;
            this.bestRadioBtn.Location = new System.Drawing.Point(6, 11);
            this.bestRadioBtn.Name = "bestRadioBtn";
            this.bestRadioBtn.Size = new System.Drawing.Size(129, 21);
            this.bestRadioBtn.TabIndex = 0;
            this.bestRadioBtn.TabStop = true;
            this.bestRadioBtn.Text = "BestImprovement";
            this.bestRadioBtn.UseVisualStyleBackColor = true;
            // 
            // NeibourhoodGroupBox
            // 
            this.NeibourhoodGroupBox.Controls.Add(this.n3CheckBox);
            this.NeibourhoodGroupBox.Controls.Add(this.n2CheckBox);
            this.NeibourhoodGroupBox.Controls.Add(this.n1CheckBox);
            this.NeibourhoodGroupBox.Location = new System.Drawing.Point(12, 105);
            this.NeibourhoodGroupBox.Name = "NeibourhoodGroupBox";
            this.NeibourhoodGroupBox.Size = new System.Drawing.Size(156, 126);
            this.NeibourhoodGroupBox.TabIndex = 11;
            this.NeibourhoodGroupBox.TabStop = false;
            this.NeibourhoodGroupBox.Text = "Neighbourhoods";
            // 
            // n3CheckBox
            // 
            this.n3CheckBox.AutoSize = true;
            this.n3CheckBox.Location = new System.Drawing.Point(7, 87);
            this.n3CheckBox.Name = "n3CheckBox";
            this.n3CheckBox.Size = new System.Drawing.Size(107, 21);
            this.n3CheckBox.TabIndex = 0;
            this.n3CheckBox.Text = "Put2RectsFirst";
            this.n3CheckBox.UseVisualStyleBackColor = true;
            // 
            // n2CheckBox
            // 
            this.n2CheckBox.AutoSize = true;
            this.n2CheckBox.Checked = true;
            this.n2CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.n2CheckBox.Location = new System.Drawing.Point(6, 60);
            this.n2CheckBox.Name = "n2CheckBox";
            this.n2CheckBox.Size = new System.Drawing.Size(101, 21);
            this.n2CheckBox.TabIndex = 0;
            this.n2CheckBox.Text = "Switch2Rects";
            this.n2CheckBox.UseVisualStyleBackColor = true;
            // 
            // n1CheckBox
            // 
            this.n1CheckBox.AutoSize = true;
            this.n1CheckBox.Checked = true;
            this.n1CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.n1CheckBox.Location = new System.Drawing.Point(7, 33);
            this.n1CheckBox.Name = "n1CheckBox";
            this.n1CheckBox.Size = new System.Drawing.Size(106, 21);
            this.n1CheckBox.TabIndex = 0;
            this.n1CheckBox.Text = "FlipASegment";
            this.n1CheckBox.UseVisualStyleBackColor = true;
            // 
            // GUIForm
            // 
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1019, 700);
            this.Controls.Add(this.NeibourhoodGroupBox);
            this.Controls.Add(this.bFOptGroupBox);
            this.Controls.Add(this.tMaxLabel);
            this.Controls.Add(this.tMaxTextBox);
            this.Controls.Add(this.BVNSRadioBtn);
            this.Controls.Add(this.heuristicRadioBtn);
            this.Controls.Add(this.resultLabel);
            this.Controls.Add(this.stripPanel);
            this.Controls.Add(this.totalAreaLabel);
            this.Controls.Add(this.rectListBox);
            this.Controls.Add(this.filePathLabel);
            this.Controls.Add(this.calBtn);
            this.Controls.Add(this.inputBtn);
            this.Name = "GUIForm";
            this.bFOptGroupBox.ResumeLayout(false);
            this.bFOptGroupBox.PerformLayout();
            this.NeibourhoodGroupBox.ResumeLayout(false);
            this.NeibourhoodGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void OnCalClick(object sender, EventArgs e)
        {
            if (heuristicRadioBtn.Checked)
            {
                // running as background
                Thread thread = new Thread(new ParameterizedThreadStart(_2DRPSolver._2DRPSolver));
                thread.IsBackground = true;
                thread.Start(_readRects.rects);
            }
            else if (BVNSRadioBtn.Checked)
            {
                neighbourhoods.Clear();
                if (n1CheckBox.Checked) neighbourhoods.Add(1);
                if (n2CheckBox.Checked) neighbourhoods.Add(2);
                if (n3CheckBox.Checked) neighbourhoods.Add(3);
                float tMax = 1;
                float.TryParse(tMaxTextBox.Text, out tMax);
                this.Cursor = Cursors.WaitCursor;
                // running as background
                Thread thread = new Thread(new ParameterizedThreadStart(bVNS2DRPSolver.BVNS));
                thread.IsBackground = true;
                thread.Start(new BVNSParams(_readRects.rects, neighbourhoods, tMax, bestRadioBtn.Checked));
            }

        }

        private void OnFinishedRectsPacking()
        {
            //this.Invoke(new PaintStripDelegate(StripPaint), _2DRPSolver);
            this.Invoke((EventHandler)delegate
            {
                StripPaint(_2DRPSolver);

                //Data fill
                float percent = (float) _readRects.TotalArea / _2DRPSolver.heightOfPlacedRects / _2DRPSolver.W;
                string utilRate = percent.ToString("0.00%");
                resultLabel.Text = "H: " + _2DRPSolver.heightOfPlacedRects + " UtilRate: " + utilRate
                 + "\nElapsedT: " + _2DRPSolver.ElapsedT.ToString("0.0000") + "s";
            });
        }

        private void OnFinishedBVNS()
        {
            //this.Invoke(new PaintStripDelegate(StripPaint), vNS2DRPSolver._2DRPSolver);
            this.Invoke((EventHandler)delegate
            {
                StripPaint(bVNS2DRPSolver._2DRPSolver);
                //Data fill
                float percent = (float)_readRects.TotalArea / bVNS2DRPSolver._2DRPSolver.heightOfPlacedRects / bVNS2DRPSolver._2DRPSolver.W;
                string utilRate = percent.ToString("0.00%");
                resultLabel.Text = "H: " + bVNS2DRPSolver._2DRPSolver.heightOfPlacedRects + " UtilRate: " + utilRate 
                    + "\nElapsedT: " + bVNS2DRPSolver.ElapsedT.ToString("0.00") + "s\niteration: " + bVNS2DRPSolver.IterationCount;

                this.Cursor = Cursors.Default;
            });
        }

        private void OnInputClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "csv文件(*.*)|*.csv";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePathLabel.Text = dialog.FileName;
            }

            if (dialog.FileName.Length > 0)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(_readRects.Read));
                thread.IsBackground = true;
                thread.Start(dialog.FileName);
            }
        }

        private void OnFinistDataRead()
        {
            this.Invoke(new FillRectListBoxDelegate(FillRectListBox));
        }

        private void FillRectListBox()
        {
            rectListBox.Items.Clear();
            foreach (var rect in _readRects.rects)
            {
                rectListBox.Items.Add(rect);
            }
            totalAreaLabel.Text = "Total area: " + _readRects.TotalArea;
        }

        private void StripPaint(TwoDRPSolver solver)
        {
            if (stripCanvas == null)
            {
                stripCanvas = stripPanel.CreateGraphics();
            }
            stripCanvas.Clear(Color.White);

            float wRate = (float)solver.W / stripCanvasW;
            float hRate = (float)solver.heightOfPlacedRects / stripCanvasH;
            float stretchRate = wRate >= hRate ? wRate : hRate;
            
            //paint strip


            var color = Color.Black;
            Pen pen = new Pen(color);
            
            // paint strip
            var convertedW = Convert.ToInt32(solver.W / stretchRate);
            var convertedH = Convert.ToInt32(solver.heightOfPlacedRects / stretchRate);
            var uLCornerOfStripX = stripCanvasW / 2 - convertedW / 2 + stripPanelPadding;
            var uLCornerOfStripY = stripCanvasH - convertedH + stripPanelPadding;
            var strpRectangle =  new Rectangle(uLCornerOfStripX, uLCornerOfStripY, convertedW, convertedH);
            stripCanvas.DrawRectangle(pen, strpRectangle);

            var solidBrush = new SolidBrush(color);
            foreach (var placedRect in solver.placedRects)
            {
                var convertedRectW = Convert.ToInt32(placedRect.WidthAfterRotate / stretchRate);
                var convertedRectH = Convert.ToInt32(placedRect.HeightAfterRotate / stretchRate);
                var convertedRectX = Convert.ToInt32(placedRect.x / stretchRate);
                var convertedRectY = Convert.ToInt32(placedRect.y / stretchRate);
                var rectangle = new Rectangle(uLCornerOfStripX + convertedRectX, uLCornerOfStripY + (convertedH - convertedRectY - convertedRectH),
                    convertedRectW, convertedRectH);
                paintReplacedRect(pen, solidBrush, rectangle);
                solidBrush.Color = Color.Black;
                stripCanvas.DrawString(placedRect.id.ToString(), drawFont, solidBrush, rectangle.X, rectangle.Y);
            }

            solidBrush.Dispose();
            pen.Dispose();

        }

        private void paintReplacedRect(Pen pen, SolidBrush brush, Rectangle rectangle)
        {
            brush.Color = genRandomColor();
            stripCanvas.FillRectangle(brush, rectangle);
            stripCanvas.DrawRectangle(pen, rectangle);
        }

        private Color genRandomColor()
        {
            int R = new Random().Next(255);
            int G = new Random().Next(255);
            int B = new Random().Next(255);
            B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
            B = (B > 255) ? 255 : B;
            return Color.FromArgb(R, G, B);
        }

     }
}
