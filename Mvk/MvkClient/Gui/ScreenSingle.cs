﻿using MvkAssets;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenSingle : Screen
    {
        protected Label label;
        protected Button buttonCancel;
        protected Button[] buttonSlots = new Button[5];
        protected Button[] buttonSlotsDel = new Button[5];

        public ScreenSingle(Client client, int slotDel) : base(client)
        {

            if (slotDel > 0) DelSlot(slotDel);

            label = new Label(Language.Current.Translate("gui.singleplayer"), FontSize.Font16);

            for (int i = 0; i < buttonSlots.Length; i++)
            {
                buttonSlots[i] = new Button(Language.Current.Translate("gui.world.empty")) { Width = 356 };
                buttonSlots[i].Tag = i + 1; // Номер слота
                buttonSlots[i].Click += ButtonSlots_Click;
                buttonSlotsDel[i] = new Button("X") { Width = 40 };
                buttonSlotsDel[i].Tag = i + 1; // Номер слота
                buttonSlotsDel[i].Click += ButtonSlotsDel_Click;
            }
            buttonCancel = new Button(EnumScreenKey.Main, Language.Current.Translate("gui.cancel"));
            InitButtonClick(buttonCancel);
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(buttonCancel);
            for (int i = 0; i < buttonSlots.Length; i++)
            {
                AddControls(buttonSlots[i]);
                AddControls(buttonSlotsDel[i]);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = 120 * sizeInterface;
            int hMax = h + 292 * sizeInterface;
            if (hMax > Height) h -= hMax - Height;

            label.Position = new vec2i(Width / 2 - 200 * sizeInterface, h - 48 * sizeInterface);
            for (int i = 0; i < buttonSlots.Length; i++)
            {
                buttonSlots[i].Position = new vec2i(Width / 2 - 200 * sizeInterface, h + (i * 44) * sizeInterface);
                buttonSlotsDel[i].Position = new vec2i(Width / 2 + 160 * sizeInterface, h + (i * 44) * sizeInterface);
            }
            buttonCancel.Position = new vec2i(Width / 2 - 200 * sizeInterface, h + 240 * sizeInterface);
        }

        private void ButtonSlots_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int slot = (int)button.Tag;
            OnFinished(new ScreenEventArgs(EnumScreenKey.WorldBegin, EnumScreenKey.SinglePlayer, slot));
        }

        private void ButtonSlotsDel_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int slot = (int)button.Tag;
            OnFinished(new ScreenEventArgs(EnumScreenKey.YesNo, EnumScreenKey.SinglePlayer, slot,
               Language.Current.Translate("gui.world.delete")));
        }

        /// <summary>
        /// Удалить слот мира
        /// </summary>
        /// <param name="slot">Слот 1-5</param>
        protected void DelSlot(int slot)
        {
            if (slot > 0) return;
        }
    }
}
