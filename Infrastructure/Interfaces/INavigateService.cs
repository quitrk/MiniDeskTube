//-----------------------------------------------------------------------
// <copyright file="INavigateService.cs" company="WPA">
//     Copyright (c) WPA. All rights reserved.
// </copyright>
// <author>Tavi</author>
//-----------------------------------------------------------------------

using System;

namespace Infrastructure.Interfaces
{
    /// <summary>
    /// The navidation service
    /// </summary>
    public interface INavigateService
    {
        /// <summary>
        /// Implement the navigation logic
        /// </summary>
        /// <param name="navObject">The object where you want to show.</param>
        /// <param name="whenDone">EventHandler called when the navigation has been completed.</param>
        void GoTo(INavigateObject navObject, EventHandler whenDone = null);

        /// <summary>
        /// Goes on the previous screen if it exists.
        /// </summary>
        /// <param name="whenDone">EventHandler called when the navigation has been completed.</param>
        /// <param name="canNavigateBack">Callback used to determine if it is possible to navigate back.</param>
        void GoBack(EventHandler whenDone = null, Func<bool> canNavigateBack = null);

        /// <summary>
        /// Resets this instance to the initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Occurs when the view is changed.
        /// </summary>
        event EventHandler<INavigateObject> ViewChangedEvent;

        /// <summary>
        /// Gets the current view.
        /// </summary>
        /// <returns></returns>
        IViewModel GetCurrentView();

        /// <summary>
        /// Gets or sets the navigate back callback.
        /// This is used in order to determine if the SwipeControl can navigate back.
        /// </summary>
        /// <value>
        /// The navigate back callback.
        /// </value>
        Func<bool> NavigateBackCallback { get; set; }
    }
}
