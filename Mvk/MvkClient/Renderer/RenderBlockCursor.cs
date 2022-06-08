using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Рендер курсора блока
    /// </summary>
    public class RenderBlockCursor : RenderDL
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        private Client clientMain;
        private BlockBase selectBlock;
        private BlockPos selectBlockPos;
        private vec3 pos;

        public RenderBlockCursor(Client client)
        {
            clientMain = client;
        }

        public void Render(BlockBase selectBlock, BlockPos selectBlockPos)
        {
            IsHidden = selectBlock == null;

            if (!IsHidden)
            {
                vec3 pos = clientMain.Player.GetPositionFrame();
                pos.y += clientMain.Player.GetEyeHeightFrame();
                if (!this.pos.Equals(pos))
                {
                    compiled = false;
                    this.pos = pos;
                }
                if (this.selectBlock == null || !this.selectBlockPos.Equals(selectBlockPos))
                {
                    compiled = false;
                    this.selectBlock = selectBlock;
                    this.selectBlockPos = selectBlockPos;
                }
                Render();
            }
            
        }

        protected override void DoRender()
        {
            // Рамка хитбокса
            GLRender.PushMatrix();
            {
                vec3 offset = clientMain.World.RenderEntityManager.CameraOffset;
                GLRender.Texture2DDisable();
                GLRender.LineWidth(2f);

                AxisAlignedBB[] axes = clientMain.Player.SelectBlock.GetCollisionBoxesToList(selectBlockPos);
                float dis = glm.distance(pos, selectBlockPos.ToVec3()) * .01f;
                dis *= dis;
                dis += 0.001f;
                GLRender.DepthDisable();
                GLRender.PushMatrix();
                {
                    GLRender.Color(new vec4(1, 1, .5f, .2f));
                    foreach (AxisAlignedBB aabb in axes)
                    {
                        GLRender.DrawOutlinedBoundingBox(aabb.Offset(offset * -1f).Expand(new vec3(dis)));
                    }
                }
                GLRender.PopMatrix();
                GLRender.DepthEnable();
                GLRender.PushMatrix();
                {
                    GLRender.Color(new vec4(1, 1, .5f, .7f));
                    foreach (AxisAlignedBB aabb in axes)
                    {
                        GLRender.DrawOutlinedBoundingBox(aabb.Offset(offset * -1f).Expand(new vec3(dis)));
                    }
                }
                GLRender.PopMatrix();
            }
            GLRender.PopMatrix();

        }
    }
}
