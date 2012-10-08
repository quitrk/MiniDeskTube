//-----------------------------------------------------------------------
// <copyright file="INavigateObject.cs" company="WPA">
//     Copyright (c) WPA. All rights reserved.
// </copyright>
// <author>Tavi</author>
//-----------------------------------------------------------------------

namespace Infrastructure.Interfaces
{
    /// <summary>
    /// Implemet is you want to navigate to this
    /// </summary>
    public interface INavigateObject : IViewModel
    {
        /// <summary>
        /// Gets or sets the navigation service.
        /// </summary>
        /// <value>
        /// The navigation service.
        /// </value>
        INavigateService NavigationService { get; set; }

        /// <summary>
        /// Gets or sets the name of 
        /// the link to be put on Previous button 
        /// for the INavigateObject this instance will drill down to.
        /// </summary>
        /// <value>
        /// The name of the link.
        /// </value>
        string LinkName { get; set; }

        /// <summary>
        /// Gets or sets the title of the page (could be the same with the link name)
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; set; }

        /// <summary>
        /// Gets the extra link. It will show as the Show all link
        /// </summary>
        INavigateObject ExtraLink { get; set; }

        /// <summary>
        /// Exit the navigation is called when this object is not used in navigation anymore.
        /// Use it to cleanup
        /// </summary>
        void ExitNavigation();

        /// <summary>
        /// Setups the navigate object properties.
        /// </summary>
        void SetupNavigateObjectProperties();
    }
}
