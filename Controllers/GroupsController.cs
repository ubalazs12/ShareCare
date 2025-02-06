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
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Groups
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var applicationDbContext = _context.Groups.Include(group => group.CreatorUser).Include(group => group.Users).Where(group => group.Users.Any(u => u.Id == user.Id));
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Groups/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(group => group.CreatorUser)
                .Include(group => group.Users)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            ViewData["Users"] = group.Users;

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            ViewData["InviteLink"] = $"{baseUrl}/Groups/JoinGroupWithLink?inviteCode={group.Id}";
            return View(@group);
        }

        // GET: Groups/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Group @group)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                @group.CreatorUserId = user.Id;
                @group.Users.Add(user);
                //ModelState.ClearValidationState(nameof(@group.CreatorUserId));
                //ModelState.MarkFieldValid(nameof(@group.CreatorUserId));
            }
            if (ModelState.IsValid)
            {
                _context.Add(@group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Groups/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            ViewData["CreatorUserId"] = new SelectList(_context.Users, "Id", "Id", @group.CreatorUserId);
            return View(@group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,CreatorUserId")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
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
            ViewData["CreatorUserId"] = new SelectList(_context.Users, "Id", "Id", @group.CreatorUserId);
            return View(@group);
        }

        // GET: Groups/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(group => group.CreatorUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var @group = await _context.Groups.FindAsync(id);
            if (@group != null)
            {
                _context.Groups.Remove(@group);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/JoinGroupWithLink/5
        [HttpGet]
        public async Task<IActionResult> JoinGroupWithLink([FromQuery] string inviteCode)
        {
            return await JoinGroupFunction(inviteCode);
        }

        // POST: Groups/JoinGroup/5
        [HttpPost]
        public async Task<IActionResult> JoinGroup(string inviteCode)
        {
            return await JoinGroupFunction(inviteCode);
        }

        private async Task<IActionResult> JoinGroupFunction(string inviteCode)
        {
            if (string.IsNullOrEmpty(inviteCode))
            {
                TempData["Message"] = "Please enter a valid invite code.";
                return RedirectToAction(nameof(Index));
            }
            var group = await _context.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == inviteCode);
            if (group == null)
            {
                TempData["Message"] = "Couldn't find a group to join.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (group.Users.Contains(user))
                {
                    TempData["Message"] = "You are already a member of this group.";
                    return RedirectToAction(nameof(Index));
                }
                group.Users.Add(user);

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Successfully joined the group: {group.Name}";
                return RedirectToAction(nameof(Index));
            }
            return Unauthorized();
        }

        private bool GroupExists(string id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}
