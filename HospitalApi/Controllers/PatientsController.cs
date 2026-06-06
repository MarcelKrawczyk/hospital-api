using HospitalApi.DTOs;
using HospitalApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController(PatientService patientService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var result = await patientService.GetAllPatientsAsync(search);
        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] AssignBedRequestDto request)
    {
        var (result, error) = await patientService.AssignBedAsync(pesel, request);
        if (error is not null)
            return NotFound(error);

        return Created(string.Empty, result);
    }
}