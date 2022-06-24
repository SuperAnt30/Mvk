using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World.Block;
using SharpGL;

namespace MvkClient.Gui
{
    /// <summary>
    /// Скрин во время игры
    /// Тут всё игрока, инвентарь на экране, ХП и другие опции, эффект воды и прочего
    /// </summary>
    public class ScreenInGame : Screen
    {
        protected Button buttonSingle;

        public ScreenInGame(Client client) : base(client)
        {
            background = EnumBackground.Game;

            buttonSingle = new Button(EnumScreenKey.SinglePlayer, Language.T("gui.singleplayer")) { Width = 300 };
            InitButtonClick(buttonSingle);


            Initialize();

        }

        protected override void Init()
        {
           // AddControls(buttonSingle);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = Height / 4 + 92 * sizeInterface;
            int hMax = h + 208 * sizeInterface;
            if (hMax > Height) h -= hMax - Height;

            buttonSingle.Position = new vec2i(100 * sizeInterface, h);
        }

        float ppp = 0;

        /// <summary>
        /// Прорисовка
        /// </summary>
        /// <param name="timeIndex">Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1, где 0 это начало, 1 финиш</param>
        public void Draw(float timeIndex)
        {
            //vec3 pos = ClientMain.Player.GetPositionFrame(timeIndex);
            //pos.y += ClientMain.Player.GetEyeHeight();
            sizeInterface = Setitings.Setting.SizeInterface;

            // базовая прорисовка контролов, скорее всего тут будет чат
            base.Draw();

            // Прицел
            if (ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
            {
                
                GLRender.PushMatrix();
                GLRender.Translate(Width / 2, Height / 2, 0);
                GLRender.Scale(sizeInterface, sizeInterface, 1);
                GLRender.Texture2DEnable();
                GLWindow.Texture.BindTexture(AssetsTexture.Icons);
                GLRender.Color(1f);
                GLRender.Rectangle(-16, -16, 16, 16, .9375f, .9375f, 1f, 1f);
                GLRender.PopMatrix();
            }

            // Эффект урона
            DrawEffDamage(ClientMain.Player.DamageTime, timeIndex);
            // Эффект воды если надо
            DrawEffWater(timeIndex);



            GLRender.PushMatrix();
            GLRender.Translate(Width / 2, Height, 0);
            GLRender.Scale(sizeInterface, sizeInterface, 1);
            GLRender.Texture2DEnable();

            // Инвенатрь
            DrawInventory();
            // Уровень
            DrawLevel();

            GLRender.PopMatrix();

            // ХП
          //  DrawHealth(timeIndex);

        }

        /// <summary>
        /// Прорисовка уровня
        /// </summary>
        private void DrawLevel()
        {
            ppp += .01f;
            if (ppp > 1f) ppp = 0;
            // Уровень интерполяции, 0 .. 1
            float level = ppp;

            GLWindow.Texture.BindTexture(AssetsTexture.Icons);
            GLRender.Color(1f);
            // Фон
            GLRender.Rectangle(-199, -68, 199, -60, 0, .96875f, 0.78125f, .984375f);
            // Актив левел
            if (level > 0) GLRender.Rectangle(-199, -68, (398 * level - 199), -60, 0, .984375f, 0.78125f * level, 1.0f);


            //int xWL = -199;
            //int xWR = 198;
            //int yH1 = -88;
            //int yH2 = -107;
            int xWL = -384;
            int xWR = 384;
            int yH1 = -32;
            int yH2 = -54;

            // Armor
            int armor = 11; // параметр брони 0 - 20
            if (armor > 0)
            { 
                for (int i = 0; i < 10; i++)
                {
                    int x = xWL + i * 18;

                    // Целая
                    if (i * 2 + 1 < armor) DrawTexturedModalRect(x, yH2, 38, 19, 19, 19);
                    // Половина
                    else if (i * 2 + 1 == armor) DrawTexturedModalRect(x, yH2, 19, 19, 19, 19);
                    // Нет
                    else if (i * 2 + 1 > armor) DrawTexturedModalRect(x, yH2, 0, 19, 19, 19);
                }
            }

            // Health
            int health = Mth.Floor(ClientMain.Player.Health);
            for (int i = 0; i < 10; i++)
            {
                int x = xWL + i * 18;

                // Фон
                DrawTexturedModalRect(x, yH1, 0, 0, 19, 19);
                // Целая
                if (i * 2 + 1 < health) DrawTexturedModalRect(x, yH1, 76, 0, 19, 19);
                // Половина
                else if (i * 2 + 1 == health) DrawTexturedModalRect(x, yH1, 95, 0, 19, 19);
            }

            // Food
            int food = 15; // параметр голода 0 - 20
            for (int i = 0; i < 10; i++)
            {
                int x = xWR - i * 18 - 19;

                // Фон
                DrawTexturedModalRect(x, yH1, 0, 57, 19, 19);
                // Целая
                if (i * 2 + 1 < food) DrawTexturedModalRect(x, yH1, 76, 57, 19, 19);
                // Половина
                else if (i * 2 + 1 == food) DrawTexturedModalRect(x, yH1, 95, 57, 19, 19);
            }

            // Air
            int air = ClientMain.Player.GetAir();
            if (air < 300)
            {
                int air0 = Mth.Ceiling((air - 2f) * 10f / 300f);
                int air1 = Mth.Ceiling(air * 10f / 300f) - air0;

                for (int i = 0; i < air0 + air1; i++)
                {
                    // Пузырь
                    if (i < air0) DrawTexturedModalRect(xWR - i * 18 - 19, yH2, 0, 38, 19, 19);
                    // Лопает
                    else DrawTexturedModalRect(xWR - i * 18 - 19, yH2, 19, 38, 19, 19);
                }
            }
        }

        /// <summary>
        /// Прорисовка инвентаря
        /// </summary>
        private void DrawInventory()
        {
            int scale = 26; // 38
            int size = 50; // 66
            int w = size * -4;
            int h = -size - 8;
            vec4 colorText = new vec4(1);
            FontSize fontSize = FontSize.Font12;

            for (int i = 0; i < 8; i++)
            {
                GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
                GLRender.Color(1f);

                int w0 = w + i * size;

                // Фон
                GLRender.Rectangle(w0, h, w0 + size, h + size, 0, .46875f, 0.1953125f, .6640625f);
                
                if (ClientMain.Player.Inventory.CurrentItem == i)
                {
                    // Выбранный
                    //GLRender.Rectangle(w0, h, w0 + size, h + size, 0.1953125f, .46875f, 0.390625f, .6640625f);
                    GLRender.Rectangle(w0 - 2, h - 2, w0 + 52, h + 52, 0.1953125f, .46875f, 0.40625f, .6796875f);
                }

                // Прорисовка предмета в стаке если есть
                ItemStack itemStack = ClientMain.Player.Inventory.GetStackInSlot(i);
                if (itemStack != null && itemStack.Item is ItemBlock itemBlock)
                {
                    int w1 = w0 + size / 2;
                    int h1 = h + size / 2;
                    ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(w1, h1, scale);
                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                    if (itemStack.Amount > 1)
                    {
                        string str = itemStack.Amount.ToString();
                        int ws = FontRenderer.WidthString(str, fontSize);
                        FontRenderer.RenderString(w1 + 21 - ws, h1 + 10, new vec4(0, 0, 0, 1), str, fontSize);
                        FontRenderer.RenderString(w1 + 20 - ws, h1 + 9, new vec4(1), str, fontSize);
                    }
                }
            }
        }

        /// <summary>
        /// Эффект вводе
        /// </summary>
        private void DrawEffWater(float timeIndex)
        {
            // Позиция камеры
            vec3 posCam = ClientMain.Player.Position + ClientMain.Player.PositionCamera;
            BlockBase block = ClientMain.World.GetBlockState(new BlockPos(posCam)).GetBlock();

            if (block.Material == EnumMaterial.Water)
            {
                GLRender.Texture2DDisable();
                GLRender.Rectangle(0, 0, Width, Height, new vec4(0.0f, 0.1f, 0.4f, 0.7f));
            }
        }

        /// <summary>
        /// Эффект урона
        /// </summary>
        private void DrawEffDamage(float damageTime, float timeIndex)
        {
            if (ClientMain.Player.DamageTime > 0 && ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
            {
                float dt = Mth.Sqrt((damageTime + timeIndex - 1f) / 5f * 1.6f);
                if (dt > 1f) dt = 1f;
                GLRender.Texture2DDisable();
                GLRender.Rectangle(0, 0, Width, Height, new vec4(0.7f, 0.4f, 0.3f, 0.7f * dt));
            }
        }

        /// <summary>
        /// Жизнь
        /// </summary>
        //private void DrawHealth(float timeIndex)
        //{
        //    int w = Width;
        //    int h = Height;

        //    GLRender.PushMatrix();
        //    GLRender.Texture2DDisable();
        //    GLRender.LineWidth(1f);
        //    GLRender.Color(new vec3(0));
        //    for (int i = 0; i < 20; i++)
        //    {
        //        GLRender.Begin(OpenGL.GL_LINE_STRIP);
        //        GLRender.Vertex(30, h - 46 - i * 20, 0);
        //        GLRender.Vertex(46, h - 46 - i * 20, 0);
        //        GLRender.Vertex(46, h - 30 - i * 20, 0);
        //        GLRender.Vertex(30, h - 30 - i * 20, 0);
        //        GLRender.Vertex(30, h - 46 - i * 20, 0);
        //        GLRender.End();
        //    }

        //    int count = Mth.Floor(ClientMain.Player.Health);

        //    for (int i = 0; i < count; i++)
        //    {
        //        GLRender.Rectangle(31, h - 45 - i * 20, 45, h - 31 - i * 20, new vec4(.9f, .4f, .4f, .7f));
        //    }


        //    GLRender.PopMatrix();
        //}

        
    }
}
