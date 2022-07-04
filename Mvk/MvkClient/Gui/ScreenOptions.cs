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
        protected Button buttonSmoothLighting;
        protected Slider sliderFps;
        protected Slider sliderChunk;
        protected Slider sliderSoundVolume;
        protected Slider sliderMusicVolume;
        protected Slider sliderSizeInterface;
        protected TextBox textBoxNickname;

        private ushort cacheLanguage;
        private bool cacheSmoothLighting;

        public ScreenOptions(Client client, EnumScreenKey where) : base(client)
        {
            cacheLanguage = Setting.Language;
            cacheSmoothLighting = Setting.SmoothLighting;
            this.where = where;

            label = new Label(Language.Current.Translate("gui.options"), FontSize.Font16);
            labelNickname = new Label(Language.Current.Translate("gui.nikname"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            textBoxNickname = new TextBox(Setting.Nickname) { Width = 160 };
            
            sliderFps = new Slider(10, 260, 10, Language.Current.Translate("gui.fps"))
            {
                Width = 256,
                Value = Setting.Fps
            };
            sliderFps.AddParam(260, Language.Current.Translate("gui.maxfps"));
            sliderChunk = new Slider(2, 32, 1, Language.Current.Translate("gui.overview.chunks"))
            {
                Width = 256,
                Value = Setting.OverviewChunk
            };
            sliderSoundVolume = new Slider(0, 100, 1, Language.Current.Translate("gui.volume.sound"))
            {
                Width = 256,
                Value = Setting.SoundVolume
            };
            sliderSoundVolume.AddParam(0, Language.Current.Translate("gui.volume.off"));
            sliderMusicVolume = new Slider(0, 100, 1, Language.Current.Translate("gui.volume.music"))
            {
                Width = 256,
                Value = Setting.MusicVolume,
                Enabled = false
            };
            sliderSizeInterface = new Slider(1, 2, 1, Language.Current.Translate("gui.size.interface"))
            {
                Width = 192,
                Value = Setting.SizeInterface
            };
            labelLanguage = new Label(Language.Current.Translate("gui.language"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            buttonLanguage = new Button(Language.Get(cacheLanguage).TranslatedName) { Width = 160 };
            buttonLanguage.Click += ButtonLanguage_Click;
            buttonNet = new Button(Language.Current.Translate("gui.net")) { Width = 160 };
            buttonNet.Click += ButtonNet_Click;
            buttonSmoothLighting = new Button(ButtonSmoothLightingName()) { Width = 320 };
            buttonSmoothLighting.Click += ButtonSmoothLighting_Click;
            buttonDone = new Button(Language.Current.Translate("gui.apply")) { Width = 256 };
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(where, Language.Current.Translate("gui.cancel")) { Width = 256 };
            InitButtonClick(buttonCancel);

            if (where == EnumScreenKey.InGameMenu)
            {
                background = EnumBackground.GameWindow;
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
            AddControls(sliderSizeInterface);
            AddControls(buttonSmoothLighting);
            AddControls(buttonDone);
            AddControls(buttonCancel);
            AddControls(buttonLanguage);
            if (ClientMain.IsServerLocalRun())
            {
                if (ClientMain.IsOpenNet())
                {
                    buttonNet.Enabled = false;
                    buttonNet.SetText(Language.Current.Translate("gui.net.on"));
                }
                AddControls(buttonNet);
            }
        }
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = 72 * sizeInterface;
            int hMax = h + 360 * sizeInterface;
            if (hMax > Height) h -= hMax - Height;

            label.Position = new vec2i(Width / 2 - 200 * sizeInterface, h);
            labelNickname.Position = new vec2i(Width / 2 - 162 * sizeInterface, h + 44 * sizeInterface);
            textBoxNickname.Position = new vec2i(Width / 2 + 2 * sizeInterface, h + 44 * sizeInterface);
            sliderSoundVolume.Position = new vec2i(Width / 2 - 258 * sizeInterface, h + 88 * sizeInterface);
            sliderMusicVolume.Position = new vec2i(Width / 2 + 2 * sizeInterface, h + 88 * sizeInterface);
            sliderFps.Position = new vec2i(Width / 2 - 258 * sizeInterface, h + 132 * sizeInterface);
            sliderChunk.Position = new vec2i(Width / 2 + 2 * sizeInterface, h + 132 * sizeInterface);
            sliderSizeInterface.Position = new vec2i(Width / 2 - 258 * sizeInterface, h + 176 * sizeInterface);
            buttonSmoothLighting.Position = new vec2i(Width / 2 - 62 * sizeInterface, h + 176 * sizeInterface);
            labelLanguage.Position = new vec2i(Width / 2 - 162 * sizeInterface, h + 220 * sizeInterface);
            buttonLanguage.Position = new vec2i(Width / 2 + 2 * sizeInterface, h + 220 * sizeInterface);
            buttonNet.Position = new vec2i(Width / 2 + 2 * sizeInterface, h + 264 * sizeInterface);
            buttonDone.Position = new vec2i(Width / 2 - 258 * sizeInterface, h + 308 * sizeInterface);
            buttonCancel.Position = new vec2i(Width / 2 + 2 * sizeInterface, h + 308 * sizeInterface);
        }

        private void ButtonLanguage_Click(object sender, EventArgs e)
        {
            cacheLanguage = Language.Get(cacheLanguage).Next().ID;
            buttonLanguage.SetText(Language.Get(cacheLanguage).TranslatedName);
        }

        private string ButtonSmoothLightingName() => Language.Current.Translate("gui.smooth.lighting." + (cacheSmoothLighting ? "on" : "off"));

        private void ButtonSmoothLighting_Click(object sender, EventArgs e)
        {
            cacheSmoothLighting = !cacheSmoothLighting;
            buttonSmoothLighting.SetText(ButtonSmoothLightingName());
        }

        private void ButtonNet_Click(object sender, EventArgs e)
        {
            if (where == EnumScreenKey.InGameMenu)
            {
                buttonNet.Enabled = false;
                buttonNet.SetText(Language.Current.Translate("gui.net.on"));
                ClientMain.OpenNet();
            }
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохранение настроек
            if (where == EnumScreenKey.InGameMenu)
            {
                if (Setting.OverviewChunk != sliderChunk.Value)
                {
                    ClientMain.World.ChunkPrClient.SetOverviewChunk();
                    ClientMain.Player.SetOverviewChunk(sliderChunk.Value);
                }
                if (Setting.SmoothLighting != cacheSmoothLighting)
                {
                    ClientMain.World.RerenderAllChunks();
                }
            }

            Setting.OverviewChunk = sliderChunk.Value;
            Setting.MusicVolume = sliderMusicVolume.Value;
            Setting.SoundVolume = sliderSoundVolume.Value;
            Setting.Fps = sliderFps.Value;
            Setting.Nickname = textBoxNickname.Text;
            Setting.Language = cacheLanguage;
            Setting.SmoothLighting = cacheSmoothLighting;
            Setting.SizeInterface = sliderSizeInterface.Value;
            Setting.Save();
            Language.Select(cacheLanguage);

            OnFinished(where);
        }
    }
}
