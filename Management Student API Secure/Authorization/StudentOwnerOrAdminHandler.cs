using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Management_Student_API_Secure.Authorization
{
    public class StudentOwnerOrAdminHandler : AuthorizationHandler<StudentOwnerOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerOrAdminRequirement requirement,
        int resourceUserId)
        {
            // Admin override
            if (context.User.IsInRole("Admin") || context.User.IsInRole("Teacher"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Ownership check
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userId, out int authenticatedStudentId) &&
                authenticatedStudentId == resourceUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
