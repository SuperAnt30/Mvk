using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.Item;
using System;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер сущности вещи
    /// </summary>
    public class RenderEntityItem : RenderEntityBase
    {
        private readonly RenderItem item;
        private Random rand;

        public RenderEntityItem(RenderManager renderManager, RenderItem item) : base(renderManager)
        {
            this.item = item;
            shadowSize = .15f;
            shadowOpaque = .75f;
        }


        public void DoRender(EntityItem entity, vec3 offset, float timeIndex)
        {
            ItemStack stack = entity.Stack;
           // rand = new Random(187);

            item.Render(stack);
            base.DoRender(entity, offset, timeIndex);
        }



    }
}
