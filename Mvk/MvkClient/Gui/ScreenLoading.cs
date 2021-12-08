namespace MvkClient.Gui
{
    public abstract class ScreenLoading : Screen
    {
        /// <summary>
        /// Максимальное значение элементов загрузки
        /// </summary>
        protected int max = 1;
        /// <summary>
        /// Сколько элементов загруженно
        /// </summary>
        protected int value = 0;

        protected ScreenLoading() { }
        public ScreenLoading(Client client) : base(client) { }

        /// <summary>
        /// Задать максимальное значение загрузчика
        /// </summary>
        public void SetMax(int max) => this.max = max;

        /// <summary>
        /// Следующий шаг загрузки
        /// </summary>
        public virtual void Step()
        {
            value++;
            RenderList();
        }
    }
}
