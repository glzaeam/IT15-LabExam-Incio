using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechCoreSolutions.Models
{
    public class Payroll
    {
        public int PayrollID { get; set; }

        [Required]
        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }

        [ForeignKey("EmployeeID")]
        public Employee? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Payroll Date")]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, 31, ErrorMessage = "Days worked must be between 0 and 31.")]
        [Display(Name = "Days Worked")]
        public int DaysWorked { get; set; }

        [Display(Name = "Gross Pay")]
        public decimal GrossPay { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Deduction cannot be negative.")]
        public decimal Deduction { get; set; }

        [Display(Name = "Net Pay")]
        public decimal NetPay { get; set; }
    }
}