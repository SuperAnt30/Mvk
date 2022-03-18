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

        public RenderBlockGui(EnumBlock enumBlock, float scale)
        {
            this.scale = scale;
            this.enumBlock = enumBlock;
        }

        public void Render(int x, int y)
        {
            SetRotationPoint(x / scale, y / scale, 0);
            Render();
        }

        protected override void DoRender()
        {
            if (enumBlock == EnumBlock.Air)
            {
                IsHidden = true;
                return;
            }

            BlockRender blockRender = new BlockRender(null, Blocks.GetBlock(enumBlock));

            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Atlas);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.PushMatrix();
            {
                GLRender.Scale(scale);
                blockRender.RenderGui();
            }
            GLRender.PopMatrix();
        }
    }
}
