namespace MvkServer.Util
{
    public delegate void StringEventHandler(object sender, StringEventArgs e);
    public class StringEventArgs
    {
        /// <summary>
        /// Строка лога
        /// </summary>
        public string Text { get; protected set; }

        public StringEventArgs(string text) => Text = text;
    }
}
