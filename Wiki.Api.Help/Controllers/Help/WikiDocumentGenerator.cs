using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

namespace Wiki.Api.Help.Controllers.Help
{
    public class WikiDocumentGenerator : IDocumentationProvider, IModelDocumentationProvider
    {

        private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string ParameterExpression = "param[@name='{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}']";
        private const string FieldExpression = "/doc/members/member[@name='F:{0}']";


        private readonly Dictionary<string, XPathNavigator> _documentNavigators = new Dictionary<string, XPathNavigator>();
        private static string _dir;


        public static void SetWorckDir(string dir)
        {
            _dir = dir;
        }

        public string GetDocumentation(HttpControllerDescriptor controllerDescriptor)
        {
            return GetDocumentation(controllerDescriptor.ControllerType);
        }

        public string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator methodNode = GetMethodNode(actionDescriptor);
            return GetTagValue(methodNode, "summary");         
        }



        public string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                XPathNavigator methodNode = GetMethodNode(reflectedParameterDescriptor.ActionDescriptor);
                if (methodNode != null)
                {
                    string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    XPathNavigator parameterNode = methodNode.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName));
                    if (parameterNode != null)
                    {
                        return parameterNode.Value.Trim();
                    }
                }
            }

            return null;
        }

        public string GetResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator methodNode = GetMethodNode(actionDescriptor);
            return GetTagValue(methodNode, "returns");
        }

        public string GetDocumentation(MemberInfo member)
        {
            var navigator = GetNodeDescriptor(member.DeclaringType);
            if (navigator != null)
            {
                var memberName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}",
                    GetTypeName(member.DeclaringType), member.Name);
                var expression = member.MemberType == MemberTypes.Field ? FieldExpression : PropertyExpression;
                var selectExpression = String.Format(CultureInfo.InvariantCulture, expression, memberName);
                XPathNavigator propertyNode = navigator.SelectSingleNode(selectExpression);
                return GetTagValue(propertyNode, "summary");
            }
            return null;
        }

        public string GetDocumentation(Type type)
        {
            var navigator = GetNodeDescriptor(type);
            if (navigator != null)
            {
                string controllerTypeName = GetTypeName(type);
                string selectExpression = String.Format(CultureInfo.InvariantCulture, TypeExpression, controllerTypeName);
                XPathNavigator typeNode = navigator.SelectSingleNode(selectExpression);
                return GetTagValue(typeNode, "summary");
            }

            return null;
        }


        private XPathNavigator GetMethodNode(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                var navigator = GetNodeDescriptor(reflectedActionDescriptor.MethodInfo.DeclaringType);
                if (navigator != null)
                {
                    string selectExpression = string.Format(CultureInfo.InvariantCulture, MethodExpression,
                        GetMemberName(reflectedActionDescriptor.MethodInfo));
                    return navigator.SelectSingleNode(selectExpression);
                }
            }

            return null;
        }



        private static string GetMemberName(MethodInfo method)
        {
            string name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(method.DeclaringType), method.Name);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                string[] parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
                name += String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames));
            }

            return name;
        }
        private static string GetTypeName(Type type)
        {
            string name = type.FullName;
            if (type.IsGenericType)
            {
                // Format the generic type name to something like: Generic{System.Int32,System.String}
                Type genericType = type.GetGenericTypeDefinition();
                Type[] genericArguments = type.GetGenericArguments();
                string genericTypeName = genericType.FullName;

                // Trim the generic parameter counts from the name
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                string[] argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
                name = String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, String.Join(",", argumentTypeNames));
            }
            if (type.IsNested)
            {
                // Changing the nested type name from OuterType+InnerType to OuterType.InnerType to match the XML documentation syntax.
                name = name.Replace("+", ".");
            }

            return name;
        }
        private static string GetTagValue(XPathNavigator parentNode, string tagName)
        {
            if (parentNode != null)
            {
                XPathNavigator node = parentNode.SelectSingleNode(tagName);
                if (node != null)
                {
                    return node.Value.Trim();
                }
            }

            return null;
        }

        private XPathNavigator GetNodeDescriptor(Type type)
        {
            if (type == null)
                return null;
            var ass = type.Assembly;
            var assName = new AssemblyName(ass.FullName).Name;
            if (this._documentNavigators.ContainsKey(assName))
                return this._documentNavigators[assName];
            var fn = HelpHelper.GetXmlPath(assName);
            if (string.IsNullOrWhiteSpace(fn))
            {
                if (!string.IsNullOrWhiteSpace(ass.Location))
                {
                    var dir = !string.IsNullOrEmpty(_dir)
                        ? _dir
                        : Path.GetDirectoryName(ass.Location);
                    fn = Path.Combine(dir, assName + ".xml");
                }
            }
            if (!string.IsNullOrWhiteSpace(fn) && File.Exists(fn))
            {
                var stream = new MemoryStream(File.ReadAllBytes(fn));
                XPathDocument xpath = new XPathDocument(stream);
                this._documentNavigators[assName] = xpath.CreateNavigator();
                return this._documentNavigators[assName];
            }
            return null;
        }


    }

    public class HelpHelper
    {
        private static readonly Dictionary<string, string> _dic = new Dictionary<string, string>();


        public static void AddXml(string assName, string path)
        {
            _dic[assName] = path;
        }

        internal static string GetXmlPath(string assName)
        {
            if (_dic.ContainsKey(assName))
                return _dic[assName];
            return null;
        }
    }
}