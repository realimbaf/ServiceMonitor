using System.Configuration;

namespace Wiki.DiscoveryService.Common
{  
    public static class Constants
    {
        public static string IdentityServerUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
        public static string ClientId = ConfigurationManager.AppSettings["ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        public static string RedirectUri = ConfigurationManager.AppSettings["RedirectUri"];

        public static string AuthorizeEndpoint = IdentityServerUrl + "/connect/authorize";
        public static string LogoutEndpoint = IdentityServerUrl + "/connect/endsession";
        public static string TokenEndpoint = IdentityServerUrl + "/connect/token";
        public static string UserInfoEndpoint = IdentityServerUrl + "/connect/userinfo";
        public static string IdentityTokenValidationEndpoint = IdentityServerUrl + "/connect/identitytokenvalidation";
        public static string TokenRevocationEndpoint = IdentityServerUrl + "/connect/revocation";

        public static string ConfigName = "DiscoveryService";
        public static string ConfigCacheName = "DiscoveryServiceCache";
    }
    
}