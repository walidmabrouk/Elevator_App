using crudmongo.Models;
using crudmongo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace crudmongo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElevatorsController : ControllerBase
    {
        private readonly ElevatorService _elevatorService;
        private readonly ILogger<ElevatorsController> _logger;

        public ElevatorsController(ElevatorService elevatorService, ILogger<ElevatorsController> logger)
        {
            _elevatorService = elevatorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var elevators = await _elevatorService.GetAllAsync();
            return Ok(elevators);
        }



        // Get a specific elevator by ID
        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"Fetching elevator with ID: {id}");
            var existingElevator = await _elevatorService.GetAsync(id);
            if (existingElevator is null)
            {
                _logger.LogWarning($"Elevator with ID: {id} not found.");
                return NotFound();
            }

            return Ok(existingElevator);
        }

        // Create a new elevator
        [HttpPost]
        public async Task<IActionResult> Post(Elevator elevator)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid elevator data received.");
                return BadRequest(ModelState);
            }

            await _elevatorService.CreateAsync(elevator);
            _logger.LogInformation($"Elevator created with ID: {elevator.Id}");
            return CreatedAtAction(nameof(Get), new { id = elevator.Id }, elevator);
        }

        // Update an existing elevator
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Elevator elevator)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid elevator data received for update.");
                return BadRequest(ModelState);
            }

            var existingElevator = await _elevatorService.GetAsync(id);
            if (existingElevator is null)
            {
                _logger.LogWarning($"Elevator with ID: {id} not found for update.");
                return NotFound();
            }

            elevator.Id = id; // Ensure the elevator ID matches the route parameter
            await _elevatorService.UpdateAsync(elevator);

            _logger.LogInformation($"Elevator with ID: {id} updated successfully.");
            return NoContent();
        }

        // Delete an elevator
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"Deleting elevator with ID: {id}");
            var existingElevator = await _elevatorService.GetAsync(id);
            if (existingElevator is null)
            {
                _logger.LogWarning($"Elevator with ID: {id} not found for deletion.");
                return NotFound();
            }

            await _elevatorService.RemoveAsync(id);

            _logger.LogInformation($"Elevator with ID: {id} deleted successfully.");
            return NoContent();
        }
    }
}