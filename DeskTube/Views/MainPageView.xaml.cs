using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Infrastructure;
using Infrastructure.Interfaces;
using mshtml;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for MainPageView.xaml
    /// </summary>
    public partial class MainPageView : IView
    {
        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageView" /> class.
        /// </summary>
        public MainPageView()
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
