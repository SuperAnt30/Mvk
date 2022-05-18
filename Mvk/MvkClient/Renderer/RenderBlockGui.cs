using MvkAssets;
using MvkClient.Renderer.Block;
using MvkServer.Glm;
using MvkServer.Util;
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
            angle--;
            if (angle < 0) angle = 360;
            RotateAngleX = glm.radians(-20);
            RotateAngleY = glm.radians(angle);
            SetRotationPoint(x / scale, y / scale + (glm.cos(RotateAngleY) * .25f), 0);
            
            //RotateAngleY = glm.radians(-45);
            Render();
        }

        float angle = 0f;

        protected override void DoRender()
        {
            if (enumBlock == EnumBlock.Air)
            {
                IsHidden = true;
                return;
            }
            BlockRender blockRender = new BlockRender(Blocks.GetBlockCache(enumBlock), new BlockPos());

            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Atlas);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.PushMatrix();
            {
                GLRender.Scale(scale, -scale, scale);
                GLRender.DepthDisable();
                blockRender.RenderVBOtoDL();
                GLRender.DepthEnable();
            }
            GLRender.PopMatrix();
        }

        protected override void ToListCall()
        {
            bool rotation = RotationPointX == 0f && RotationPointY == 0f && RotationPointZ == 0f;

            if (RotateAngleX == 0f && RotateAngleY == 0f && RotateAngleZ == 0f)
            {
                if (!rotation)
                {
                    GLRender.PushMatrix();
                    GLRender.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                }
                GLRender.ListCall(dList);
                if (!rotation) GLRender.PopMatrix();
            }
            else
            {
                GLRender.PushMatrix();
                if (!rotation) GLRender.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                if (RotateAngleX != 0f) GLRender.Rotate(glm.degrees(RotateAngleX), 1, 0, 0);
                if (RotateAngleZ != 0f) GLRender.Rotate(glm.degrees(RotateAngleZ), 0, 0, 1);
                if (RotateAngleY != 0f) GLRender.Rotate(glm.degrees(RotateAngleY), 0, 1, 0); // Для вращения
                GLRender.ListCall(dList);
                GLRender.PopMatrix();
            }
        }

    }
}
