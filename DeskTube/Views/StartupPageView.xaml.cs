using Infrastructure;
using Infrastructure.Interfaces;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for StartupPageView.xaml
    /// </summary>
    public partial class StartupPageView : IView
    {
        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupPageView" /> class.
        /// </summary>
        public StartupPageView()
        {
            InitializeComponent();
        }

        #endregion
        
        #region IView overrides

        /// <summary>
        /// Sets the data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public void SetDataContext(ViewModelBase dataContext)
        {
            this.DataContext = dataContext;
        }

        #endregion
    }
}
