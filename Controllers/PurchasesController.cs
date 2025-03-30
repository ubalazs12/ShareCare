using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShareCare.Data;
using ShareCare.Models;

namespace ShareCare.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public PurchasesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Purchases
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Purchase.Include(p => p.Group).Include(p => p.UploaderUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Purchases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchase
                .Include(p => p.Group)
                .Include(p => p.UploaderUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // GET: Purchases/Create
        [HttpGet]
        public async Task<IActionResult> Create([FromQuery] string groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return View("Error");
            }
            ViewData["Group"] = group;
            ViewData["User"] = user;
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", groupId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "FullName", user.Id);
            return View();
        }

        // POST: Purchases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GroupId,UploaderUserId,TotalAmount,Note,ImageData,ImageMimeType")] Purchase purchase)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == purchase.GroupId);
            if (group == null)
            {
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                purchase.Debts = MakeDebts(group, user);
                _context.Add(purchase);
                await _context.SaveChangesAsync();
                return  RedirectToAction("Purchases", "Groups", new { id = purchase.GroupId });
            }
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Id", purchase.GroupId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id", purchase.UploaderUserId);
            return View(purchase);
        }

        // GET: Purchases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Id", purchase.GroupId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id", purchase.UploaderUserId);
            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GroupId,UploaderUserId,TotalAmount,Note,ImageData,ImageMimeType")] Purchase purchase)
        {
            if (id != purchase.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseExists(purchase.Id))
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
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Id", purchase.GroupId);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "Id", purchase.UploaderUserId);
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchase
                .Include(p => p.Group)
                .Include(p => p.UploaderUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase != null)
            {
                _context.Purchase.Remove(purchase);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Purchases", "Groups", new { id = purchase.GroupId });
        }

        // POST: Purchases/Approve/5
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var purchase = await _context.Purchase.Include(purchase => purchase.Debts).FirstOrDefaultAsync(purchase => purchase.Id == id);
            if (purchase != null)
            {
                var debt = purchase.Debts.FirstOrDefault(debt => debt.OwerUser == user);
                if (debt == null)
                {
                    return NotFound();
                }
                debt.ApprovalState = eApprovalState.eApproved;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("PurchasesToApprove", "Groups", new { id = purchase.GroupId });
        }

        // POST: Purchases/Approve/5
        [HttpPost, ActionName("Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            // TO DO
            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase != null)
            {
                return NotFound();
            }

            //await _context.SaveChangesAsync();
            return RedirectToAction("PurchasesToApprove", "Groups", new { id = purchase.GroupId });
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchase.Any(e => e.Id == id);
        }
        private void ValidateDebts()
        {
            // TODO
        }
        private ICollection<Debt> MakeDebts(Group group, ApplicationUser uploaderUser)
        {
            ICollection<Debt> debts = [];
            foreach (var member in group.Users)
            {
                double value = Double.Parse(Request.Form[member.Id]);
                Debt debt = new Debt();
                debt.Amount = value;
                debt.UploaderUserId = uploaderUser.Id;
                debt.OwerUserId = member.Id;
                debt.GroupId = group.Id;
                debt.ApprovalState = uploaderUser.Id == member.Id ? eApprovalState.eApproved : eApprovalState.eIdle;
                debt.PaymentState = ePaymentState.eInvalid;
                debts.Add(debt);
                _context.Add(debt);
            }

            return debts;
        }
    }
}
