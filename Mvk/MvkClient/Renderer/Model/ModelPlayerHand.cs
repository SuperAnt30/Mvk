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
            boxArmRight = new ModelRender(this, 40, 16);
            boxArmRight.SetBox(-2, 0, -2, 4, 12, 4, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);

            GLWindow.gl.PushMatrix();
            {
                // Смещение руки примерно чтоб при ударе не виден разрыв, и в пасиве не сильно мешала
                GLWindow.gl.Translate(-0.5f, 1.01f, -0.5f);
                // Руку поворачиваем чтоб видеи ребро
                GLRender.Rotate(65, 0, 0, 1);

                boxArmRight.Render(scale);
            }
            GLWindow.gl.PopMatrix();
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxArmRight.RotationPointZ = 0;
            boxArmRight.RotateAngleX = glm.pi270;

            if (SwingProgress > 0)
            {
                // Удар правой руки
                //float inv = 1.0f - SwingProgress;
                //float sp = 1.0f - inv * inv;
                //float s1 = glm.sin(sp * glm.pi);
                //float s2 = glm.sin(SwingProgress * glm.pi);
                //boxArmRight.RotateAngleX -= s1 * .6f + s2;
                //boxArmRight.RotateAngleZ = glm.pi45 * inv;
                //boxArmRight.RotationPointX = -4 * inv;
                //boxArmRight.RotationPointY = -6 * inv;

                // Удар правой руки
                float inv = 1.0f - SwingProgress;
                float sp = inv * inv;
                float s1 = glm.sin(sp);
                float s2 = glm.sin(SwingProgress);
                boxArmRight.RotateAngleX -= s1 * .8f;// + s2 * .5f;
                //boxArmRight.RotateAngleY = glm.sin(Mth.Sqrt(sp) * glm.pi) * .4f;
                //boxArmRight.RotateAngleZ = s2 * -.4f;
                boxArmRight.RotationPointX = -4 * sp; // -6
                boxArmRight.RotationPointY = -4 * sp; // -6
            }
            else
            {
                boxArmRight.RotateAngleY = glm.pi45;
                boxArmRight.RotateAngleZ = 0f;
                boxArmRight.RotationPointX = 0;
                boxArmRight.RotationPointY = 0;

                // Движение рук от дыхания
                boxArmRight.RotateAngleZ += glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
                boxArmRight.RotateAngleX += glm.sin(ageInTicks * 0.067f) * 0.05f;
            }
        }
    }
}
