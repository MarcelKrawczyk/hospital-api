using HospitalApi.Data;
using HospitalApi.DTOs;
using Microsoft.EntityFrameworkCore;
using HospitalApi.Models;


namespace HospitalApi.Services;

public class PatientService(HospitalDbContext db)
{
    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync(string? search)
    {
        throw new NotImplementedException();
    }

    public async Task<(BedAssignmentDto? Result, string? Error)> AssignBedAsync(string pesel,
        AssignBedRequestDto request)
    {
        var patient = await db.Patients.FindAsync(pesel);
        if (patient is null)
            return (null, $"Patient with PESEL '{pesel}' was not found.");

        var ward = await db.Wards.FirstOrDefaultAsync(w => w.Name == request.Ward);
        if (ward is null)
            return (null, $"Ward '{request.Ward}' does not exist.");

        var bedType = await db.BedTypes.FirstOrDefaultAsync(bt => bt.Name == request.BedType);
        if (bedType is null)
            return (null, $"Bed type '{request.BedType}' does not exist.");

        var bed = await db.Beds
            .Include(b => b.BedType)
            .Include(b => b.Room).ThenInclude(r => r.Ward)
            .Where(b =>
                b.BedTypeId == bedType.Id &&
                b.Room.WardId == ward.Id &&
                !b.BedAssignments.Any(ba =>
                    (ba.To == null || ba.To > request.From) &&
                    (request.To == null || request.To > ba.From)))
            .FirstOrDefaultAsync();

        if (bed is null)
            return (null,
                $"No free bed of type '{request.BedType}' in ward '{request.Ward}' for the requested period.");

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = bed.Id,
            From = request.From,
            To = request.To
        };

        db.BedAssignments.Add(assignment);
        await db.SaveChangesAsync();

        return (new BedAssignmentDto
        {
            Id = assignment.Id,
            From = assignment.From,
            To = assignment.To,
            Bed = new BedDto
            {
                Id = bed.Id,
                BedType = new BedTypeDto
                {
                    Id = bed.BedType.Id,
                    Name = bed.BedType.Name,
                    Description = bed.BedType.Description
                },
                Room = new RoomDto
                {
                    Id = bed.Room.Id,
                    HasTv = bed.Room.HasTv,
                    Ward = new WardDto
                    {
                        Id = bed.Room.Ward.Id,
                        Name = bed.Room.Ward.Name,
                        Description = bed.Room.Ward.Description
                    }
                }
            }
        }, null);
    }
}