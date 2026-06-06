using HospitalApi.Data;
using HospitalApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public class PatientService(HospitalDbContext db)
{
    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync(string? search)
    {
        throw new NotImplementedException();
    }

    public async Task<(BedAssignmentDto? Result, string? Error)> AssignBedAsync(string pesel, AssignBedRequestDto request)
    {
        throw new NotImplementedException();
    }
}