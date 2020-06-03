using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Highwind.Helpers.Interfaces;
using Highwind.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Highwind.Helpers
{
    public class JwtHelper : IJwtHelper
    {
        private readonly TokenSettings _settings;
        private readonly IXmlHelper _xmlHelper;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private SecurityKey _issuerSigningKey;
        private ILogger<JwtHelper> _logger;
        private SigningCredentials _signingCredentials;
        private JwtHeader _jwtHeader;

        public JwtHelper(IOptions<TokenSettings> settings, ILogger<JwtHelper> logger, IXmlHelper xmlHelper){
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xmlHelper = xmlHelper ?? throw new ArgumentNullException(nameof(xmlHelper));

            if(_settings.UseRsa)
            {
                InitializeRsa();
            }
            else
            {
                InitializeHmac();
            }

            InitializeJwtParameters();
        }

        private void InitializeRsa()
        {
            var publicKeyXml = File.ReadAllText(_settings.RsaPublicKeyXml);
            var publicRsa = _xmlHelper.FromXmlString(publicKeyXml);

            _issuerSigningKey = new RsaSecurityKey(publicRsa);

            if(string.IsNullOrWhiteSpace(_settings.RsaPrivateKeyXml))
            {
                return;
            }

            var privateKeyXml = File.ReadAllText(_settings.RsaPrivateKeyXml);
            var privateRsa = _xmlHelper.FromXmlString(privateKeyXml);
            var privateKey = new RsaSecurityKey(privateRsa);

             _signingCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);
        }

        private void InitializeHmac()
        {
            _issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.HmacSecretKey));
            _signingCredentials = new SigningCredentials(_issuerSigningKey, SecurityAlgorithms.HmacSha256); 
        }
        
        private void InitializeJwtParameters()
        {
            _jwtHeader = new JwtHeader(_signingCredentials);
        }

        public string Create(IIdentity identity, string audience, List<string> appGroupRegexes)
        {
            var nowUtc = DateTime.UtcNow;
            var expires = _settings.TokenExpiryMinutes > 0 ? nowUtc.AddMinutes(_settings.TokenExpiryMinutes) : nowUtc.AddDays(_settings.TokenExpiryDays);
            var centuryBegin = new DateTime(1970, 1, 1);
            var exp = (long)(new TimeSpan(expires.Ticks - centuryBegin.Ticks).TotalSeconds);
            var now = (long)(new TimeSpan(nowUtc.Ticks - centuryBegin.Ticks).TotalSeconds);
            var issuer = _settings.Issuer ?? string.Empty;
            var payload = new JwtPayload
            {
                {"sub", identity.Name},
                {"unique_name", identity.Name},
                {"iss", issuer},
                {"aud", audience},
                {"iat", now},
                {"nbf", now},
                {"exp", exp},
                {"jti", Guid.NewGuid().ToString("N")}
            };

            var windowsUser = identity as WindowsIdentity;

            // SId
            var claim = windowsUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid);
            if(claim != null){
                payload.AddClaim(new Claim("sid", claim.Value));
            }       

            // Group SId
            if(_settings.IncludeGroupSIdClaims){
                var groupClaims = windowsUser.Claims.Where(c => c.Type == ClaimTypes.GroupSid);
                if(groupClaims != null){
                    foreach(Claim c in groupClaims){
                        payload.AddClaim(new Claim(c.Type, c.Value));
                    }
                }
            }

            // Group Roles
            if(_settings.IncludeGroupRoleClaims){
                var groupClaims = windowsUser.Claims.Where(c => c.Type == ClaimTypes.GroupSid);
                if(groupClaims != null){
                    foreach(Claim c in groupClaims){
                        var role = new System.Security.Principal.SecurityIdentifier(c.Value).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                        foreach(var regex in appGroupRegexes) {
                            if(Regex.IsMatch(role, regex)) {
                                payload.AddClaim(new Claim("role", role));
                            }
                        }
                    }
                }
            }
            
            var jwt = new JwtSecurityToken(_jwtHeader, payload);
           
            return _jwtSecurityTokenHandler.WriteToken(jwt);
        }

        public bool ValidateToken(string jwt, string validAudience)
        {
            var parameters = new TokenValidationParameters
            {
                ValidAudience = validAudience,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = _settings.ValidateAudience,
                ValidateIssuer = _settings.ValidateIssuer,
                ValidateIssuerSigningKey = _settings.ValidateIssuerSigningKey,
                ValidateLifetime = _settings.ValidateLifetime,
                IssuerSigningKey = _issuerSigningKey
            }; 

            SecurityToken validatedToken;
            
            try
            {
                _jwtSecurityTokenHandler.ValidateToken(jwt, parameters, out validatedToken);
            }
            catch(SecurityTokenException)
            {
                return false;
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Exception occurred validating security token.");
                throw e;
            }
            
            return true;
    
        }

        public dynamic ReadToken(string jwt)
        {
            JwtSecurityToken token = _jwtSecurityTokenHandler.ReadJwtToken(jwt);

            return new {
                id = token.Id,
                header = token.Header,
                payload = token.Payload,
                validFrom = token.ValidFrom,
                validTo = token.ValidTo
            };
        }
    }
}