using AppointmentService.Context;
using AppointmentService.DTO.RequestDTO;
using AppointmentService.DTO.ResponseDTO;
using AppointmentService.Entity;
using AppointmentService.Interface;
using Dapper;
using System.Data.SqlClient;

namespace Service
{
    public class AppointmentServices:IAppointment
    {
        private readonly AppointmentContext _context;
        private readonly IHttpClientFactory httpClientFactory;

        public AppointmentServices(AppointmentContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<int> CreateAppointment(AppointmentRequest appointment, int PatientID, int DoctorID,string patientName)
        {
            try
            {
                string insertQuery = @"INSERT INTO APPOINTMENT (PatientName, PatientAge, Issue, DoctorName, Specialization,
                              AppointmentDate, Status, BookedWith, BookedBy)
                              VALUES (@PatientName, @PatientAge, @Issue, @DoctorName, @Specialization,
                              @AppointmentDate, @Status, @BookedWith, @BookedBy);";
                Appointment appointmentEntity = MapToEntity(appointment, getDoctorById(DoctorID), PatientID,patientName);
                using (var connection = _context.CreateConnection())
                {
                    var appointmentId = await connection.ExecuteAsync(insertQuery, appointmentEntity);

                    return appointmentId;
                }            
            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.StackTrace);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private Appointment MapToEntity(AppointmentRequest request, Doctor userObject, int PatientId,string patientName)
        {
            return new Appointment
            {
                PatientName = patientName,
                PatientAge = request.PatientAge,
                Issue = request.Issue,
                DoctorName = userObject?.DoctorName ?? "", 
                Specialization = userObject?.Specialization ?? "", 
                AppointmentDate = DateTime.Now,
                Status = false,
                BookedWith = userObject?.DoctorId ?? 0, 
                BookedBy = PatientId
            };
        }
        public Doctor getDoctorById(int doctorId)
        {
            try
            {
                var httpclient = httpClientFactory.CreateClient("GetByDoctorId");
                var response = httpclient.GetAsync($"GetDoctorById?doctorId={doctorId}").Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadFromJsonAsync<Doctor>().Result;
                }
                throw new Exception("DoctorNotFound Create Appointment FIRST TO TRY DIFFERENT  DoctorID");
            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<AppointmentResponse>> GetAllAppointmentsByPatient(int patientId)
        {
            try
            {
                string selectQuery = @"SELECT * FROM APPOINTMENT WHERE BookedBy = @PatientId;";
                using (var connection = _context.CreateConnection())
                {
                    return await connection.QueryAsync<AppointmentResponse>(selectQuery, new { PatientId = patientId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving appointments by patient.", ex);
            }
        }

        public async Task<IEnumerable<AppointmentResponse>> GetAllAppointmentsByDoctor(int doctorId)
        {
            try
            {
                string selectQuery = @"SELECT * FROM APPOINTMENT WHERE BookedWith = @DoctorId;";
                using (var connection = _context.CreateConnection())
                {
                    return await connection.QueryAsync<AppointmentResponse>(selectQuery, new { DoctorId = doctorId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving appointments by doctor.", ex);
            }
        }

        public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsById(int appointmentId)
        {
            try
            {
                string selectQuery = @"SELECT * FROM APPOINTMENT WHERE AppointmentId = @AppointmentId;";
                using (var connection = _context.CreateConnection())
                {
                    return await connection.QueryAsync<AppointmentResponse>(selectQuery, new { AppointmentId = appointmentId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving appointments by doctor.", ex);
            }
        }

        public async Task<AppointmentResponse> UpdateAppointment(AppointmentRequest request, int patientId, int AppointmentId,string patientName)
        {
            try
            {
                Appointment existingAppointment = GetAppointmentsbyId(AppointmentId);
                if (existingAppointment == null)
                {
                    throw new Exception("Appointment not found");
                }
                existingAppointment.PatientName = patientName;
                existingAppointment.PatientAge = request.PatientAge;
                existingAppointment.Issue = request.Issue;
                existingAppointment.AppointmentDate = request.AppointmentDate;
                string sql = @" UPDATE APPOINTMENT SET PatientAge=@PatientAge, Issue = @Issue, BookedBy = @BookedBy, AppointmentDate = @AppointmentDate WHERE AppointmentId = @Appointmentid";
                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(sql, existingAppointment);

                    // Assuming AppointmentResponseDto needs to be retrieved after the update
                    return await connection.QueryFirstOrDefaultAsync<AppointmentResponse>("SELECT * FROM APPOINTMENT WHERE AppointmentId = @AppointmentId", new { Appointmentid = AppointmentId });
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> CancelAppointment(int patientId, int AppointmentId)
        {
            try
            {
                Appointment existingAppointment = GetAppointmentsbyId(AppointmentId);
                if (existingAppointment == null)
                {
                    throw new Exception("Appointment not found");
                }
                string sql = "Delete from APPOINTMENT where AppointmentId=@appointmentid and BookedBy=@userid";
                var result = await _context.CreateConnection().ExecuteAsync(sql, new { appointmentid = AppointmentId, userid = patientId });
                return result > 0;
            }
            catch(Exception ex) { throw new Exception(ex.Message); }
        }


        public async Task<AppointmentResponse> UpdateStatus(int appointmentId, string status)
        {
            try
            {
                string query = "UPDATE APPOINTMENT SET Status = @Status WHERE AppointmentId = @AppointmentId";
                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(query, new { Status = status, AppointmentId = appointmentId });
                    // Assuming AppointmentResponseDto needs to be retrieved after the update
                    return await connection.QueryFirstOrDefaultAsync<AppointmentResponse>("SELECT * FROM APPOINTMENT WHERE AppointmentId = @AppointmentId", new { AppointmentId = appointmentId });
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        private Appointment GetAppointmentsbyId(int appointmentId)
        {
            try
            {
                string selectQuery = @"SELECT * FROM APPOINTMENT WHERE AppointmentId = @AppointmentId;";
                using (var connection = _context.CreateConnection())
                {
                    return connection.QueryFirstOrDefault<Appointment>(selectQuery, new { AppointmentId = appointmentId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving appointment by ID.", ex);
            }
        }


    }
}
