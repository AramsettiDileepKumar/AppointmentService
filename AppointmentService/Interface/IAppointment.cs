using AppointmentService.DTO.RequestDTO;
using AppointmentService.DTO.ResponseDTO;

namespace AppointmentService.Interface
{
    public interface IAppointment
    {
        public Task<int> CreateAppointment(AppointmentRequest appointment, int PatientID, int DoctorID,string patientname);
       
        public Task<IEnumerable<AppointmentResponse>> GetAllAppointmentsByDoctor(int doctorId);
        public Task<IEnumerable<AppointmentResponse>> GetAllAppointmentsByPatient(int patientId);
        public Task<IEnumerable<AppointmentResponse>> GetAppointmentsById(int appointmentId);
        public Task<AppointmentResponse> UpdateAppointment(AppointmentRequest request, int patientId, int AppointmentId,string patientName);
        public Task<bool> CancelAppointment( int patientId, int AppointmentId);
        public Task<AppointmentResponse> UpdateStatus(int appointmentId, string status);


    }
}
