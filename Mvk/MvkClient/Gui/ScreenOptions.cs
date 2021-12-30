using MvkAssets;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
using System;

namespace MvkClient.Gui
{
    public class ScreenOptions : Screen
    {
        protected Label label;
        protected Label labelNickname;
        protected Label labelLanguage;
        protected Button buttonCancel;
        protected Button buttonDone;
        protected Button buttonLanguage;
        protected Button buttonNet;
        protected Slider sliderFps;
        protected Slider sliderChunk;
        protected Slider sliderSoundVolume;
        protected Slider sliderMusicVolume;
        protected TextBox textBoxNickname;

        protected int cacheLanguage;

        public ScreenOptions(Client client, EnumScreenKey where) : base(client)
        {
            cacheLanguage = Setting.Language;
            this.where = where;

            label = new Label(Language.T("gui.options"), FontSize.Font16);
            labelNickname = new Label(Language.T("gui.nikname"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            textBoxNickname = new TextBox(Setting.Nickname) { Width = 160 };
            
            sliderFps = new Slider(10, 260, 10, Language.T("gui.fps"))
            {
                Width = 256,
                Value = Setting.Fps
            };
            sliderFps.AddParam(260, Language.T("gui.maxfps"));
            sliderChunk = new Slider(2, 32, 1, Language.T("gui.overview.chunks"))
            {
                Width = 256,
                Value = Setting.OverviewChunk
            };
            sliderSoundVolume = new Slider(0, 100, 1, Language.T("gui.volume.sound"))
            {
                Width = 256,
                Value = Setting.SoundVolume
            };
            sliderSoundVolume.AddParam(0, Language.T("gui.volume.off"));
            sliderMusicVolume = new Slider(0, 100, 1, Language.T("gui.volume.music"))
            {
                Width = 256,
                Value = Setting.MusicVolume,
                Enabled = false
            };
            labelLanguage = new Label(Language.T("gui.language"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            buttonLanguage = new Button(Language.GetName(cacheLanguage)) { Width = 160 };
            buttonLanguage.Click += ButtonLanguage_Click;
            buttonNet = new Button(Language.T("gui.net")) { Width = 160 };
            buttonNet.Click += ButtonNet_Click;
            buttonDone = new Button(Language.T("gui.apply")) { Width = 256 };
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(where, Language.T("gui.cancel")) { Width = 256 };
            InitButtonClick(buttonCancel);

            if (where == EnumScreenKey.InGameMenu)
            {
                background = EnumBackground.Game;
                textBoxNickname.Enabled = false;
            }
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(labelNickname);
            AddControls(labelLanguage);
            AddControls(textBoxNickname);
            AddControls(sliderFps);
            AddControls(sliderChunk);
            AddControls(sliderSoundVolume);
            AddControls(sliderMusicVolume);
            AddControls(buttonDone);
            AddControls(buttonCancel);
            AddControls(buttonLanguage);
            if (ClientMain.IsServerLocalRun())
            {
                if (ClientMain.IsOpenNet())
                {
                    buttonNet.Enabled = false;
                    buttonNet.SetText(Language.T("gui.net.on"));
                }
                AddControls(buttonNet);
            }
        }
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            label.Position = new vec2i(Width / 2 - 200, 72);
            labelNickname.Position = new vec2i(Width / 2 - 162, 120);
            textBoxNickname.Position = new vec2i(Width / 2 + 2, 120);
            sliderSoundVolume.Position = new vec2i(Width / 2 - 258, 164);
            sliderMusicVolume.Position = new vec2i(Width / 2 + 2, 164);
            sliderFps.Position = new vec2i(Width / 2 - 258, 208);
            sliderChunk.Position = new vec2i(Width / 2 + 2, 208);
            labelLanguage.Position = new vec2i(Width / 2 - 162, 252);
            buttonLanguage.Position = new vec2i(Width / 2 + 2, 252);
            buttonNet.Position = new vec2i(Width / 2 + 2, 296);
            buttonDone.Position = new vec2i(Width / 2 - 258, 400);
            buttonCancel.Position = new vec2i(Width / 2 + 2, 400);
        }

        private void ButtonLanguage_Click(object sender, EventArgs e)
        {
            cacheLanguage = Language.Next(cacheLanguage);
            buttonLanguage.SetText(Language.GetName(cacheLanguage));
        }

        private void ButtonNet_Click(object sender, EventArgs e)
        {
            if (where == EnumScreenKey.InGameMenu)
            {
                buttonNet.Enabled = false;
                buttonNet.SetText(Language.T("gui.net.on"));
                ClientMain.OpenNet();
            }
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохранение настроек

            if (where == EnumScreenKey.InGameMenu && Setting.OverviewChunk != sliderChunk.Value)
            {
                // Отправить запрос серверу на смену опций
                ClientMain.World.ChunkPrClient.ClearAllChunks();
                ClientMain.TrancivePacket(new PacketC13ClientSetting(sliderChunk.Value));
                ClientMain.World.Player.SetOverviewChunk(sliderChunk.Value, 0);
            }

            Setting.OverviewChunk = sliderChunk.Value;
            Setting.MusicVolume = sliderMusicVolume.Value;
            Setting.SoundVolume = sliderSoundVolume.Value;
            Setting.Fps = sliderFps.Value;
            Setting.Nickname = textBoxNickname.Text;
            Setting.Language = cacheLanguage;
            Setting.Save();
            Language.SetLanguage((AssetsLanguage)cacheLanguage);

            OnFinished(where);
        }
    }
}
