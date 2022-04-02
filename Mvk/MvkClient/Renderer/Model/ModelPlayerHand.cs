using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    public class ModelPlayerHand : ModelBase
    {
        public ModelRender BoxArmRight { get; protected set; }

        public ModelPlayerHand()
        {
            BoxArmRight = new ModelRender(this, 40, 16);
            BoxArmRight.SetBox(-2, 0, -2, 4, 12, 4, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            BoxArmRight.Render(scale);
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            BoxArmRight.RotationPointZ = 0;
            BoxArmRight.RotateAngleX = glm.pi270;

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
                BoxArmRight.RotateAngleX -= s1 * .8f;// + s2 * .5f;
                //boxArmRight.RotateAngleY = glm.sin(Mth.Sqrt(sp) * glm.pi) * .4f;
                //boxArmRight.RotateAngleZ = s2 * -.4f;
                BoxArmRight.RotationPointX = -4 * sp; // -6
                BoxArmRight.RotationPointY = -4 * sp; // -6
            }
            else
            {
                BoxArmRight.RotateAngleY = glm.pi45;
                BoxArmRight.RotateAngleZ = 0f;
                BoxArmRight.RotationPointX = 0;
                BoxArmRight.RotationPointY = 0;

                // Движение рук от дыхания
                BoxArmRight.RotateAngleZ += glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
                BoxArmRight.RotateAngleX += glm.sin(ageInTicks * 0.067f) * 0.05f;
            }
        }
    }
}
