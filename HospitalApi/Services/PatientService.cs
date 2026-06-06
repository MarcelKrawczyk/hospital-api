using HospitalApi.Data;
using HospitalApi.DTOs;
using HospitalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public class PatientService
{
    private readonly HospitalDbContext _db;

    public PatientService(HospitalDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync(string? search)
    {
        var query = _db.Patients
            .Include(p => p.Admissions).ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments).ThenInclude(ba => ba.Bed).ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments).ThenInclude(ba => ba.Bed).ThenInclude(b => b.Room).ThenInclude(r => r.Ward)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, $"%{search}%") ||
                EF.Functions.Like(p.LastName, $"%{search}%"));
        }

        var patients = await query.ToListAsync();

        return patients.Select(p => new PatientDto
        {
            Pesel = p.Pesel,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Age = p.Age,
            Sex = p.Sex ? "Male" : "Female",
            Admissions = p.Admissions.Select(a => new AdmissionDto
            {
                Id = a.Id,
                AdmissionDate = a.AdmissionDate,
                DischargeDate = a.DischargeDate,
                Ward = new WardDto
                {
                    Id = a.Ward.Id,
                    Name = a.Ward.Name,
                    Description = a.Ward.Description
                }
            }).ToList(),
            BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
            {
                Id = ba.Id,
                From = ba.From,
                To = ba.To,
                Bed = new BedDto
                {
                    Id = ba.Bed.Id,
                    BedType = new BedTypeDto
                    {
                        Id = ba.Bed.BedType.Id,
                        Name = ba.Bed.BedType.Name,
                        Description = ba.Bed.BedType.Description
                    },
                    Room = new RoomDto
                    {
                        Id = ba.Bed.Room.Id,
                        HasTv = ba.Bed.Room.HasTv,
                        Ward = new WardDto
                        {
                            Id = ba.Bed.Room.Ward.Id,
                            Name = ba.Bed.Room.Ward.Name,
                            Description = ba.Bed.Room.Ward.Description
                        }
                    }
                }
            }).ToList()
        });
    }

    public async Task<(BedAssignmentDto? Result, string? Error)> AssignBedAsync(string pesel, AssignBedRequestDto request)
    {
        var patient = await _db.Patients.FindAsync(pesel);
        if (patient is null)
            return (null, $"Patient with PESEL '{pesel}' was not found.");

        var ward = await _db.Wards.FirstOrDefaultAsync(w => w.Name == request.Ward);
        if (ward is null)
            return (null, $"Ward '{request.Ward}' does not exist.");

        var bedType = await _db.BedTypes.FirstOrDefaultAsync(bt => bt.Name == request.BedType);
        if (bedType is null)
            return (null, $"Bed type '{request.BedType}' does not exist.");

        var bed = await _db.Beds
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
            return (null, $"No free bed of type '{request.BedType}' in ward '{request.Ward}' for the requested period.");

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = bed.Id,
            From = request.From,
            To = request.To
        };

        _db.BedAssignments.Add(assignment);
        await _db.SaveChangesAsync();

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