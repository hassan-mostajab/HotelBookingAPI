using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(IRoomRepository roomRepository, ILogger<RoomsController> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }

        // GET: api/rooms
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomRepository.GetAllAsync();
            var roomDtos = rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Type = r.Type,
                PricePerNight = r.PricePerNight,
                Capacity = r.Capacity,
                IsAvailable = r.IsAvailable
            });

            return Ok(roomDtos);
        }

        // GET: api/rooms/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound($"اتاق با شناسه {id} یافت نشد.");

            var roomDto = new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Type = room.Type,
                PricePerNight = room.PricePerNight,
                Capacity = room.Capacity,
                IsAvailable = room.IsAvailable
            };

            return Ok(roomDto);
        }

        // POST: api/rooms
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto roomDto)
        {
            var room = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                Type = roomDto.Type,
                PricePerNight = roomDto.PricePerNight,
                Capacity = roomDto.Capacity,
                IsAvailable = true
            };

            await _roomRepository.AddAsync(room);
            await _roomRepository.SaveChangesAsync();

            roomDto.Id = room.Id;
            return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, roomDto);
        }

        // PUT: api/rooms/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomDto roomDto)
        {
            if (id != roomDto.Id)
                return BadRequest("شناسه در مسیر با بدنه همخوانی ندارد.");

            var existingRoom = await _roomRepository.GetByIdAsync(id);
            if (existingRoom == null)
                return NotFound($"اتاق با شناسه {id} یافت نشد.");

            existingRoom.RoomNumber = roomDto.RoomNumber;
            existingRoom.Type = roomDto.Type;
            existingRoom.PricePerNight = roomDto.PricePerNight;
            existingRoom.Capacity = roomDto.Capacity;
            existingRoom.IsAvailable = roomDto.IsAvailable;

            _roomRepository.Update(existingRoom);
            await _roomRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/rooms/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound($"اتاق با شناسه {id} یافت نشد.");

            _roomRepository.Delete(room);
            await _roomRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}