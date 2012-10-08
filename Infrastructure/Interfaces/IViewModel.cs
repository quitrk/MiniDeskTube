namespace Infrastructure.Interfaces
{
    public interface IViewModel
    {
        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        IView View { get; set; }

        /// <summary>
        /// Resolves the view.
        /// </summary>
        /// <returns></returns>
        void ResolveView();
    }
}