using HospitalApi.DTOs;
using HospitalApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly PatientService _patientService;

    public PatientsController(PatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var result = await _patientService.GetAllPatientsAsync(search);
        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] AssignBedRequestDto request)
    {
        var (result, error) = await _patientService.AssignBedAsync(pesel, request);
        if (error is not null)
            return NotFound(error);

        return Created(string.Empty, result);
    }
}