using System;
using System.Reflection;

namespace Wiki.Api.Help.Controllers.Help
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}