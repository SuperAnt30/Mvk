using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Renderer.Model
{
    public class ModelPlayerHand : ModelBase
    {
        protected ModelRender boxArmRight;

        public ModelPlayerHand()
        {
            float y = 0;
            boxArmRight = new ModelRender(this, 40, 16);
            boxArmRight.SetBox(-3, -2, -2, 4, 12, 4, 0);
            boxArmRight.SetRotationPoint(-5, 2 + y, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLWindow.gl.PushMatrix();

            if (entity.IsSneaking)
            {
                GLWindow.gl.Translate(0, .3f, 0);
            }
            boxArmRight.Render(scale * .5f);

            GLWindow.gl.PopMatrix();
        }

        protected float swingProgress2;
        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxArmRight.RotateAngleX = -.9f;//glm.cos(limbSwing * 0.6662f + glm.pi) * 1.4f * limbSwingAmount * 0.5f;
            boxArmRight.RotateAngleY = -.1f;
            boxArmRight.RotateAngleZ = -1.2f;
            boxArmRight.RotationPointY = 2;

            // В правой руке что-то
            //boxArmRight.RotateAngleX = boxArmRight.RotateAngleX * .5f - glm.pi / 10f;

            float bhrx = headYaw;
            // Удар рукой
            if (SwingProgress > 0)
            {
                float swingProgress = SwingProgress;
                float sp = swingProgress;
                bhrx = glm.sin(Mth.Sqrt(sp) * glm.pi360) * .2f;
                boxArmRight.RotateAngleY += bhrx;
                sp = 1f - swingProgress;
                sp *= sp;
                sp *= sp;
                sp = 1f - sp;
                float s1 = glm.sin(sp * glm.pi);
              //  float s2 = glm.sin(swingProgress * glm.pi) * -(headYaw - .7f) * .75f;
                boxArmRight.RotateAngleX = boxArmRight.RotateAngleX - (s1 * 1.2f);// + s2);
                boxArmRight.RotateAngleY += bhrx * 2f;
                boxArmRight.RotateAngleZ += glm.sin(swingProgress * glm.pi) * -.4f;
            }

            // Вращение рук в зависимости от торса
            boxArmRight.RotationPointZ = glm.sin(bhrx) * 5f;
            boxArmRight.RotationPointX = -glm.cos(bhrx) * 5f;

            // Движение рук от дыхания
            boxArmRight.RotateAngleZ += glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
            boxArmRight.RotateAngleX += glm.sin(ageInTicks * 0.067f) * 0.05f;
        }
    }
}
