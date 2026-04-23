using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TechCoreSolutions.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required.")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Daily rate is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Daily rate must be greater than 0.")]
        [Display(Name = "Daily Rate")]
        public decimal DailyRate { get; set; }

        public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    }
}