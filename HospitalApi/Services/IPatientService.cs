using HospitalApi.DTOs;

namespace HospitalApi.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllPatientsAsync(string? search);
    Task<(BedAssignmentDto? Result, string? Error)> AssignBedAsync(string pesel, AssignBedRequestDto request);
}