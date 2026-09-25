using Business_Layer;
using DataAccess_Layer;
using Management_Student_API_Secure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Management_Student_API_Secure.Controllers
{
    [Authorize]
    [Route("api/Students")]
    [ApiController]
    public class ManagementStudentApiController : ControllerBase
    {
        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("All", Name = "GetAllStudents")] // Marks this method to respond to HTTP GET requests.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<IEnumerable<StudentDTO>> GetAllStudents() // Define a method to get all students.
        {
            List<StudentDTO> students = clsStudent.GetAllStudents();

            if (students.Count == 0)
            {
                return NotFound("No Students Found!");
            }
            return Ok(students); // Returns the list of students.
        }

       
        [HttpGet("{id}", Name = "GetStudentByID")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDTO>> GetStudentByID(int id,[FromServices] IAuthorizationService authorizationService)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }
            Business_Layer.clsStudent student = Business_Layer.clsStudent.FindByUserId(id);

            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            var authResult = await authorizationService.AuthorizeAsync(
          User,
          student.UserId,
          "StudentOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            //here we get only the DTO object to send it back.
            StudentDTO SDTO = student.S_DTO;

            //we return the DTO not the student object.
            return Ok(SDTO);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost(Name = "AddNewStudent")]

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public ActionResult<StudentDTO> AddnewStudent([FromBody] CreateStudentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentNumber) || string.IsNullOrEmpty(request.Address) || string.IsNullOrEmpty(request.Phone) 
                || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.FirstName))
            {
                return BadRequest($"Invalid student date");
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

            // 3. تهيئة كائن الطالب الجديد بالاستعانة بالمنشئ الافتراضي
            Business_Layer.clsStudent student = new Business_Layer.clsStudent();

            // 4. تعبئة بيانات المستخدم (User Data) وتشفير الباسورد
            student.UserData.Email = request.Email;
            student.UserData.FirstName = request.FirstName;
            student.UserData.LastName = request.LastName;
            student.UserData.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password); // تشفير كلمة المرور هنا

            // 5. تعبئة بيانات الطالب (Student Data)
            student.StudentNumber = request.StudentNumber;
            student.DateOfBirth = request.DateOfBirth;
            student.Phone = request.Phone;
            student.Address = request.Address;

            // 6. استدعاء دالة الحفظ
            if (student.Save(createdByUserId, ipAddress))
            {
                // إرجاع كائن الـ DTO الخاص بالطالب بعد نجاح الحفظ وتوليد الـ ID
                return CreatedAtRoute("GetStudentById", new { id = student.Id }, student.S_DTO);
            }
            else
            {
                return StatusCode(500, "An error occurred while attempting to save the student to the database.");
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentDTO> UpdateStudent(int id, StudentDTO updatedStudent)
        {
            if (id < 1 || updatedStudent == null || string.IsNullOrEmpty(updatedStudent.StudentNumber) || string.IsNullOrEmpty(updatedStudent.Address) || string.IsNullOrEmpty(updatedStudent.Phone))
            {
                return BadRequest("Invalid student data.");
            }

            Business_Layer.clsStudent student = Business_Layer.clsStudent.Find(id);


            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }


      
            student.DateOfBirth = updatedStudent.DateOfBirth;
            student.Phone = updatedStudent.Phone;
            student.Address = updatedStudent.Address;

            // 1. استخراج الـ IP
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // 2. استخراج الـ UserId من الـ Claims
            int createdByUserId = 1; // قيمة افتراضية كـ Admin إن لم يكن الـ Auth مفعل بعد
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                createdByUserId = currentUserId;
            }

            if (student.Save(createdByUserId,ipAddress))

                //we return the DTO not the full student object.
                return Ok(student.S_DTO);
            else
                return StatusCode(500, new { message = "Error Updating Student" });

        }
        [Authorize(Roles = "Admin,Teacher")]

        [HttpDelete("{id}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult DeleteStudent(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
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

            if (Business_Layer.clsStudent.DeleteStudent(id,createdByUserId,ipAddress))

                return Ok($"Student with ID {id} has been deleted.");
            else
                return NotFound($"Student with ID {id} not found. no rows deleted!");
        }
    }
}
