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
        protected Button buttonCancel;
        protected Button buttonDone;
        protected Slider sliderFps;
        protected Slider sliderChunk;
        protected Slider sliderSoundVolume;
        protected Slider sliderMusicVolume;
        protected TextBox textBoxNickname;

        public ScreenOptions(Client client, EnumScreenKey where) : base(client)
        {
            this.where = where;
            if (where == EnumScreenKey.InGameMenu) background = EnumBackground.Game;

            label = new Label("Опции", FontSize.Font16);
            labelNickname = new Label("Имя игрока:", FontSize.Font12) { Width = 160 };
            textBoxNickname = new TextBox(Setting.Nickname) { Width = 160 };
            sliderFps = new Slider(10, 260, 10, "FPS")
            {
                Width = 256,
                Value = Setting.Fps
            };
            sliderFps.AddParam(260, "Максимум FPS");
            sliderChunk = new Slider(2, 32, 1, "Обзор chunks")
            {
                Width = 256,
                Value = Setting.OverviewChunk
            };
            sliderSoundVolume = new Slider(0, 100, 1, "Общая громкость")
            {
                Width = 256,
                Value = Setting.SoundVolume
            };
            sliderSoundVolume.AddParam(0, "Выключен звук");
            sliderMusicVolume = new Slider(0, 100, 1, "Громкость музыки")
            {
                Width = 256,
                Value = Setting.MusicVolume,
                Enabled = false
            };
            buttonDone = new Button("Применить") { Width = 256 };
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(where, "Отмена") { Width = 256 };
            InitButtonClick(buttonCancel);
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(labelNickname);
            AddControls(textBoxNickname);
            AddControls(sliderFps);
            AddControls(sliderChunk);
            AddControls(sliderSoundVolume);
            AddControls(sliderMusicVolume);
            AddControls(buttonDone);
            AddControls(buttonCancel);
        }
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            label.Position = new vec2i(Width / 2 - 200, Height / 4 - 64);
            labelNickname.Position = new vec2i(Width / 2 - 158, Height / 4 - 8);
            textBoxNickname.Position = new vec2i(Width / 2 + 2, Height / 4 - 8);
            sliderSoundVolume.Position = new vec2i(Width / 2 - 258, Height / 4 + 48);
            sliderMusicVolume.Position = new vec2i(Width / 2 + 2, Height / 4 + 48);
            sliderFps.Position = new vec2i(Width / 2 - 258, Height / 4 + 92);
            sliderChunk.Position = new vec2i(Width / 2 + 2, Height / 4 + 92);
            buttonDone.Position = new vec2i(Width / 2 - 258, Height / 4 + 192);
            buttonCancel.Position = new vec2i(Width / 2 + 2, Height / 4 + 192);
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохранение настроек
            Setting.OverviewChunk = sliderChunk.Value;
            Setting.MusicVolume = sliderMusicVolume.Value;
            Setting.SoundVolume = sliderSoundVolume.Value;
            Setting.Fps = sliderFps.Value;
            Setting.Nickname = textBoxNickname.Text;
            Setting.Save();

            OnFinished(where);
        }
    }
}
