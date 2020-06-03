using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Highwind.Helpers.Interfaces;
using Highwind.Models;
using Highwind.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Highwind.Controllers
{
    [ApiVersion( "1.0" )]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly TokenSettings _settings;
        private readonly IJwtHelper _jwtHelper;
        private readonly IDataRepository _dataRepository;

        public TokenController(IOptions<TokenSettings> settings, IJwtHelper jwtHelper, IDataRepository dataRepository){
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _jwtHelper = jwtHelper ?? throw new ArgumentNullException(nameof(jwtHelper));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
        }

        /// <summary>
        /// Sets an http only cookie with the JWT token as the value based on an apiKey
        /// </summary>
        // GET /token/auth
        [HttpGet("auth/apiKey")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AuthByApiKey([FromQuery] string apiKey, [FromQuery] string redirectURL = null){
            var secure = _settings.CookieSecure ? "Secure;" : "";
            var expires = _settings.CookieExpiryDays == 0 ? "" : $"Expires={DateTime.UtcNow.AddDays(_settings.CookieExpiryDays).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'")};";

            var user = User.Identity;

            // Auth user
            if (user == null){
                if(String.IsNullOrWhiteSpace(redirectURL)){
                    return BadRequest();
                }

                Response.Headers.Add("Set-Cookie", $"{_settings.TokenName}={false}; Domain={_settings.CookieDomain}; SameSite={_settings.SameSite}; Path={_settings.CookiePath};{secure} HttpOnly");
                
                return Redirect(redirectURL);
            }

            var client = await _dataRepository.GetClientByApiKey(apiKey);

            // Auth API Key
            if(client == null){
                if(String.IsNullOrWhiteSpace(redirectURL)){
                    return BadRequest();
                }

                Response.Headers.Add("Set-Cookie", $"{_settings.TokenName}={false}; Domain={_settings.CookieDomain}; SameSite={_settings.SameSite}; Path={_settings.CookiePath};{secure} HttpOnly");
                
                return Redirect(redirectURL);
            }

            var token = _jwtHelper.Create(user, client.Audience, client.AppGroupRegexes);
            
            Response.Headers.Add("Set-Cookie", $"{client.TokenName}={token};{expires} Domain={client.CookieDomain}; SameSite={_settings.SameSite}; Path={client.CookiePath};{secure} HttpOnly");

            if(String.IsNullOrWhiteSpace(redirectURL)){
                return Ok();
            }
            
            return Redirect(redirectURL);
        }

        /// <summary>
        /// Sets an http only cookie with the JWT token as the value based on an application name
        /// </summary>
        // GET /token/auth
        [HttpGet("auth/application")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AuthByApplication([FromQuery] string application, [FromQuery] string redirectURL = null){
            var secure = _settings.CookieSecure ? "Secure;" : "";
            var expires = _settings.CookieExpiryDays == 0 ? "" : $"Expires={DateTime.UtcNow.AddDays(_settings.CookieExpiryDays).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'")};";

            var user = User.Identity;

            // Auth user
            if (user == null){
                if(String.IsNullOrWhiteSpace(redirectURL)){
                    return BadRequest();
                }

                Response.Headers.Add("Set-Cookie", $"{_settings.TokenName}={false}; Domain={_settings.CookieDomain}; SameSite={_settings.SameSite}; Path={_settings.CookiePath};{secure} HttpOnly");
                
                return Redirect(redirectURL);
            }

            var client = await _dataRepository.GetClientByApplication(application);

            // Auth API Key
            if(client == null){
                if(String.IsNullOrWhiteSpace(redirectURL)){
                    return BadRequest();
                }

                Response.Headers.Add("Set-Cookie", $"{_settings.TokenName}={false}; Domain={_settings.CookieDomain}; SameSite={_settings.SameSite}; Path={_settings.CookiePath};{secure} HttpOnly");
                
                return Redirect(redirectURL);
            }

            var token = _jwtHelper.Create(user, client.Audience, client.AppGroupRegexes);
            
            Response.Headers.Add("Set-Cookie", $"{client.TokenName}={token};{expires} Domain={client.CookieDomain}; SameSite={_settings.SameSite}; Path={client.CookiePath};{secure} HttpOnly");

            if(String.IsNullOrWhiteSpace(redirectURL)){
                return Ok();
            }
            
            return Redirect(redirectURL);
        }

        /// <summary>
        /// Returns a JWT token inside a JSON packet based on an api key
        /// </summary>
        // GET /token/auth/bearer
        [HttpGet("auth/bearer/apiKey")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BearerByApiKey([FromQuery] string apiKey){
            var user = User.Identity;

            // Auth user
            if (user == null){
                return BadRequest();
            }

            var client = await _dataRepository.GetClientByApiKey(apiKey);

            // Auth API Key
            if(client == null){
                return BadRequest();
            }

            var token = _jwtHelper.Create(user, client.Audience, client.AppGroupRegexes);
            
            return Ok(new { token = token });
        }

        /// <summary>
        /// Returns a JWT token inside a JSON packet based on an application name
        /// </summary>
        // GET /token/auth/bearer
        [HttpGet("auth/bearer/application")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BearerByAppication([FromQuery] string application){
            var user = User.Identity;

            // Auth user
            if (user == null){
                return BadRequest();
            }

            var client = await _dataRepository.GetClientByApplication(application);

            // Auth API Key
            if(client == null){
                return BadRequest();
            }

            var token = _jwtHelper.Create(user, client.Audience, client.AppGroupRegexes);
            
            return Ok(new { token = token });
        }
        
        /// <summary>
        /// Validate an existing JWT.
        /// </summary>
        // POST/token/validate
        [HttpPost("validate")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public ActionResult<bool> Validate([FromBody] ValidateRequest request){
            return Ok(_jwtHelper.ValidateToken(request.Token, request.ValidAudience));
        }

        /// <summary>
        /// Read an existing JWT.
        /// </summary>
        // POST/token/read
        [HttpPost("read")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public ActionResult<bool> Read([FromBody] ReadRequest jwt){
            return Ok(_jwtHelper.ReadToken(jwt.Token));
        }
    }
}
