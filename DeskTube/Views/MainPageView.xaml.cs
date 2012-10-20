using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DeskTube.ViewModels;
using Infrastructure;
using Infrastructure.Interfaces;

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

        #region EVENT HANDLERS

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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool all)
        {

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion
    }
}
