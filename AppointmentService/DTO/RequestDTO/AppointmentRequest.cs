namespace AppointmentService.DTO.RequestDTO
{
    public class AppointmentRequest
    {
        public int PatientAge { get; set; }
        public string Issue { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
