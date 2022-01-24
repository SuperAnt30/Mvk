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

        public ModelChicken(bool sneak, bool onGround) : base(sneak, onGround)
        {
            TextureSize = new vec2(64f, 32f);
            float y = 0;
            boxHead = new ModelRender(this, 0, 0);
            boxHead.SetBox(-2, -6, -2, 4, 6, 3);
            boxHead.SetRotationPoint(0, 15 + y, -4);
            boxBill = new ModelRender(this, 14, 0);
            boxBill.SetBox(-2, -4, -4, 4, 2, 2);
            boxBill.SetRotationPoint(0, 15 + y, -4);
            boxChin = new ModelRender(this, 14, 4);
            boxChin.SetBox(-1, -2, -3, 2, 2, 2);
            boxChin.SetRotationPoint(0, 15 + y, -4);

            boxBody = new ModelRender(this, 0, 9) { RotationPointY = y + 16 };
            boxBody.SetBox(-3, -4, -3, 6, 8, 6);

            boxArmRight = new ModelRender(this, 24, 13);
            boxArmRight.SetBox(0, 0, -3, 1, 4, 6);
            boxArmRight.SetRotationPoint(-4, 13 + y, 1);
            boxArmLeft = new ModelRender(this, 24, 13);
            boxArmLeft.SetBox(-1, 0, -3, 1, 4, 6);
            boxArmLeft.SetRotationPoint(4, 13 + y, 0);
            boxLegRight = new ModelRender(this, 26, 0);
            boxLegRight.SetBox(-1, 0, -3, 3, 5, 3);
            boxLegRight.SetRotationPoint(-2, 19 + y, 1);
            boxLegLeft = new ModelRender(this, 26, 0);
            boxLegLeft.SetBox(-1, 0, -3, 3, 5, 3);
            boxLegLeft.SetRotationPoint(1, 19 + y, 1);

            //boxArmLeft.Mirror();
            //boxLegLeft.Mirror();
        }

        public override void Render(float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLWindow.gl.PushMatrix();

            if (isSneak)
            {
                GLWindow.gl.Translate(0, .3f, 0);
            } else
            {
                GLWindow.gl.Translate(0, -.01f, 0);
            }

            boxHead.Render(scale);
            boxBill.Render(scale);
            boxChin.Render(scale);
            boxBody.Render(scale);
            boxArmRight.Render(scale);
            boxArmLeft.Render(scale);
            boxLegRight.Render(scale);
            boxLegLeft.Render(scale);

            GLWindow.gl.PopMatrix();
        }

        protected override void SetRotationAngles(float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxHead.RotateAngleY = headYaw;
            boxHead.RotateAngleX = -headPitch;
            boxLegRight.RotateAngleX = glm.cos(limbSwing * 0.6662f) * 1.4f * limbSwingAmount;
            boxLegLeft.RotateAngleX = glm.cos(limbSwing * 0.6662f + glm.pi) * 1.4f * limbSwingAmount;

            if (onGround)
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

            //if (isSneak)
            //{
            //    boxBody.RotateAngleX = 0.5f;
            //    boxArmRight.RotateAngleX += 0.4f;
            //    boxArmLeft.RotateAngleX += 0.4f;
            //    boxLegRight.RotationPointZ = 4.0f;
            //    boxLegLeft.RotationPointZ = 4.0f;
            //    boxLegRight.RotationPointY = 9.0f;
            //    boxLegLeft.RotationPointY = 9.0f;
            //    boxHead.RotationPointY = 1.0f;
            //}
            //else
            //{
            //    boxBody.RotateAngleX = 0f;
            //    boxLegRight.RotationPointZ = .1f;
            //    boxLegLeft.RotationPointZ = .1f;
            //    boxLegRight.RotationPointY = 12.0f;
            //    boxLegLeft.RotationPointY = 12.0f;
            //    boxHead.RotationPointY = 0.0f;
            //}


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
