namespace MvkServer.Util
{
    public delegate void IntEventHandler(object sender, IntEventArgs e);
    public class IntEventArgs
    {
        /// <summary>
        /// Строка лога
        /// </summary>
        public int Number { get; protected set; }

        public IntEventArgs(int number) => Number = number;
    }
}
