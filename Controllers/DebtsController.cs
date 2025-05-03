using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShareCare.Data;
using ShareCare.Models;

namespace ShareCare.Controllers
{
    [Authorize]
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
            return View(debt);
        }

        // POST: Debts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount")] Debt debt)
        {
            if (id != debt.Id)
            {
                return NotFound();
            }
            var debtToUpdate = await _context.Debt.Include(d => d.Purchase).ThenInclude(p => p.Debts).FirstOrDefaultAsync(d => d.Id == id);
            var purchase = debtToUpdate.Purchase;
            if (debtToUpdate == null || purchase == null)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var uploaderDebt = purchase.Debts.FirstOrDefault(d => d.OwerUserId == debtToUpdate.UploaderUserId);
                    if (uploaderDebt == null)
                    {
                        return NotFound();
                    }
                    debtToUpdate.Amount = debt.Amount;
                    debtToUpdate.ApprovalState = eApprovalState.eIdle;

                    uploaderDebt.Amount = purchase.TotalAmount - purchase.Debts.Where(d => d.UploaderUserId != d.OwerUserId).Sum(d => d.Amount);

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
                return RedirectToAction("RejectedPurchases", "Groups", new { id = debtToUpdate.GroupId });
            }
            return View(debtToUpdate);
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
        // POST: Debts/Approve/5
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var debt = await _context.Debt.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }
            debt.ApprovalState = eApprovalState.eApproved;

            await _context.SaveChangesAsync();
            return RedirectToAction("PurchasesToApprove", "Groups", new { id = debt.GroupId });
        }

        // POST: Debts/ApprovePayment/5
        [HttpPost, ActionName("ApprovePayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            var debt = await _context.Debt.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }
            debt.ApprovalState = eApprovalState.eApproved;

            await _context.SaveChangesAsync();
            var referer = Request.Headers["Referer"].ToString();
            return Redirect(referer.ToString());
            return RedirectToAction("PurchasesToApprove", "Groups", new { id = debt.GroupId });
        }

        // POST: Debts/Reject/5
        [HttpPost, ActionName("Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            // TO DO
            var debt = await _context.Debt.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }

            debt.ApprovalState = eApprovalState.eRejected;

            await _context.SaveChangesAsync();
            return RedirectToAction("PurchasesToApprove", "Groups", new { id = debt.GroupId });
        }

        private bool DebtExists(int id)
        {
            return _context.Debt.Any(e => e.Id == id);
        }
    }
}
