using System.Windows.Forms;

namespace MvkLauncher
{
    public partial class FormLauncher : Form
    {
        public FormLauncher()
        {
            InitializeComponent();
        }

        private void FormLauncher_Load(object sender, System.EventArgs e)
        {
            MvkClient.Client.TestSound();
        }

        private void openGLControl1_OpenGLInitialized(object sender, System.EventArgs e)
        {
            MvkClient.Client.TestOpenGL(openGLControl1.OpenGL);
        }

        private void openGLControl1_OpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            MvkClient.Client.TestOpenGLDraw(openGLControl1.OpenGL);
        }

        private void openGLControl1_Resized(object sender, System.EventArgs e)
        {

        }
    }
}
