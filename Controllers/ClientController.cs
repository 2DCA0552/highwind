using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Highwind.Helpers.Interfaces;
using Highwind.Models;
using Highwind.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Highwind.Controllers
{
    [ApiVersion( "1.0" )]
    [Authorize(Policy = "AdministratorsOnly")]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHashHelper _hashHelper;
        private readonly TokenSettings _settings;

        public ClientController(IOptions<TokenSettings> settings, IDataRepository dataRepository, IHashHelper hashHelper){
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _hashHelper = hashHelper ?? throw new ArgumentNullException(nameof(hashHelper));
        }

        /// <summary>
        /// Get All Clients (an entity which describes a subscriber application to Highwind)
        /// </summary>
        // GET client
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Client>), 200)]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients(ApiVersion apiVersion)
        {
            var clients = await _dataRepository.GetClients();
            return Ok(clients);
        }

        /// <summary>
        /// Get a specific Client
        /// </summary>
        // GET client/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Client), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Client>> GetClient([FromRoute] int id, ApiVersion apiVersion)
        {
            var client = await _dataRepository.GetClient(id);

            if(client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        /// <summary>
        /// Post a new Client
        /// </summary>
        // POST client
        [HttpPost]
        [ProducesResponseType(typeof(Client), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PostClient([FromBody] ClientRequest clientRequest, ApiVersion apiVersion)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new Client {
                Application = clientRequest.Application,
                Audience = clientRequest.Audience,
                AppGroupRegexes = clientRequest.AppGroupRegexes,
                CookieDomain = clientRequest.CookieDomain ?? _settings.CookieDomain,
                CookiePath = clientRequest.CookiePath ?? _settings.CookiePath,
                TokenName = clientRequest.TokenName ?? _settings.TokenName,
                ApiKey = Guid.NewGuid().ToString("N")
            };

            await _dataRepository.AddClient(client);
            
            return CreatedAtAction(nameof(ClientController.GetClient), "client", new { id = client.Id, version = apiVersion.ToString() }, client);
        }

        /// <summary>
        /// Put (update) an existing Client - overwrite completely.
        /// </summary>
        // PUT client/5
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PutClient([FromRoute] int id, [FromBody] Client client, ApiVersion apiVersion)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var c = await _dataRepository.GetClient(id);

            if (c == null)
            {
                return NotFound();
            }
            
            await _dataRepository.UpdateClient(id, client);
            
            return Ok();
        }

        /// <summary>
        /// Patch (update) an existing Client - specific fields.
        /// </summary>
        // PATCH client/5
        [HttpPatch("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PatchClient([FromRoute] int id, [FromBody] JsonPatchDocument<Client> jsonPatchDocument, ApiVersion apiVersion)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var c = await _dataRepository.GetClient(id);

            if (c == null)
            {
                return NotFound();
            }

            jsonPatchDocument.ApplyTo(c);
            
            await _dataRepository.UpdateClient(id, c);
            
            return Ok();
        }

        /// <summary>
        /// Delete an existing Client
        /// </summary>
        // DELETE client/5
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteClient([FromRoute] int id, ApiVersion apiVersion)
        {
            var client = await _dataRepository.GetClient(id);

            if (client == null)
            {
                return NotFound();
            }

            await _dataRepository.DeleteClient(id);

            return Ok();
        }
    }
}
