using AppointmentService.DTO.RequestDTO;
using AppointmentService.DTO.ResponseDTO;
using AppointmentService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointment _appointmentService;
        public AppointmentController(IAppointment appointmentService)
        {
            _appointmentService = appointmentService;
           
        }
        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequest appointment, int DoctorID)
        {
            try
            {
                int patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var patientName = User.FindFirstValue(ClaimTypes.Name);
                var addedAppointment = await _appointmentService.CreateAppointment(appointment, patientId, DoctorID,patientName);
                return Ok(new { Success = true, Message = "Appointment added successfully", Data = addedAppointment });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = $"An error occurred while Creating appointment: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("getbypatient")]
        public async Task<IActionResult> GetAllAppointmentsByPatient()
        {
            try
            {
                var patientId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var appointments = await _appointmentService.GetAllAppointmentsByPatient(patientId);
                return Ok(new { Success = true, Message = "Data Fetched successfully", Data = appointments});
            }
            catch (Exception ex)
            {
                return Ok( $"An error occurred while retrieving appointments by doctor: {ex.Message}");
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("GetByDoctor")]
        public async Task<IActionResult> GetAllAppointmentsByDoctor()
        {
            try
            {
                var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var appointments = await _appointmentService.GetAllAppointmentsByDoctor(doctorId);
                return Ok(new { Success = true, Message = "Data Fetched successfully", Data = appointments });
            }
            catch (Exception ex)
            {
                return Ok( $"An error occurred while retrieving appointments by doctor: {ex.Message}");
            }
        }

        [Authorize(Roles = "Patient,Doctor,Admin")]
        [HttpGet("GetAppointmentById")]
        public async Task<IActionResult> GetAppointmentsById()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var appointments = await _appointmentService.GetAppointmentsById(userId);
                return Ok(new { Success = true, Message = "Data Fetched successfully", Data = appointments });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = $"An error occurred while retrieving appointments by patient: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("UpdateAppointment")]
        public async Task<IActionResult> UpdateAppointment(AppointmentRequest request, int AppointmentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var patientName = User.FindFirstValue(ClaimTypes.Name);
                var appointments = await _appointmentService.UpdateAppointment(request, userId, AppointmentId, patientName);
                return Ok(new { Success = true, Message = "Appointments Updated successfully", Data = appointments });
            }
            catch(Exception ex)
            {
                return Ok(new { Success = false, Message = $"An error occurred while Updaing appointments by patient: {ex.Message}" });
            }
        }
        [Authorize(Roles = "Patient")]
        [HttpDelete("cancelappointment")]
        public async Task<IActionResult> CancelAppointment( int AppointmentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var appointments= await _appointmentService.CancelAppointment(userId, AppointmentId);
                return Ok(new { Success = true, Message = "Appointment Cancelled successfully", Data = appointments });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = $"An error occurred while Cancelling appointments by patient: {ex.Message}" });
            }

        }
        [Authorize(Roles = "Doctor")]
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int Appointmentid, string status)
        {
            try
            {
                var appointments = Ok(await _appointmentService.UpdateStatus(Appointmentid, status));
                return Ok(new { Success = true, Message = "Appointment Status Updated successfully", Data = appointments });
            }
            catch(Exception ex) 
            {
                return Ok(new {Success=false,Message = $"An error occurred while Updating appointments by Doctor: {ex.Message}");
            }
        }
    }
}
