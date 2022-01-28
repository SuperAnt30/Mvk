//using MvkAssets;
//using MvkClient.Entity;
//using MvkClient.Renderer.Entity;
//using MvkClient.Renderer.Model;
//using MvkClient.World;
//using MvkServer.Glm;
//using MvkServer.Util;
//using SharpGL;

//namespace MvkClient.Renderer
//{
//    public class Render
//    {

//        public void DrawEntitiesNew(WorldClient world, float timeIndex, float ageInTicks)
//        {
//            world.Player.CameraMatrixProjection();
//            //GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
//            //TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Steve);
//            //GLWindow.Texture.BindTexture(ts.GetKey());
//            //GLWindow.gl.Color(1f, 1f, 1f, 1f);

//            //if (world.Player.ViewCamera != EnumViewCamera.Eye) DrawEntityNew(world.RenderEntityManager.GetEntityRenderObject(), world.Player, timeIndex, ageInTicks);

//            //foreach (EntityPlayerClient entity in world.PlayerEntities.Values)
//            //{
//            //    if (entity.Name != world.Player.Name)
//            //    {
//            //        DrawEntityNew(world.RenderEntityManager.GetEntityRenderObject(), entity, entity.TimeIndex(), ageInTicks);
//            //    }
//            //}
//        }

//        protected void DrawEntityNew(RendererLivingEntity render, EntityPlayerClient entity, float timeIndex, float ageInTicks)
//        {
//            render.DoRender(entity, new vec3(0), timeIndex);
//            //vec3 pos = entity.GetPositionFrame(timeIndex);
//            ////ModelChicken modelPlayer = new ModelChicken(entity.IsSneaking, entity.OnGround);
//            //GLWindow.gl.PushMatrix();
//            //{
//            //    GLWindow.gl.Translate(pos.x, pos.y, pos.z);
//            //    GLWindow.gl.PushMatrix();
//            //    {
//            //        // соотношение высоты 3.6, к цельной модели 2.0, 3.6/2.0 = 1.8
//            //        float scale = 1.8f;
//            //        GLWindow.gl.Scale(scale, scale, scale);
//            //        GLWindow.gl.Translate(0, 1.508f, 0);
//            //        GLWindow.gl.Rotate(180f, 0, 0, 1);
//            //        float yaw = entity.GetRotationYawBodyFrame(timeIndex);
//            //        GLWindow.gl.Rotate(glm.degrees(yaw), 0, 1, 0);
//            //        yaw -= entity.GetRotationYawFrame(timeIndex);

//            //       // float limbSwing = entity.LimbSwing - 

//            //        render.Render(entity, entity.LimbSwing, entity.GetLimbSwingAmountFrame(timeIndex), ageInTicks,
//            //            -yaw, entity.GetRotationPitchFrame(timeIndex), .0625f);
//            //    }
//            //    GLWindow.gl.PopMatrix();
//            //}
//            //GLWindow.gl.PopMatrix();
//        }



//        //public void DrawEntities(WorldClient world, float timeIndex, float ageInTicks)
//        //{
//        //    world.Player.DrawMatrixProjection();

//        //    GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
//        //    TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Steve);
//        //    GLWindow.Texture.BindTexture(ts.GetKey());
//        //    GLWindow.gl.Color(1f, 1f, 1f, 1f);
//        //    //ModelPlayer
//        //    if (world.Player.ViewCamera != EnumViewCamera.Eye) DrawEntity(new ModelPlayer(world.Player.IsSneaking, world.Player.OnGround), world.Player, timeIndex, ageInTicks);

//        //    //ts = GLWindow.Texture.GetData(AssetsTexture.Chicken);
//        //    //GLWindow.Texture.BindTexture(ts.GetKey());

//        //    foreach (EntityPlayerClient entity in world.PlayerEntities.Values)
//        //    {
//        //        if (entity.Name != world.Player.Name) DrawEntity(new ModelPlayer(entity.IsSneaking, entity.OnGround), entity, entity.TimeIndex(), ageInTicks);
//        //    }

//        //}

//        //protected void DrawEntity(ModelBase model, EntityPlayerClient entity, float timeIndex, float ageInTicks)
//        //{
//        //    vec3 pos = entity.GetPositionFrame(timeIndex);
//        //    //ModelChicken modelPlayer = new ModelChicken(entity.IsSneaking, entity.OnGround);
//        //    GLWindow.gl.PushMatrix();
//        //    {
//        //        GLWindow.gl.Translate(pos.x, pos.y, pos.z);
//        //        GLWindow.gl.PushMatrix();
//        //        {
//        //            // соотношение высоты 3.6, к цельной модели 2.0, 3.6/2.0 = 1.8
//        //            float scale = 1.8f;
//        //            GLWindow.gl.Scale(scale, scale, scale);
//        //            GLWindow.gl.Translate(0, 1.5f, 0);
//        //            GLWindow.gl.Rotate(180f, 0, 0, 1);
//        //            float yaw = entity.GetRotationYawBodyFrame(timeIndex);
//        //            GLWindow.gl.Rotate(glm.degrees(yaw), 0, 1, 0);
//        //            yaw -= entity.GetRotationYawFrame(timeIndex);
//        //            model.Render(entity.LimbSwing, entity.GetLimbSwingAmountFrame(timeIndex), ageInTicks,
//        //                -yaw, entity.GetRotationPitchFrame(timeIndex), .0625f);
//        //        }
//        //        GLWindow.gl.PopMatrix();
//        //    }
//        //    GLWindow.gl.PopMatrix();
//        //}


      
//    }
//}
