using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using SharpGL;

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
        /// <summary>
        /// Позиция глаз
        /// </summary>
        private vec3 pos;
        private MovingObjectPosition movingObject;

        public RenderBlockCursor(Client client)
        {
            clientMain = client;
        }

        public void Render(MovingObjectPosition moving)
        {
           
            IsHidden = !moving.IsBlock();

            if (!IsHidden)
            {
                vec3 pos = clientMain.Player.GetPositionFrame();
                pos.y += clientMain.Player.GetEyeHeightFrame();
                if (!this.pos.Equals(pos))
                {
                    compiled = false;
                    this.pos = pos;
                }
                if (!moving.Equals(movingObject))
                {
                    compiled = false;
                    movingObject = moving;
                }
                Render();
            }
            
        }

        protected override void DoRender()
        {
            vec3 offset = clientMain.World.RenderEntityManager.CameraOffset * -1f; ;
            GLRender.Texture2DDisable();
            GLRender.LineWidth(2f);

            AxisAlignedBB[] axes = movingObject.Block.GetBlock().GetCollisionBoxesToList(
                movingObject.BlockPosition, movingObject.Block.Met());
            float dis = glm.distance(pos, movingObject.BlockPosition.ToVec3()) * .01f;
            dis *= dis;
            dis += 0.001f;
            //GLRender.DepthDisable();
            //GLRender.PushMatrix();
            //GLRender.Color(new vec4(1, 1, .5f, .2f));
            //foreach (AxisAlignedBB aabb in axes)
            //{
            //    GLRender.DrawOutlinedBoundingBox(aabb.Offset(offset).Expand(new vec3(dis)));
            //}
            //GLRender.PopMatrix();
            //GLRender.DepthEnable();
            GLRender.PushMatrix();
            GLRender.Color(new vec4(1, 1, .5f, .7f));
            foreach (AxisAlignedBB aabb in axes)
            {
                GLRender.DrawOutlinedBoundingBox(aabb.Offset(offset).Expand(new vec3(dis)));
            }
            GLRender.PopMatrix();

            //RenderSide(offset);

        }

        /// <summary>
        /// Рендер стороны
        /// </summary>
        private void RenderSide(vec3 offset)
        {
            Pole selectSide = movingObject.Side;
            GLRender.PushMatrix();
            GLRender.Translate(movingObject.BlockPosition.ToVec3() + offset);
            GLRender.LineWidth(4f);
            GLRender.Color(new vec4(1, .25f, .25f, 1));
            GLRender.Begin(OpenGL.GL_LINES);
            float min = -.01f;
            float max = 1.01f;
            if (selectSide == Pole.Up)
            {
                GLRender.Vertex(0, max, 0);
                GLRender.Vertex(1, max, 1);
                GLRender.Vertex(0, max, 1);
                GLRender.Vertex(1, max, 0);
            }
            else if (selectSide == Pole.Down)
            {
                GLRender.Vertex(0, min, 0);
                GLRender.Vertex(1, min, 1);
                GLRender.Vertex(0, min, 1);
                GLRender.Vertex(1, min, 0);
            }
            else if (selectSide == Pole.East)
            {
                GLRender.Vertex(max, 0, 0);
                GLRender.Vertex(max, 1, 1);
                GLRender.Vertex(max, 0, 1);
                GLRender.Vertex(max, 1, 0);
            }
            else if (selectSide == Pole.West)
            {
                GLRender.Vertex(min, 0, 0);
                GLRender.Vertex(min, 1, 1);
                GLRender.Vertex(min, 0, 1);
                GLRender.Vertex(min, 1, 0);
            }
            else if (selectSide == Pole.South)
            {
                GLRender.Vertex(0, 0, max);
                GLRender.Vertex(1, 1, max);
                GLRender.Vertex(0, 1, max);
                GLRender.Vertex(1, 0, max);
            }
            else if (selectSide == Pole.North)
            {
                GLRender.Vertex(0, 0, min);
                GLRender.Vertex(1, 1, min);
                GLRender.Vertex(0, 1, min);
                GLRender.Vertex(1, 0, min);
            }

            GLRender.End();
            GLRender.PopMatrix();
        }
    }
}
