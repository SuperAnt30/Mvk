using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Модель курочки
    /// </summary>
    public class ModelChicken : ModelBase
    {
        protected ModelRender boxHead;
        protected ModelRender boxBill;
        protected ModelRender boxChin;
        protected ModelRender boxBody;
        protected ModelRender boxArmRight;
        protected ModelRender boxArmLeft;
        protected ModelRender boxLegRight;
        protected ModelRender boxLegLeft;

        public ModelChicken()
        {
            TextureSize = new vec2(64f, 32f);
            boxHead = new ModelRender(this, 0, 0);
            boxHead.SetBox(-2, -6, -2, 4, 6, 3, 0);
            boxHead.SetRotationPoint(0, 15, -4);
            boxBill = new ModelRender(this, 14, 0);
            boxBill.SetBox(-2, -4, -4, 4, 2, 2, 0);
            boxBill.SetRotationPoint(0, 15, -4);
            boxChin = new ModelRender(this, 14, 4);
            boxChin.SetBox(-1, -2, -3, 2, 2, 2, 0);
            boxChin.SetRotationPoint(0, 15, -4);

            boxBody = new ModelRender(this, 0, 9) { RotationPointY = 16 };
            boxBody.SetBox(-3, -4, -3, 6, 8, 6, 0);

            boxArmRight = new ModelRender(this, 24, 13);
            boxArmRight.SetBox(0, 0, -3, 1, 4, 6, 0);
            boxArmRight.SetRotationPoint(-4, 13, 1);
            boxArmLeft = new ModelRender(this, 24, 13);
            boxArmLeft.Mirror();
            boxArmLeft.SetBox(-1, 0, -3, 1, 4, 6, 0);
            boxArmLeft.SetRotationPoint(4, 13, 0);
            boxLegRight = new ModelRender(this, 26, 0);
            boxLegRight.SetBox(-1, 0, -3, 3, 5, 3, 0);
            boxLegRight.SetRotationPoint(-2, 19, 1);
            boxLegLeft = new ModelRender(this, 26, 0);
            boxLegLeft.Mirror();
            boxLegLeft.SetBox(-1, 0, -3, 3, 5, 3, 0);
            boxLegLeft.SetRotationPoint(1, 19, 1);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLRender.PushMatrix();

            if (entity.IsSneaking)
            {
                GLWindow.gl.Translate(0, .25f, 0);
            } else
            {
                GLWindow.gl.Translate(0, -.04f, 0);
            }

            boxHead.Render(scale);
            boxBill.Render(scale);
            boxChin.Render(scale);
            boxBody.Render(scale);
            boxArmRight.Render(scale);
            boxArmLeft.Render(scale);
            boxLegRight.Render(scale);
            boxLegLeft.Render(scale);

            GLRender.PopMatrix();
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxHead.RotateAngleY = headYaw;
            boxHead.RotateAngleX = -headPitch;
            boxLegRight.RotateAngleX = glm.cos(limbSwing * 2.6662f) * 1.4f * limbSwingAmount;
            boxLegLeft.RotateAngleX = glm.cos(limbSwing * 2.6662f + glm.pi) * 1.4f * limbSwingAmount;

            if (entity.OnGround)
            {
                boxArmRight.RotateAngleZ = 0;
                boxArmLeft.RotateAngleZ = 0;
            }
            else
            {
                boxArmRight.RotateAngleZ = glm.cos(ageInTicks * 2f + glm.pi) + glm.pi45 + .2f;
                boxArmLeft.RotateAngleZ = glm.cos(ageInTicks * 2f) - glm.pi45 - .2f;
            }
            boxBill.RotateAngleX = boxHead.RotateAngleX;
            boxBill.RotateAngleY = boxHead.RotateAngleY;
            boxChin.RotateAngleX = boxHead.RotateAngleX;
            boxChin.RotateAngleY = boxHead.RotateAngleY;
            boxBody.RotateAngleX = glm.pi90;

            if (entity.IsSneaking)
            {
                boxHead.RotationPointY = 16.5f;
                boxBill.RotationPointY = 16.5f;
                boxChin.RotationPointY = 16.5f;
                boxLegRight.RotationPointY = 15;
                boxLegLeft.RotationPointY = 15;
            }
            else
            {
                boxHead.RotationPointY = 14;
                boxBill.RotationPointY = 14;
                boxChin.RotationPointY = 14;
                boxLegRight.RotationPointY = 19;
                boxLegLeft.RotationPointY = 19;
            }
        }
    }
}
