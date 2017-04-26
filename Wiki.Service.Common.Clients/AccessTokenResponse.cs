namespace Wiki.Service.Common.Clients
{
    /// <summary>
    /// AccessTokenResponse
    /// </summary>
    public class AccessTokenResponse
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Expires
        /// </summary>
        public int Expires { get; set; }

        /// <summary>
        /// TokenType
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// IsError
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public string Error { get; set; }
    }
}