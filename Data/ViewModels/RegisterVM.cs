using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SchoolApp.Api.Data.ViewModels
{
    public class RegisterVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [MinLength(6)] 
        public string Password { get; set; } 
    }
}
