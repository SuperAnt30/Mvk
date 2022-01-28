using MvkServer.Entity;
using MvkServer.Glm;

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

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxHead.RotateAngleY = headYaw;
            boxHead.RotateAngleX = -headPitch;
            boxLegRight.RotateAngleX = glm.cos(limbSwing * 0.6662f) * 1.4f * limbSwingAmount;
            boxLegLeft.RotateAngleX = glm.cos(limbSwing * 0.6662f + glm.pi) * 1.4f * limbSwingAmount;
            boxArmRight.RotateAngleX = boxLegLeft.RotateAngleX * 0.5f;
            boxArmLeft.RotateAngleX = boxLegRight.RotateAngleX * 0.5f;

            if (entity.IsSneaking)
            {
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
                boxBody.RotateAngleX = 0f;
                boxLegRight.RotationPointZ = .1f;
                boxLegLeft.RotationPointZ = .1f;
                boxLegRight.RotationPointY = 12.0f;
                boxLegLeft.RotationPointY = 12.0f;
                boxHead.RotationPointY = 0.0f;
            }
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
