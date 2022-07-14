using MvkAssets;
using MvkClient.Renderer.Block;
using MvkServer.World.Block;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Рендер блока для GUI
    /// </summary>
    public class RenderBlockGui : RenderDL
    {
        /// <summary>
        /// Тип блока
        /// </summary>
        private readonly EnumBlock enumBlock;

        public RenderBlockGui(EnumBlock enumBlock) => this.enumBlock = enumBlock;

        public void Render(int x, int y, float scale)
        {
            GLRender.PushMatrix();
            GLRender.Translate(x, y, 0);
            GLRender.Rotate(-20, 1, 0, 0);
            GLRender.Rotate(-45, 0, 1, 0);
            GLRender.Scale(scale, -scale, scale);
            Render();
            GLRender.PopMatrix();
        }

        protected override void DoRender()
        {
            if (enumBlock == EnumBlock.Air)
            {
                IsHidden = true;
                return;
            }
            BlockGuiRender render = new BlockGuiRender(Blocks.GetBlockCache(enumBlock));

            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Atlas);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.PushMatrix();
            {
                GLRender.DepthDisable();
                render.RenderVBOtoDL();
                GLRender.DepthEnable();
            }
            GLRender.PopMatrix();
        }
    }
}
