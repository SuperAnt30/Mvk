namespace MvkClient.Gui
{
    public delegate void ScreenEventHandler(object sender, ScreenEventArgs e);
    public class ScreenEventArgs
    {
        /// <summary>
        /// Ключ
        /// </summary>
        public EnumScreenKey Key { get; protected set; }
        /// <summary>
        /// Откуда
        /// </summary>
        public EnumScreenKey Where { get; protected set; }
        /// <summary>
        /// Номер слота
        /// </summary>
        public int Slot { get; protected set; } = -1;
        /// <summary>
        /// Текст
        /// </summary>
        public string Text { get; protected set; }
        /// <summary>
        /// Вспомогательный объект
        /// </summary>
        public object Tag { get; set; }

        public ScreenEventArgs(EnumScreenKey key) => Key = key;
        public ScreenEventArgs(EnumScreenKey key, EnumScreenKey where) : this(key) => Where = where;
        public ScreenEventArgs(EnumScreenKey key, EnumScreenKey where, int slot) : this(key, where) => Slot = slot;
        public ScreenEventArgs(EnumScreenKey key, EnumScreenKey where, int slot, string text) : this(key, where, slot) => Text = text;
    }
}
