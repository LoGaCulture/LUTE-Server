using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LUTE_Server.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SharedVariableController : ControllerBase
    {
        private readonly IGameSharedVariableService _service;

        public SharedVariableController(IGameSharedVariableService service)
        {
            _service = service;
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<IEnumerable<GameSharedVariable>>> GetSharedVariables(int gameId)
        {
            var variables = await _service.GetSharedVariablesAsync(gameId);
            return Ok(variables);
        }

        [HttpGet("{gameId}/user/{uuid}")]
        public async Task<ActionResult<IEnumerable<GameSharedVariable>>> GetSharedVariablesForUser(int gameId, string uuid)
        {
            var variables = await _service.GetSharedVariablesForUserAsync(gameId, uuid);
            return Ok(variables);
        }

        [HttpPost]
        public async Task<ActionResult> AddSharedVariable(GameSharedVariable sharedVariable)
        {
            await _service.AddSharedVariableAsync(sharedVariable);
            return CreatedAtAction(nameof(GetSharedVariables), new { id = sharedVariable.Id }, sharedVariable);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSharedVariable(int id, [FromBody] GameSharedVariable sharedVariable)
        {
            // Ensure the id matches the shared variable ID to avoid conflicts
            if (id != sharedVariable.Id)
            {
                return BadRequest("Shared variable ID mismatch.");
            }

            // Update the shared variable in the database (handled by your service)
            var result = await _service.UpdateSharedVariableAsync(sharedVariable);

            if (result == null)
            {
                return NotFound("Shared variable not found.");
            }

            // Return an acknowledgment once the shared variable is successfully updated
            return Ok(new { status = "ack" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSharedVariable(int id)
        {
            await _service.DeleteSharedVariableAsync(id);
            return NoContent();
        }
    }
}