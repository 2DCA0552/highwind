namespace Highwind.Settings
{
    public class TokenSettings
    {
        public string CookieDomain { get; set; }
        public int CookieExpiryDays { get; set; }
        public string CookiePath { get; set; }
        public bool CookieSecure { get; set; }
        public string HmacSecretKey { get; set; }
        public bool IncludeGroupSIdClaims { get; set; }
        public bool IncludeGroupRoleClaims { get; set; }
        public string Issuer { get; set; }
        public string RsaPrivateKeyXml { get; set; }
        public string RsaPublicKeyXml { get; set; }
        public string SameSite { get; set; }
        public int TokenExpiryDays { get; set; }
        public int TokenExpiryMinutes { get; set; }
        public string TokenName { get; set; }
        public bool UseRsa { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool ValidateLifetime { get; set; }
    }
}