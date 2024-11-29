using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LUTE_Server.Data;


using LUTE_Server.DTOs;

namespace LUTE_Server.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SharedVariableController : ControllerBase
    {
        //private readonly ISharedVariableService _service;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<SharedVariableController> _logger;


        public SharedVariableController(ApplicationDbContext context, ILogger<SharedVariableController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> SaveSharedVariable(
            [FromHeader(Name = "X-Secret-Key")] string secretKey,
            [FromBody] SharedVariableDto sharedVariableDto
        )
        {


            //log the request
            _logger.LogInformation("Received request to save shared variable");
            _logger.LogInformation("Secret Key: " + secretKey);

            // Validate secret key
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Missing secret key.");
                return BadRequest("Missing secret key.");
            }

            // Find game with matching secret key
            var game = _context.Games.FirstOrDefault(g => g.SecretKey == secretKey);
            if (game == null)
            {
                _logger.LogWarning("Invalid secret key.");
                return Unauthorized("Invalid secret key.");
            }

            if(sharedVariableDto == null)
            {
                _logger.LogWarning("No shared variable data provided.");
                return BadRequest("No shared variable data provided.");
            }

            _logger.LogInformation("Saving shared variable: {VariableName} for GameId: {GameId}", sharedVariableDto.VariableName, game.Id);

            SharedVariable sharedVariable = new SharedVariable
            {
                GameId = game.Id,
                UUID = sharedVariableDto.UUID,
                VariableName = sharedVariableDto.VariableName,
                VariableType = sharedVariableDto.VariableType,
                Data = sharedVariableDto.Data,
                CreatedAt = sharedVariableDto.CreatedAt
            };


            //we have to check if the variable type and name are already existing in the database by that UUID for that game
            var existingVariable = await _context.SharedVariables
                .Where(v => v.GameId == game.Id && v.UUID == sharedVariableDto.UUID && v.VariableName == sharedVariableDto.VariableName && v.VariableType == sharedVariableDto.VariableType)
                .FirstOrDefaultAsync();

            //if it exists, replace it

            if(existingVariable != null)
            {
                _logger.LogInformation("Updating existing shared variable: {VariableName} for GameId: {GameId}", sharedVariableDto.VariableName, game.Id);
                existingVariable.Data = sharedVariableDto.Data;
                existingVariable.CreatedAt = sharedVariableDto.CreatedAt;
            }
            else
            {
                //if it doesn't exist, add it
                await _context.SharedVariables.AddAsync(sharedVariable);
            }
            


            //await _context.SharedVariables.AddAsync(sharedVariable);

            await _context.SaveChangesAsync();

            return Ok(new { status = "ack" });


        }

        //get shared variables for a game
        //secret key is required to access this endpoint
        //a name for the variable is required
        //number of different variables to return is optional
        [HttpGet]
        public async Task<IActionResult> GetSharedVariables(
            [FromHeader(Name = "X-Secret-Key")] string secretKey,
            [FromQuery] string variableName,
            [FromQuery] int count = 1
        )
        {
            //log the request
            _logger.LogInformation("Received request to get shared variables");
            _logger.LogInformation("Secret Key: " + secretKey);

            // Validate secret key
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Missing secret key.");
                return BadRequest("Missing secret key.");
            }

            // Find game with matching secret key
            var game = _context.Games.FirstOrDefault(g => g.SecretKey == secretKey);
            if (game == null)
            {
                _logger.LogWarning("Invalid secret key.");
                return Unauthorized("Invalid secret key.");
            }

            if(string.IsNullOrEmpty(variableName))
            {
                _logger.LogWarning("No variable name provided.");
                return BadRequest("No variable name provided.");
            }

            _logger.LogInformation("Retrieving up to {Count} shared variables for GameId: {GameId}", count, game.Id);

            //only select the variables that match the game id and the variable name and return only the variable name variable type data and timestamp
            var sharedVariables = await _context.SharedVariables
                .Where(v => v.GameId == game.Id && v.VariableName == variableName)
                .OrderByDescending(v => v.CreatedAt)
                .Take(count)
                .Select(v => new { v.VariableName, v.VariableType, v.Data, v.CreatedAt })
                .ToListAsync();

            return Ok(sharedVariables);
        }

        
    }
}