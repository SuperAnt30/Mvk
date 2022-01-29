namespace MvkClient.Util
{
    public delegate void CursorEventHandler(object sender, CursorEventArgs e);
    public class CursorEventArgs
    {
        /// <summary>
        /// Курсор только в окне
        /// </summary>
        public bool IsBounds { get; protected set; } = true;

        public CursorEventArgs(bool isBounds) => IsBounds = isBounds;
    }
}
