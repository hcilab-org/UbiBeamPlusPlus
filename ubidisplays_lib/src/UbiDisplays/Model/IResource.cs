
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model
{
    /// <summary>
    /// Defines methods for resources which can be requested by a display.  This is used to help remove anything state based (i.e. spatial queries) after
    /// a display is removed from a surface.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Delete this resource and remove it from the authority.
        /// </summary>
        void Delete();

        /// <summary>
        /// Check to see if this resource has been deleted.
        /// </summary>
        bool IsDeleted();

        /// <summary>
        /// An event which is raised when this is deleted.
        /// </summary>
        event Action<IResource> OnDeleted;
    }


    /// <summary>
    /// Defines methods for objects which can own other resources which they should release on deletion.
    /// </summary>
    public abstract class ResourceOwner // : IResourceOwner
    {
        /// <summary>
        /// A list of resources asociated with this displays. i.e. Spatial queries or other surfaces it has created.
        /// </summary>
        //[NonSerialized]
        protected List<IResource> lResources = new List<IResource>();

        /// <summary>
        /// Add a resource to the list of managed resources.
        /// </summary>
        /// <param name="pResource"></param>
        public void AttachResource(IResource pResource)
        {
            lResources.Add(pResource);
        }

        /// <summary>
        /// Remove a resource from the list of managed resources.
        /// </summary>
        /// <remarks>Make sure to delete the resource if you are not going to put it elsewhere.</remarks>
        /// <param name="pResource">The resource in question.</param>
        /// <returns>True if it was removed, false if not.</returns>
        public bool RemoveResource(IResource pResource)
        {
            return lResources.Remove(pResource);
        }

        /// <summary>
        /// Delete all the resources managed by this object.
        /// </summary>
        protected void DeleteResources()
        {
            foreach (var pResource in lResources)
            {
                if (pResource.IsDeleted())
                    continue;
                pResource.Delete();
            }
            lResources.Clear();
        }
    }
}
