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
            // Статусы
            DrawStat();

            GLRender.PopMatrix();
        }

        /// <summary>
        /// Прорисовка статусов
        /// </summary>
        private void DrawStat()
        {
            int armor = 9; // параметр брони 0 - 16
            int health = Mth.Ceiling(ClientMain.Player.Health); // хп 0 - 16
            int endurance = 16; // выносливось 0 - 16
            int food = 16; // параметр голода 0 - 16
            int air = ClientMain.Player.GetAir();
            int air0 = 0;
            int uH = 0;
            int uA = 0;

            // Level
            if (ClientMain.Player.XpBarCap() > 0)
            {
                GLRender.Color(1f);
                GLWindow.Texture.BindTexture(AssetsTexture.Icons);
                int levelBar = (int)(ClientMain.Player.Experience * 398f);
                DrawTexturedModalRect(-199, -67, 0, 496, 398, 8); // Фон
                if (levelBar > 0) DrawTexturedModalRect(-199, -67, 0, 504, levelBar, 8); // Значение
            }

            // Level Text
            if (ClientMain.Player.ExperienceLevel > 0)
            {
                string str = ClientMain.Player.ExperienceLevel.ToString();
                FontSize fontSize = FontSize.Font12;
                GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                int x = FontRenderer.WidthString(str, fontSize) / -2;
                int y = -73;
                FontRenderer.RenderString(x + 1, y, new vec4(0, 0, 0, 1), str, fontSize);
                FontRenderer.RenderString(x - 1, y, new vec4(0, 0, 0, 1), str, fontSize);
                FontRenderer.RenderString(x, y + 1, new vec4(0, 0, 0, 1), str, fontSize);
                FontRenderer.RenderString(x, y - 1, new vec4(0, 0, 0, 1), str, fontSize);
                FontRenderer.RenderString(x, y, new vec4(.52f, .9f, .2f, 1f), str, fontSize);//.5f, 1f, .125f
            }

            GLRender.Color(1f);
            GLWindow.Texture.BindTexture(AssetsTexture.Icons);

            // Armor static
            if (armor > 0)
            {
                DrawTexturedModalRect(-256, -78, 0, 19, 19, 19);
                DrawTexturedModalRect(-256, -78, 76, 19, 19, 19);
            }

            // Health static
            if (ClientMain.Player.DamageTime > 0)
            {
                if (ClientMain.Player.DamageTime == 1 || ClientMain.Player.DamageTime == 5) uH = 19;
                else if (ClientMain.Player.DamageTime == 2 || ClientMain.Player.DamageTime == 4) uH = 38;
                else if (ClientMain.Player.DamageTime == 3) uH = 57;
            }
            DrawTexturedModalRect(-228, -78, uH, 0, 19, 19);
            DrawTexturedModalRect(-228, -78, 76, 0, 19, 19);

            // Endurance static
            DrawTexturedModalRect(209, -78, 0, 76, 19, 19);
            DrawTexturedModalRect(209, -78, 76, 76, 19, 19);

            // Food static
            DrawTexturedModalRect(237, -78, 0, 57, 19, 19);
            DrawTexturedModalRect(237, -78, 76, 57, 19, 19);

            // Air static
            if (air < 300)
            {
                air0 = Mth.Ceiling((air - 2f) * 16f / 300f);
                int air1 = Mth.Ceiling(air * 16f / 300f) - air0;

                if (air1 == 0)
                {
                    uA = 0;
                    DrawTexturedModalRect(265, -78, 0, 38, 19, 19);
                }
                else
                {
                    uA = 19;
                    DrawTexturedModalRect(265, -78, 19, 38, 19, 19);
                }
            }

            // Динамик
            for (int i = 0; i < 8; i++)
            {
                int y = -16 - i * 6;
                int i16 = i * 2 + 1;

                // Armor dinamic
                if (armor > 0)
                {
                    DrawTexturedModalRect(-256, y, 0, 473, 19, 7); // Фон
                    if (i16 < armor) DrawTexturedModalRect(-256, y, 95, 473, 19, 7); // Целая и половина
                    else if (i16 == armor) DrawTexturedModalRect(-256, y, 95, 473, 9, 7); // Целая и половина
                }

                // Health dinamic
                DrawTexturedModalRect(-228, y, uH, 473, 19, 7); // Фон
                if (i16 < health) DrawTexturedModalRect(-228, y, 76, 473, 19, 7); // Целая и половина
                else if (i16 == health) DrawTexturedModalRect(-228, y, 76, 473, 9, 7); // Целая и половина

                // Endurance dinamic
                DrawTexturedModalRect(209, y, 0, 473, 19, 7); // Фон
                if (i16 < endurance) DrawTexturedModalRect(209, y, 152, 473, 19, 7); // Целая и половина
                else if (i16 == endurance) DrawTexturedModalRect(209, y, 152, 473, 9, 7); // Целая и половина

                // Food dinamic
                DrawTexturedModalRect(237, y, 0, 473, 19, 7); // Фон
                if (i16 < food) DrawTexturedModalRect(237, y, 133, 473, 19, 7); // Целая и половина
                if (i16 == food) DrawTexturedModalRect(237, y, 133, 473, 9, 7); // Целая и половина

                // Air dinamic
                if (air < 300)
                {
                    DrawTexturedModalRect(265, y, uA, 473, 19, 7); // Фон
                    if (i16 < air0) DrawTexturedModalRect(265, y, 114, 473, 19, 7); // Целая и половина
                    else if (i16 == air0) DrawTexturedModalRect(265, y, 114, 473, 9, 7); // Целая и половина
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
    }
}
