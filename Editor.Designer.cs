namespace GK1_PolygonEditor
{
    partial class PolygonEditor
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolygonEditor));
            panel2 = new Panel();
            panel4 = new Panel();
            c_Canvas = new Canvas();
            panel3 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            groupBox1 = new GroupBox();
            rb_Library = new RadioButton();
            rb_Bresenham = new RadioButton();
            panel2.SuspendLayout();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(panel3);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(752, 393);
            panel2.TabIndex = 1;
            // 
            // panel4
            // 
            panel4.Controls.Add(c_Canvas);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(0, 0);
            panel4.Margin = new Padding(0);
            panel4.Name = "panel4";
            panel4.Size = new Size(577, 393);
            panel4.TabIndex = 1;
            // 
            // c_Canvas
            // 
            c_Canvas.Dock = DockStyle.Fill;
            c_Canvas.Location = new Point(0, 0);
            c_Canvas.Margin = new Padding(0);
            c_Canvas.Name = "c_Canvas";
            c_Canvas.Size = new Size(577, 393);
            c_Canvas.TabIndex = 0;
            c_Canvas.Text = "canvas1";
            c_Canvas.MouseDown += c_Canvas_MouseDown;
            c_Canvas.MouseMove += c_Canvas_MouseMove;
            c_Canvas.MouseUp += c_Canvas_MouseUp;
            // 
            // panel3
            // 
            panel3.AutoSize = true;
            panel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel3.Controls.Add(flowLayoutPanel1);
            panel3.Dock = DockStyle.Right;
            panel3.Location = new Point(577, 0);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(175, 393);
            panel3.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(groupBox1);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(10);
            flowLayoutPanel1.Size = new Size(175, 105);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(rb_Library);
            groupBox1.Controls.Add(rb_Bresenham);
            groupBox1.Location = new Point(10, 10);
            groupBox1.Margin = new Padding(0);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 3, 3, 0);
            groupBox1.Size = new Size(155, 85);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Renderer";
            // 
            // rb_Library
            // 
            rb_Library.AutoSize = true;
            rb_Library.Location = new Point(6, 22);
            rb_Library.Name = "rb_Library";
            rb_Library.Size = new Size(142, 19);
            rb_Library.TabIndex = 0;
            rb_Library.TabStop = true;
            rb_Library.Text = "Library";
            rb_Library.UseVisualStyleBackColor = true;
            // 
            // rb_Bresenham
            // 
            rb_Bresenham.AutoSize = true;
            rb_Bresenham.Location = new Point(6, 47);
            rb_Bresenham.Name = "rb_Bresenham";
            rb_Bresenham.Size = new Size(143, 19);
            rb_Bresenham.TabIndex = 1;
            rb_Bresenham.TabStop = true;
            rb_Bresenham.Text = "Bresenham";
            rb_Bresenham.UseVisualStyleBackColor = true;
            // 
            // PolygonEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(752, 393);
            Controls.Add(panel2);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(768, 432);
            Name = "PolygonEditor";
            Text = "Polygon Editor";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel4.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel2;
        private Panel panel4;
        private Panel panel3;
        private FlowLayoutPanel flowLayoutPanel1;
        private GroupBox groupBox1;
        private RadioButton rb_Library;
        private RadioButton rb_Bresenham;
        private Canvas c_Canvas;
    }
}
