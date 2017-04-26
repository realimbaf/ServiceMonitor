using Wiki.Api.Help.Controllers.Help.Model;

namespace Wiki.Api.Help.Model
{
    public class CollectionModelDescription : ModelDescription
    {
        public ModelDescription ElementDescription { get; set; }
    }
}