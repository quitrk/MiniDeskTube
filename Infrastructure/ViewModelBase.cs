using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Infrastructure.Interfaces;

namespace Infrastructure
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region EVENTS

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region BACKING FIELDS

        /// <summary>
        /// Backing field for IsLoading property
        /// </summary>
        private bool isLoading;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase" /> class.
        /// </summary>
        public ViewModelBase()
        {
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>
        /// The view.
        /// </value>
        public IView View { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loading.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.OnPropertyChanged(() => this.IsLoading);
                this.OnPropertyChanged(() => this.IsViewEnabled);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is view enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is view enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsViewEnabled
        {
            get { return !this.IsLoading; }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Resolves the view.
        /// </summary>
        public virtual void ResolveView()
        {

        }

        #endregion

        #region PROTECTED METHODS

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <typeparam name="T">The generic type param</typeparam>
        /// <param name="propertyExpresssion">The property expression.</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = this.ExtractPropertyName(propertyExpresssion);
            this.OnPropertyChanged(propertyName);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// OnPropertyChanged is triggered if a property is changed
        /// </summary>
        /// <param name="propertyName">The property name</param>
        private void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Extracts the name of the property.
        /// </summary>
        /// <typeparam name="T">The generic typeparam</typeparam>
        /// <param name="propertyExpresssion">The property expresssion.</param>
        /// <returns>the property name</returns>
        [DebuggerStepThrough]
        private string ExtractPropertyName<T>(Expression<Func<T>> propertyExpresssion)
        {
            if (propertyExpresssion == null)
            {
                throw new ArgumentNullException("propertyExpresssion");
            }

            var memberExpression = propertyExpresssion.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("The expression is not a member access expression.", "propertyExpresssion");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("The member access expression does not access a property.", "propertyExpresssion");
            }

            if (!property.DeclaringType.IsInstanceOfType(this))
            {
                throw new ArgumentException("The referenced property belongs to a different type.", "propertyExpresssion");
            }

            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                // this shouldn't happen - the expression would reject the property before reaching this far
                throw new ArgumentException("The referenced property does not have a get method.", "propertyExpresssion");
            }

            if (getMethod.IsStatic)
            {
                throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");
            }

            return memberExpression.Member.Name;
        }

        #endregion

        #region VIRTUAL METHODS

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool all)
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
