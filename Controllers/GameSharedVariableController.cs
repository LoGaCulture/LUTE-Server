using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Services;
using LUTE_Server.Models;
using LUTE_Server.DTOs;

namespace LUTE_Server.Controllers
{
    [Route("games/{gameId}/shared-variables")]
    [ApiController]
    public class GameSharedVariableController : ControllerBase
    {
        private readonly IGameSharedVariableService _sharedVariableService;

        public GameSharedVariableController(IGameSharedVariableService sharedVariableService)
        {
            _sharedVariableService = sharedVariableService;
        }

        // GET: /games/{gameId}/shared-variables?type=Stone&limit=5
        [HttpGet]
        public async Task<IActionResult> GetSharedVariables(int gameId, [FromQuery] string? type = null, [FromQuery] int? limit = null)
        {
            var variables = await _sharedVariableService.GetSharedVariablesAsync(gameId, type, limit);
            return Ok(variables);
        }

        // POST: /games/{gameId}/shared-variables
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateSharedVariable(int gameId, [FromBody] SharedVariableDto sharedVariableDto)
        {
            await _sharedVariableService.CreateOrUpdateSharedVariable(gameId, sharedVariableDto);
            return Ok();
        }

        [HttpPut("{variableName}")]
        public async Task<IActionResult> UpdateSharedVariable(int gameId, string variableName, [FromBody] SharedVariableDto sharedVariableDto)
        {

            // print the request for debugging
            Console.WriteLine($"Update request: {variableName}, {sharedVariableDto}");

            if (sharedVariableDto == null || sharedVariableDto.VariableName != variableName)
            {
                return BadRequest("Invalid shared variable data.");
            }

            // Call the service to update the shared variable
            await _sharedVariableService.CreateOrUpdateSharedVariable(gameId, sharedVariableDto);

            return Ok(new { status = "ack" });  // Acknowledge successful update
        }


        // DELETE: /games/{gameId}/shared-variables/{variableName}
        [HttpDelete("{variableName}")]
        public async Task<IActionResult> DeleteSharedVariable(int gameId, string variableName)
        {
            await _sharedVariableService.DeleteSharedVariable(gameId, variableName);
            return Ok();
        }
    }
}
