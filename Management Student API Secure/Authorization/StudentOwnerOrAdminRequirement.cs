using Microsoft.AspNetCore.Authorization;

namespace Management_Student_API_Secure.Authorization
{
    public class StudentOwnerOrAdminRequirement : IAuthorizationRequirement
    {
    }
}
