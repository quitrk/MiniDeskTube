namespace Infrastructure.Interfaces
{
    public interface IView
    {
        /// <summary>
        /// Sets the data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        void SetDataContext(ViewModelBase dataContext);
    }
}
