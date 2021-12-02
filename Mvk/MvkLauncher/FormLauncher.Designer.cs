namespace MvkLauncher
{
    partial class FormLauncher
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.openGLControl1 = new SharpGL.OpenGLControl();
            ((System.ComponentModel.ISupportInitialize)(this.openGLControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // openGLControl1
            // 
            this.openGLControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openGLControl1.DrawFPS = false;
            this.openGLControl1.Location = new System.Drawing.Point(0, 0);
            this.openGLControl1.Name = "openGLControl1";
            this.openGLControl1.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            this.openGLControl1.RenderContextType = SharpGL.RenderContextType.NativeWindow;
            this.openGLControl1.RenderTrigger = SharpGL.RenderTrigger.Manual;
            this.openGLControl1.Size = new System.Drawing.Size(1284, 691);
            this.openGLControl1.TabIndex = 0;
            this.openGLControl1.OpenGLInitialized += new System.EventHandler(this.OpenGLControl1_OpenGLInitialized);
            this.openGLControl1.OpenGLDraw += new SharpGL.RenderEventHandler(this.OpenGLControl1_OpenGLDraw);
            this.openGLControl1.Resized += new System.EventHandler(this.OpenGLControl1_Resized);
            this.openGLControl1.Enter += new System.EventHandler(this.OpenGLControl1_Enter);
            this.openGLControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OpenGLControl1_KeyDown);
            this.openGLControl1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OpenGLControl1_KeyUp);
            this.openGLControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OpenGLControl1_MouseDown);
            this.openGLControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OpenGLControl1_MouseMove);
            this.openGLControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OpenGLControl1_MouseUp);
            this.openGLControl1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.OpenGLControl1_PreviewKeyDown);
            // 
            // FormLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 691);
            this.Controls.Add(this.openGLControl1);
            this.Name = "FormLauncher";
            this.Text = "Малювекi";
            this.Deactivate += new System.EventHandler(this.FormLauncher_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLauncher_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLauncher_FormClosed);
            this.Load += new System.EventHandler(this.FormLauncher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.openGLControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private SharpGL.OpenGLControl openGLControl1;
    }
}

