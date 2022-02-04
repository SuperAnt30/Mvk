using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Renderer.Model
{
    public class ModelPlayer : ModelBase
    {
        protected ModelRender boxHead;
        protected ModelRender boxBody;
        protected ModelRender boxArmRight;
        protected ModelRender boxArmLeft;
        protected ModelRender boxLegRight;
        protected ModelRender boxLegLeft;

        public ModelPlayer()
        {
            float y = 0;
            boxHead = new ModelRender(this, 0, 0) { RotationPointY = y };
            boxHead.SetBox(-4, -8, -4, 8, 8, 8, 0);

            boxBody = new ModelRender(this, 16, 16) { RotationPointY = y };
            boxBody.SetBox(-4, 0, -2, 8, 12, 4, 0);
            boxArmRight = new ModelRender(this, 40, 16);
            boxArmRight.SetBox(-3, -2, -2, 4, 12, 4, 0);
            boxArmRight.SetRotationPoint(-5, 2 + y, 0);
            boxArmLeft = new ModelRender(this, 40, 16);
            boxArmLeft.Mirror();
            boxArmLeft.SetBox(-1, -2, -2, 4, 12, 4, 0);
            boxArmLeft.SetRotationPoint(5, 2 + y, 0);
            boxLegRight = new ModelRender(this, 0, 16);
            boxLegRight.SetBox(-2, 0, -2, 4, 12, 4, 0);
            boxLegRight.SetRotationPoint(-1.9f, 12 + y, 0);
            boxLegLeft = new ModelRender(this, 0, 16);
            boxLegLeft.Mirror();
            boxLegLeft.SetBox(-2, 0, -2, 4, 12, 4, 0);
            boxLegLeft.SetRotationPoint(1.9f, 12 + y, 0);

            
            
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLWindow.gl.PushMatrix();

            if (entity.IsSneaking)
            {
                GLWindow.gl.Translate(0, .3f, 0);
            }

            boxHead.Render(scale);
            boxBody.Render(scale);
            boxArmRight.Render(scale);
            boxArmLeft.Render(scale);
            boxLegRight.Render(scale);
            boxLegLeft.Render(scale);

            GLWindow.gl.PopMatrix();
        }

        protected float swingProgress2;
        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxBody.RotateAngleY = 0;
            boxHead.RotateAngleY = headYaw;
            boxHead.RotateAngleX = -headPitch;
            boxLegRight.RotateAngleX = glm.cos(limbSwing * 0.6662f) * 1.4f * limbSwingAmount;
            boxLegLeft.RotateAngleX = glm.cos(limbSwing * 0.6662f + glm.pi) * 1.4f * limbSwingAmount;
            boxArmRight.RotateAngleX = boxLegLeft.RotateAngleX * 0.5f;
            boxArmLeft.RotateAngleX = boxLegRight.RotateAngleX * 0.5f;
            boxArmRight.RotateAngleY = 0.0f;
            boxArmRight.RotateAngleZ = 0.0f;
            boxArmLeft.RotateAngleY = 0.0f;
            boxArmLeft.RotateAngleZ = 0.0f;
            

            //if (Сидим в транспорте)
            //{
            //    boxArmRight.RotateAngleX += -(glm.pi / 5f);
            //    boxArmLeft.RotateAngleX += -(glm.pi / 5f);
            //    boxLegRight.RotateAngleX = -(glm.pi * 2f / 5f);
            //    boxLegLeft.RotateAngleX = -(glm.pi * 2f / 5f);
            //    boxLegRight.RotateAngleY = (glm.pi / 10f);
            //    boxLegLeft.RotateAngleY = -(glm.pi / 10f);
            //}

            // В правой руке что-то
            //boxArmRight.RotateAngleX = boxArmRight.RotateAngleX * .5f - glm.pi / 10f;
            // В левой руке что-то
            boxArmLeft.RotateAngleX = boxArmLeft.RotateAngleX * .5f - glm.pi / 10f;
            // boxArmRight.RotateAngleY = -.524f;

            // Удар рукой
            if (SwingProgress > 0)
            {
                float swingProgress = SwingProgress;
                float sp = swingProgress;
                boxBody.RotateAngleY = glm.sin(Mth.Sqrt(sp) * glm.pi360) * .2f;
                boxArmRight.RotateAngleY += boxBody.RotateAngleY;
                boxArmLeft.RotateAngleY += boxBody.RotateAngleY;
                boxArmLeft.RotateAngleX += boxBody.RotateAngleY;
                sp = 1f - swingProgress;
                sp *= sp;
                sp *= sp;
                sp = 1f - sp;
                float s1 = glm.sin(sp * glm.pi);
                float s2 = glm.sin(swingProgress * glm.pi) * -(boxHead.RotateAngleX - .7f) * .75f;
                boxArmRight.RotateAngleX = boxArmRight.RotateAngleX - (s1 * 1.2f + s2);
                boxArmRight.RotateAngleY += boxBody.RotateAngleY * 2f;
                boxArmRight.RotateAngleZ += glm.sin(swingProgress * glm.pi) * -.4f;
            }

            // Вращение рук в зависимости от торса
            boxArmRight.RotationPointZ = glm.sin(boxBody.RotateAngleY) * 5f;
            boxArmRight.RotationPointX = -glm.cos(boxBody.RotateAngleY) * 5f;
            boxArmLeft.RotationPointZ = -glm.sin(boxBody.RotateAngleY) * 5f;
            boxArmLeft.RotationPointX = glm.cos(boxBody.RotateAngleY) * 5f;

            if (entity.IsSneaking)
            {
                // Положение сидя
                boxBody.RotateAngleX = 0.5f;
                boxArmRight.RotateAngleX += 0.4f;
                boxArmLeft.RotateAngleX += 0.4f;
                boxLegRight.RotationPointZ = 4.0f;
                boxLegLeft.RotationPointZ = 4.0f;
                boxLegRight.RotationPointY = 9.0f;
                boxLegLeft.RotationPointY = 9.0f;
                boxHead.RotationPointY = 1.0f;
            }
            else
            {
                // Положение стоя
                boxBody.RotateAngleX = 0f;
                boxLegRight.RotationPointZ = .1f;
                boxLegLeft.RotationPointZ = .1f;
                boxLegRight.RotationPointY = 12.0f;
                boxLegLeft.RotationPointY = 12.0f;
                boxHead.RotationPointY = 0.0f;
            }

            // Движение рук от дыхания
            boxArmRight.RotateAngleZ += glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
            boxArmLeft.RotateAngleZ -= glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
            boxArmRight.RotateAngleX += glm.sin(ageInTicks * 0.067f) * 0.05f;
            boxArmLeft.RotateAngleX -= glm.sin(ageInTicks * 0.067f) * 0.05f;

            //boxArmRight.RotateAngleX = boxLegLeft.RotateAngleX * .5f;
            //boxArmLeft.RotateAngleX = boxLegRight.RotateAngleX * .5f;

            //boxLegRight.RotationPointX = .1f;
            //boxLegLeft.RotationPointX = -.1f;

            //this.bipedRightLeg.rotateAngleX = MathHelper.cos(p_78087_1_ * 0.6662F) * 1.4F * p_78087_2_;
            //this.bipedLeftLeg.rotateAngleX = MathHelper.cos(p_78087_1_ * 0.6662F + (float)Math.PI) * 1.4F * p_78087_2_;

            //boxLegRight.RotationPointZ = 0.1f;
            //boxLegLeft.RotationPointZ = 0.1f;
            //boxLegRight.RotationPointY = 12.0f;
            //boxLegLeft.RotationPointY = 12.0f;

            //Head.RotateAngleX = headPitch / (180f / glm.pi);
            //Head.RotateAngleY = headYaw / (180f / glm.pi);
            ////Head.RotateAngleX = (float)System.Math.Cos(ageInTicks * 0.6662f);// glm.cos(ageInTicks);// glm.pi90;
            ////Head.RotateAngleX = glm.cos(t);// glm.cos(ageInTicks);// glm.pi90;
            ////Head.RotateAngleY = glm.pi45;
            //Bill.RotateAngleX = Head.RotateAngleX;
            //Bill.RotateAngleY = Head.RotateAngleY;
            //Chin.RotateAngleX = Head.RotateAngleX;
            //Chin.RotateAngleY = Head.RotateAngleY;
            //Body.RotateAngleX = glm.pi90;
            //RightLeg.RotateAngleX = glm.cos(limbSwing * 0.6662f) * 1.4f * limbSwingAmount;
            //LeftLeg.RotateAngleX = glm.cos(limbSwing * 0.6662f + glm.pi) * 1.4f * limbSwingAmount;
            //RightWing.RotateAngleZ = glm.cos(ageInTicks * 2f + glm.pi) + glm.pi45 + .2f;
            //LeftWing.RotateAngleZ = glm.cos(ageInTicks * 2f) - glm.pi45 - .2f;
            //RightWing.RotateAngleZ = ageInTicks;
            //LeftWing.RotateAngleZ = -ageInTicks;
        }
    }
}
