using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DogHub.HelpingClasses
{
    public class UserDTO
    {
        public int PK_UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
    }

}