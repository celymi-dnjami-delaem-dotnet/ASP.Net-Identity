using System;

namespace IdentityWebApi.PL.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        
        public string UserName { get; set; }
        
        public string Password { get; set; }
        
        public string UserRole { get; set; }
        
        public string Email { get; set; }
    }
}