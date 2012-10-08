//-----------------------------------------------------------------------
// <copyright file="SwipeControl.cs" company="WPA">
//     Copyright (c) WPA. All rights reserved.
// </copyright>
// <author>Tavi</author>
//-----------------------------------------------------------------------

using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infrastructure.Utilities;

namespace Infrastructure.Controls
{
    /// <summary>
    /// The Swipe Control
    /// </summary>
    [TemplatePart(Name = "PART_Content", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_PreviousBtn", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
    public class SwipeControl : Control, INavigateService
    {
        #region FIELDS
        /// <summary>
        /// The FirstViewProperty
        /// </summary>
        public static readonly DependencyProperty FirstViewProperty = DependencyProperty.Register("FirstView", typeof(INavigateObject), typeof(SwipeControl), new UIPropertyMetadata(null, OnFirstViewPropertyChanged));

        /// <summary>
        /// The OnCurrentViewPropertyChanged
        /// </summary>
        public static readonly DependencyProperty CurrentViewProperty = DependencyProperty.Register("CurrentView", typeof(INavigateObject), typeof(SwipeControl), new UIPropertyMetadata(null, OnCurrentViewPropertyChanged));

        #endregion

        #region PRIVATE FIELDS
        /// <summary>
        /// The content part
        /// </summary>
        private ContentControl partContent;

        /// <summary>
        /// The previous button part
        /// </summary>
        private Button partPreviousBtn;
        
        /// <summary>
        /// The current node name part
        /// </summary>
        private TextBlock partTitle;
        
        /// <summary>
        /// The stack of NavigateObjects
        /// </summary>
        private readonly Stack<INavigateObject> stackOfNavigateObjects = new Stack<INavigateObject>();

        /// <summary>
        /// Time for the animation
        /// </summary>
        private const double animationTime = 0.3;

        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Initializes static members of the <see cref="SwipeControl"/> class.
        /// </summary>
        static SwipeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SwipeControl), new FrameworkPropertyMetadata(typeof(SwipeControl)));
        }
        #endregion

        #region EVENTS

        /// <summary>
        /// Event for ViewChangedEvent
        /// </summary>
        public event EventHandler<INavigateObject> ViewChangedEvent;

        /// <summary>
        /// Gets the current view.
        /// </summary>
        /// <returns></returns>
        public IViewModel GetCurrentView()
        {
            if (this.stackOfNavigateObjects.Count > 0)
            {
                return this.stackOfNavigateObjects.Peek();
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the navigate back callback.
        /// This is used in order to determine if the SwipeControl can navigate back.
        /// </summary>
        /// <value>
        /// The navigate back callback.
        /// </value>
        public Func<bool> NavigateBackCallback
        {
            get;
            set;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the first view.
        /// </summary>
        /// <value>
        /// The first view.
        /// </value>
        public INavigateObject FirstView
        {
            get { return (INavigateObject)GetValue(FirstViewProperty); }
            set { SetValue(FirstViewProperty, value); }
        }

        public INavigateObject CurrentView
        {
            get { return (INavigateObject)GetValue(CurrentViewProperty); }
            set { SetValue(CurrentViewProperty, value); }
        }

        #endregion

        #region IMPLEMENTS INavigateService

        /// <summary>
        /// Goes to the navigate object we want.
        /// </summary>
        /// <param name="navObject">The nav object.</param>
        /// <param name="whenDone"></param>
        public void GoTo(INavigateObject navObject, EventHandler whenDone = null)
        {
            // if is not visible or something
            if (this.partContent == null)
            {
                return;
            }

            if (navObject.View == null) //If the view model has no View resolve it.
            {
                navObject.ResolveView();
            }

            var originalView = this.stackOfNavigateObjects.Count > 0 ? this.stackOfNavigateObjects.Peek().View : null;

            navObject.NavigationService = this;

            Animate.Slide(
                originalView,
                navObject.View,
                this.partContent,
                animationTime,
                Direction.Left,
                null,
                 (obj, ev) =>
                 {
                     if (whenDone != null)
                     {
                         whenDone(null, EventArgs.Empty);
                     }
                     whenDone = null;

                     this.partContent.Content = navObject.View;
                     this.stackOfNavigateObjects.Push(navObject);
                     //this.stackOfNavigateObjects.Peek().View.Focus();
                     this.OnViewChangedEvent(navObject);
                     this.SetBackLink();
                     this.CurrentView = this.stackOfNavigateObjects.Peek();
                 });
        }

        /// <summary>
        /// Goes on the previous screen if it exists.
        /// </summary>
        /// <param name="whenDone">EventHandler called when the navigation has been completed.</param>
        /// <param name="canNavigateBack">Callback used to determine if it is possible to navigate back.</param>
        public void GoBack(EventHandler whenDone = null, Func<bool> canNavigateBack = null)
        {
            if (canNavigateBack != null) // No callback found => Can not navigate back.
            {
                var result = canNavigateBack();
                if (result)
                {
                    this.NavigateBack(whenDone); // Only navigate back if we are allowed to.
                }
            }
            else
            {
                this.NavigateBack(whenDone);
            }
        }

        #endregion

        #region OVERRIDES

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>  
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.partContent = this.GetTemplateChild("PART_Content") as ContentControl;
            this.partPreviousBtn = this.GetTemplateChild("PART_PreviousBtn") as Button;
            this.partTitle = this.GetTemplateChild("PART_Title") as TextBlock;

            this.partPreviousBtn.Click += this.PartPreviousButton_Click;

            // prepare
            if (this.stackOfNavigateObjects.Count > 0)
            {
                var currentView = this.stackOfNavigateObjects.Peek();
                this.partContent.Content = currentView.View;
                this.partTitle.Text = currentView.Title;
            }

            this.partPreviousBtn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            if (this.partContent == null)
            {
                return;
            }

            while (this.stackOfNavigateObjects.Count > 1)
            {
                var obj = this.stackOfNavigateObjects.Pop();
                obj.ExitNavigation();
            }

            if (this.stackOfNavigateObjects.Count > 0)
            {
                this.partContent.Content = this.stackOfNavigateObjects.Peek().View;
                //this.stackOfNavigateObjects.Peek().View.Focus();
                this.SetBackLink();
            }
        }

        #endregion

        #region PROTECTED METHODS

        /// <summary>
        /// Handler for SwipeList depth changed
        /// </summary>
        /// <param name="e">Data Event Arguments</param>
        public void OnViewChangedEvent(INavigateObject e)
        {
            if (this.ViewChangedEvent != null)
            {
                this.ViewChangedEvent(this, e);
            }
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public void RefreshView()
        {
            if (this.stackOfNavigateObjects.Count > 0)
            {
                var current = this.stackOfNavigateObjects.Pop();
                current.View = null;
                this.GoTo(current);
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Sets the back link.
        /// </summary>
        private void SetBackLink()
        {
            if (this.partPreviousBtn != null)
            {
                if (this.stackOfNavigateObjects.Count < 2)
                {
                    this.partPreviousBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var curr = this.stackOfNavigateObjects.Pop();
                    var link = this.stackOfNavigateObjects.Peek().LinkName;
                    this.stackOfNavigateObjects.Push(curr);
                    this.partPreviousBtn.Visibility = Visibility.Visible;
                }
            }

            if (this.partTitle != null)
            {
                if (this.stackOfNavigateObjects.Count > 0)
                {
                    this.partTitle.Text = this.stackOfNavigateObjects.Peek().Title;
                }
            }
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Roots the property changed.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnFirstViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (SwipeControl)d;
            var oldRoot = e.OldValue as INavigateObject;
            var newRoot = e.NewValue as INavigateObject;

            if (oldRoot != null)
            {
                instance.stackOfNavigateObjects.Clear();
            }

            if (newRoot != null)
            {
                if (newRoot.View == null) //If the new root's view is null resolve it.
                {
                    newRoot.ResolveView();
                }

                newRoot.NavigationService = instance;
                instance.stackOfNavigateObjects.Push(newRoot);
                if (instance.partContent != null)
                {
                    instance.partContent.Content = newRoot.View;
                    instance.partTitle.Text = newRoot.Title;
                }

                instance.SetBackLink();

                instance.CurrentView = instance.stackOfNavigateObjects.Peek();
            }
        }

        /// <summary>
        /// Called when [current view property changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnCurrentViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Handles the Click event of the partPreviousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PartPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            this.GoBack(null, this.NavigateBackCallback);
            //NavigateBack();
        }

        /// <summary>
        /// Navigates the back.
        /// </summary>
        /// <param name="whenDone">The when done.</param>
        private void NavigateBack(EventHandler whenDone = null)
        {
            if (this.stackOfNavigateObjects.Count < 2)
            {
                return;
            }

            var oldObj = this.stackOfNavigateObjects.Pop();
            var oldView = oldObj.View;
            var newObj = this.stackOfNavigateObjects.Peek();
            var newView = newObj.View;

            Animate.Slide(
                oldView,
                newView,
                this.partContent,
                animationTime,
                Direction.Right,
                null,
                (obj, ev) =>
                {
                    this.partContent.Content = newView;
                    this.SetBackLink();

                    if (this.stackOfNavigateObjects.Count > 0)
                    {
                        this.OnViewChangedEvent(this.stackOfNavigateObjects.Peek());
                        //this.stackOfNavigateObjects.Peek().View.Focus();
                    }

                    oldObj.ExitNavigation();

                    if (whenDone != null)
                    {
                        whenDone(newObj, EventArgs.Empty);
                    }

                    whenDone = null;
                });

            this.CurrentView = this.stackOfNavigateObjects.Peek();
        }

        #endregion
    }
}
