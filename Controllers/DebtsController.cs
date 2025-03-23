using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShareCare.Data;
using ShareCare.Models;

namespace ShareCare.Controllers
{
    public class DebtsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DebtsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Debts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Debt.Include(d => d.OwerUser).Include(d => d.Purchase).Include(d => d.UploaderUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Debts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debt
                .Include(d => d.OwerUser)
                .Include(d => d.Purchase)
                .Include(d => d.UploaderUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debt == null)
            {
                return NotFound();
            }

            return View(debt);
        }

        // GET: Debts/Create
        public IActionResult Create()
        {
            ViewData["OwerUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["PurchaseId"] = new SelectList(_context.Purchase, "Id", "Note");
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Debts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PurchaseId,UploaderUserId,OwerUserId,Amount,ApprovalState,PaymentState")] Debt debt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(debt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwerUserId"] = new SelectList(_context.Users, "Id", "Id", debt.OwerUserId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchase, "Id", "Note", debt.PurchaseId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id", debt.UploaderUserId);
            return View(debt);
        }

        // GET: Debts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debt.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }
            ViewData["OwerUserId"] = new SelectList(_context.Users, "Id", "Id", debt.OwerUserId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchase, "Id", "Note", debt.PurchaseId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id", debt.UploaderUserId);
            return View(debt);
        }

        // POST: Debts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PurchaseId,UploaderUserId,OwerUserId,Amount,ApprovalState,PaymentState")] Debt debt)
        {
            if (id != debt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(debt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DebtExists(debt.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwerUserId"] = new SelectList(_context.Users, "Id", "Id", debt.OwerUserId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchase, "Id", "Note", debt.PurchaseId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id", debt.UploaderUserId);
            return View(debt);
        }

        // GET: Debts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debt
                .Include(d => d.OwerUser)
                .Include(d => d.Purchase)
                .Include(d => d.UploaderUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debt == null)
            {
                return NotFound();
            }

            return View(debt);
        }

        // POST: Debts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var debt = await _context.Debt.FindAsync(id);
            if (debt != null)
            {
                _context.Debt.Remove(debt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DebtExists(int id)
        {
            return _context.Debt.Any(e => e.Id == id);
        }
    }
}
