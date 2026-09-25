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
    public class UsresController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet("All", Name = "GetAllUsers")] // Marks this method to respond to HTTP GET requests.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<IEnumerable<UsersDTO>> GetAllUsers() // Define a method to get all students.
        {
            List<UsersDTO> students = clsUser.GetAllUsers();

            if (students.Count == 0)
            {
                return NotFound("No Users Found!");
            }
            return Ok(students); // Returns the list of students.
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetUserBy{id}", Name = "GetUserByID")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UsersDTO> GetUserByID(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }
            Business_Layer.clsUser user = Business_Layer.clsUser.Find(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            //here we get only the DTO object to send it back.
            UsersDTO SDTO = user.U_DTO;

            //we return the DTO not the student object.
            return Ok(SDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddNewUser", Name = "AddNewUser")]

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public ActionResult<StudentDTO> AddnewUser(UsersDTO newUserDTO)
        {
            if (newUserDTO == null || string.IsNullOrEmpty(newUserDTO.LastName) || string.IsNullOrEmpty(newUserDTO.FirstName) || string.IsNullOrEmpty(newUserDTO.Email) || string.IsNullOrEmpty(newUserDTO.Role))
            {
                return BadRequest($"Invalid user date");
            }
            // 1. استخراج الـ IP
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // 2. استخدام الـ Constructor الافتراضي لضمان أن الـ Mode = AddNew
            Business_Layer.clsUser user = new Business_Layer.clsUser();

            // 3. تعبئة البيانات وتشفير كلمة المرور
            user.Email = newUserDTO.Email;
            user.FirstName = newUserDTO.FirstName;
            user.LastName = newUserDTO.LastName;
            user.Role = newUserDTO.Role;
            user.IsActive = newUserDTO.IsActive;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserDTO.PasswordHash);

            // 4. الحفظ
            if (user.Save(ipAddress))
            {
                newUserDTO.Id = user.Id;
                newUserDTO.PasswordHash = user.PasswordHash; // تحديث الهاش في الـ DTO للإرجاع

                return CreatedAtRoute("GetUserByID", new { id = newUserDTO.Id }, newUserDTO);
            }

            // في حال فشل الحفظ
            return StatusCode(500, "Error adding user to the database.");

           
        }

        [Authorize(Roles = "Admin")]

        [HttpPut("InActiveUser{id}", Name = "InActiveUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentDTO> UpdateStause(int id, bool newStatus)
        {
            if (id < 1 )
            {
                return BadRequest("Invalid User data.");
            }

            Business_Layer.clsUser user = Business_Layer.clsUser.Find(id);


            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // 1. استخراج الـ IP
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // 2. استخراج الـ UserId من الـ Claims
            int createdByUserId = 1; // قيمة افتراضية كـ Admin إن لم يكن الـ Auth مفعل بعد
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                createdByUserId = currentUserId;
            }

            user.IsActive = newStatus;
           

          
            if (user.ChangeStatus(newStatus,createdByUserId, ipAddress))

                //we return the DTO not the full student object.
                return Ok(user.U_DTO);
            else
                return StatusCode(500, new { message = "Error Updating User" });

        }
    }
}
