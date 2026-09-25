using Business_Layer;
using DataAccess_Layer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Management_Student_API_Secure.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AudtingLogsController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet("All", Name = "GetAllAuditLogs")] // Marks this method to respond to HTTP GET requests.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<IEnumerable<AuditLogDTO>> GetAllAuditLogs() // Define a method to get all students.
        {
            List<AuditLogDTO> logs = clsAuditLog.GetAllAuditLogs();

            if (logs.Count == 0)
            {
                return NotFound("No Audit Logs Found!");
            }
            return Ok(logs); // Returns the list of students.
        }
    }
}
