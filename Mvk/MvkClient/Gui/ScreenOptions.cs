using MvkAssets;
using MvkClient.Setitings;
using MvkServer.Glm;
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
            if (where == EnumScreenKey.InGameMenu) background = EnumBackground.Game;

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
            buttonDone = new Button(Language.T("gui.apply")) { Width = 256 };
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(where, Language.T("gui.cancel")) { Width = 256 };
            InitButtonClick(buttonCancel);
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
        }
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            label.Position = new vec2i(Width / 2 - 200, Height / 4 - 64);
            labelNickname.Position = new vec2i(Width / 2 - 162, Height / 4 - 8);
            textBoxNickname.Position = new vec2i(Width / 2 + 2, Height / 4 - 8);
            sliderSoundVolume.Position = new vec2i(Width / 2 - 258, Height / 4 + 48);
            sliderMusicVolume.Position = new vec2i(Width / 2 + 2, Height / 4 + 48);
            sliderFps.Position = new vec2i(Width / 2 - 258, Height / 4 + 92);
            sliderChunk.Position = new vec2i(Width / 2 + 2, Height / 4 + 92);
            labelLanguage.Position = new vec2i(Width / 2 - 162, Height / 4 + 136);
            buttonLanguage.Position = new vec2i(Width / 2 + 2, Height / 4 + 136);
            buttonDone.Position = new vec2i(Width / 2 - 258, Height / 4 + 192);
            buttonCancel.Position = new vec2i(Width / 2 + 2, Height / 4 + 192);
        }

        private void ButtonLanguage_Click(object sender, EventArgs e)
        {
            cacheLanguage = Language.Next(cacheLanguage);
            buttonLanguage.SetText(Language.GetName(cacheLanguage));
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохранение настроек
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
