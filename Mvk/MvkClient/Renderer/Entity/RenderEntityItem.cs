using MvkAssets;
using MvkServer.Entity;
using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.Item;
using System;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер сущности предмета
    /// </summary>
    public class RenderEntityItem : RenderEntityBase
    {
        private readonly RenderItem item;
        private Random rand;
        //private int begin;

        public RenderEntityItem(RenderManager renderManager, RenderItem item) : base(renderManager)
        {
            this.item = item;
            shadowSize = .15f;
            shadowOpaque = .75f;
            //begin = renderManager.World.Rand.Next(360);
        }

       // float fff;

        //List<float> list = new List<float>();
        public override void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (entity is EntityItem entityItem)
            {
                ItemStack stack = entityItem.GetEntityItemStack();
                vec3 pos = entity.GetPositionFrame(timeIndex);
                vec3 offsetPos = pos - offset;
                rand = new Random(entity.Id);
                int begin = rand.Next(360);
                rand = new Random(187);
                int count = CountItem(stack);

                GLRender.Texture2DEnable();
                TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Atlas);
                GLWindow.Texture.BindTexture(ts.GetKey());

                GLRender.PushMatrix();
                {
                    GLRender.Translate(offsetPos.x, offsetPos.y, offsetPos.z);

                    float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex + begin;
                                                                                              // float yaw = glm.cos(ageInTicks * .025f) * glm.pi ;// * .025f;
                    float yaw = ageInTicks * 2f;
                    float height = glm.cos(glm.radians(yaw)) * .16f + .5f;
                    //  list.Add(glm.degrees(yaw));
                    //fff++;
                    //if (fff > 180) fff = -180f;
                    //GLRender.Rotate(fff, 0, 1, 0);
                    GLRender.Rotate(yaw, 0, 1, 0);
                    // GLRender.Rotate(glm.degrees(yaw), 0, 1, 0);
                    for (int i = 0; i < count; i++)
                    {
                        GLRender.PushMatrix();
                        {
                            float x = ((float)rand.NextDouble() * 2f - 1f) * .15f;
                            float y = ((float)rand.NextDouble() * 2f - 1f) * .15f;
                            float z = ((float)rand.NextDouble() * 2f - 1f) * .15f;

                            // GLRender.Translate(offsetPos.x + x, offsetPos.y + y + height, offsetPos.z + z);
                            GLRender.Translate(x, y + height, z);
                            GLRender.Scale(.5f);
                            
                            item.Render(stack);
                        }
                        GLRender.PopMatrix();
                    }
                }
                GLRender.PopMatrix();
                base.DoRender(entity, offset, timeIndex);
            }
            
            
        }

        /// <summary>
        /// Количество предметов в зависимости от количества 
        /// </summary>
        private int CountItem(ItemStack itemStack)
        {
            int amount = itemStack.Amount;
            if (amount > 47) return 5;
            if (amount > 31) return 4;
            if (amount > 15) return 3;
            if (amount > 1) return 2;
            return 1;
        }


    }
}
