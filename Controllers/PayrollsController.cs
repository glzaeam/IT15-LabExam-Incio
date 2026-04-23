using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechCoreSolutions.Data;
using TechCoreSolutions.Models;

namespace TechCoreSolutions.Controllers
{
    public class PayrollsController : Controller
    {
        private readonly AppDbContext _context;

        public PayrollsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? employeeId)
        {
            var query = _context.Payrolls.Include(p => p.Employee).AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeID == employeeId.Value);
                ViewBag.FilteredEmployee = await _context.Employees.FindAsync(employeeId.Value);
            }

            ViewBag.Employees = await _context.Employees.ToListAsync();
            ViewBag.EmployeeList = await _context.Employees.ToListAsync();
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.PreselectedEmployeeId = employeeId;

            return View(await query.OrderByDescending(p => p.Date).ToListAsync());
        }

        public async Task<IActionResult> Create(int? employeeId)
        {
            // Just redirect to Index - the modal form is on the Index page
            return RedirectToAction(nameof(Index), new { employeeId = employeeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payroll payroll)
        {
            var employee = await _context.Employees.FindAsync(payroll.EmployeeID);
            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            payroll.GrossPay = employee.DailyRate * payroll.DaysWorked;
            payroll.NetPay = payroll.GrossPay - payroll.Deduction;

            if (payroll.NetPay < 0)
            {
                TempData["Error"] = "Deduction cannot exceed Gross Pay.";
                return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
            }

            if (ModelState.IsValid)
            {
                _context.Add(payroll);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payroll record added successfully!";
                return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
            }

            TempData["Error"] = "Failed to add payroll record. Please check the form.";
            return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            // Just redirect to Index - the modal form is on the Index page
            if (id == null) return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payroll payroll)
        {
            if (id != payroll.PayrollID) return NotFound();

            var employee = await _context.Employees.FindAsync(payroll.EmployeeID);
            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            payroll.GrossPay = employee.DailyRate * payroll.DaysWorked;
            payroll.NetPay = payroll.GrossPay - payroll.Deduction;

            if (payroll.NetPay < 0)
            {
                TempData["Error"] = "Deduction cannot exceed Gross Pay.";
                return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payroll);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Payroll updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Payrolls.Any(p => p.PayrollID == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
            }

            TempData["Error"] = "Failed to update payroll record. Please check the form.";
            return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.PayrollID == id);
            if (payroll == null) return NotFound();
            return View(payroll);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            int empId = payroll?.EmployeeID ?? 0;
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payroll record deleted!";
            }
            return RedirectToAction(nameof(Index), new { employeeId = empId });
        }
    }
}