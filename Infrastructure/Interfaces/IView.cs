using System;

namespace Infrastructure.Interfaces
{
    public interface IView : IDisposable
    {
        /// <summary>
        /// Sets the data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        void SetDataContext(ViewModelBase dataContext);
    }
}
