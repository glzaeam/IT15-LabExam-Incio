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
            ViewBag.SelectedEmployeeId = employeeId;

            return View(await query.OrderByDescending(p => p.Date).ToListAsync());
        }

        public async Task<IActionResult> Create(int? employeeId)
        {
            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(),
                "EmployeeID", "FirstName");
            ViewBag.EmployeeList = await _context.Employees.ToListAsync();
            ViewBag.PreselectedEmployeeId = employeeId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payroll payroll)
        {
            var employee = await _context.Employees.FindAsync(payroll.EmployeeID);
            if (employee == null)
            {
                ModelState.AddModelError("", "Employee not found.");
            }
            else
            {
                payroll.GrossPay = employee.DailyRate * payroll.DaysWorked;
                payroll.NetPay = payroll.GrossPay - payroll.Deduction;

                if (payroll.NetPay < 0)
                    ModelState.AddModelError("Deduction", "Deduction cannot exceed Gross Pay.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(payroll);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payroll record added successfully!";
                return RedirectToAction(nameof(Index), new { employeeId = payroll.EmployeeID });
            }

            ViewBag.EmployeeList = await _context.Employees.ToListAsync();
            ViewBag.PreselectedEmployeeId = payroll.EmployeeID;
            return View(payroll);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.PayrollID == id);
            if (payroll == null) return NotFound();

            ViewBag.EmployeeList = await _context.Employees.ToListAsync();
            return View(payroll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payroll payroll)
        {
            if (id != payroll.PayrollID) return NotFound();

            var employee = await _context.Employees.FindAsync(payroll.EmployeeID);
            if (employee == null)
            {
                ModelState.AddModelError("", "Employee not found.");
            }
            else
            {
                payroll.GrossPay = employee.DailyRate * payroll.DaysWorked;
                payroll.NetPay = payroll.GrossPay - payroll.Deduction;

                if (payroll.NetPay < 0)
                    ModelState.AddModelError("Deduction", "Deduction cannot exceed Gross Pay.");
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

            ViewBag.EmployeeList = await _context.Employees.ToListAsync();
            return View(payroll);
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