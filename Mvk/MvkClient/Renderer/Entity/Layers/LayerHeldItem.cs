using MvkAssets;
using MvkClient.Renderer.Model;
using MvkServer.Entity;
using MvkServer.Item;
using MvkServer.Item.List;

namespace MvkClient.Renderer.Entity.Layers
{
    public class LayerHeldItem : ILayerRenderer
    {
        /// <summary>
        /// Модель к которой привязывается предмет, правая рука как равило
        /// </summary>
        private readonly ModelRender modelRender;
        /// <summary>
        /// Рендер предмета
        /// </summary>
        private readonly RenderItem renderItem;
        /// <summary>
        /// Вид с глаз
        /// </summary>
        private readonly bool fromEyes = false;

        public LayerHeldItem(ModelRender modelRender, RenderItem renderItem, bool fromEyes)
        {
            this.modelRender = modelRender;
            this.renderItem = renderItem;
            this.fromEyes = fromEyes;
        }

        public void DoRenderLayer(EntityLiving entity, float limbSwing, float limbSwingAmount, float timeIndex, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            ItemStack itemStack = entity.GetHeldItem();

            if (itemStack != null)
            {
                GLRender.Texture2DEnable();
                TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Atlas);
                GLWindow.Texture.BindTexture(ts.GetKey());
                GLRender.PushMatrix();

                ItemBase item = itemStack.Item;
                if (entity.IsSneaking()) GLRender.Translate(0, .2f, 0);

                // Дублирование поворотов и смещения модели
                modelRender.PostRender(.0625f);
                GLRender.Translate(-.0625f, .4375f, .0625f);

                if (item is ItemBlock)
                {
                    GLRender.Translate(0, .1875f, -.3125f);
                    GLRender.Rotate(20, 1, 0, 0);
                    GLRender.Rotate(45, 0, 1, 0);
                    if (fromEyes) GLRender.Rotate(90, 1, 0, 0);
                    float size = 0.375F;
                    GLRender.Scale(-size, -size, size);
                }

                // Прорисовка предмета
                renderItem.Render(item);
                GLRender.PopMatrix();
            }
        }
    }
}
