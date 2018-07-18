namespace Aggregator.Core.Interfaces
{
    // Identical to Microsoft.TeamFoundation.WorkItemTracking.Server.AddedResourceLink
    public class AddedResourceLink
    {
        public string Resource { get; set; }

        public static implicit operator AddedResourceLink(Microsoft.TeamFoundation.WorkItemTracking.Server.AddedResourceLink addedResourceLink)
        {
            return new AddedResourceLink {Resource = addedResourceLink.Resource};
        }

        public AddedResourceLinkTypes AddedResourceLinkType
        {
            get
            {
                if (this.Resource.StartsWith(@"vstfs:///VersionControl/Changeset/"))
                    return AddedResourceLinkTypes.Changeset;
                if (this.Resource.StartsWith(@"vstfs:///VersionControl/VersionedItem/"))
                    return AddedResourceLinkTypes.SourceFile;
                return AddedResourceLinkTypes.None;
            }
        }
    }
}