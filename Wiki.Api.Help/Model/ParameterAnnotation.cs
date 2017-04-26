using System;

namespace Wiki.Api.Help.Controllers.Help.Model
{
    public class ParameterAnnotation
    {
        public Attribute AnnotationAttribute { get; set; }

        public string Documentation { get; set; }
    }
}