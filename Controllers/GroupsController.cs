using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using ShareCare.Data;
using ShareCare.Logic;
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
                .Include(group => group.Purchases)
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

        // GET: Groups/Purchases/5
        [HttpGet]
        public async Task<IActionResult> Purchases(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(group => group.CreatorUser)
                .Include(group => group.Users)
                .Include(group => group.Purchases)
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

        // GET: Groups/Debts/5
        [HttpGet]
        public async Task<IActionResult> Debts(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(group => group.CreatorUser)
                .Include(group => group.Users)
                .Include(group => group.Purchases)
                    .ThenInclude(purchase => purchase.Debts)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            ViewData["TotalDebts"] = GetTotalDebtsNoSimplification(group)/*.OrderByDescending(totalDebt => totalDebt.Value)*/.ToList();
            ViewData["TotalDebtsSimpler"] = GetTotalDebtsNoBackAndForth(group)/*.OrderByDescending(totalDebt => totalDebt.Value)*/.ToList();
            ViewData["TotalDebtsSimplest"] = GetTotalDebtsTransitiveSimple(group)/*.OrderByDescending(totalDebt => totalDebt.Value)*/.ToList();
            ViewData["Users"] = group.Users;

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

        private Dictionary<(ApplicationUser, ApplicationUser), double> GetTotalDebtsNoBackAndForth(Group group)
        {
            Dictionary<(ApplicationUser, ApplicationUser), double> totalDebts = [];
            var userList = group.Users.ToList();
            foreach (var receiver in group.Users)
            {
                foreach (var ower in group.Users)
                {
                    if (receiver != ower)
                    {
                        var roTuple = (receiver, ower);
                        var orTuple = (ower, receiver);
                        double roTotal = group.Debts.Where(debt => (debt.UploaderUser == receiver && debt.OwerUser == ower)).Sum(debt => debt.Amount);
                        totalDebts[roTuple] = roTotal;
                        if (totalDebts.ContainsKey(orTuple))
                        {
                            double orTotal = totalDebts[orTuple];
                            totalDebts[roTuple] = Math.Max(0, roTotal - orTotal);
                            totalDebts[orTuple] = Math.Max(0, orTotal - roTotal);
                        }
                    }
                }
            }
            foreach(var debt in totalDebts)
            {
                if (debt.Value <= 0)
                {
                    totalDebts.Remove(debt.Key);
                }
            }
            return totalDebts;
        }

            private Dictionary<(ApplicationUser, ApplicationUser), double> GetTotalDebtsTransitiveSimple(Group group)
        {
            Dictionary<(ApplicationUser, ApplicationUser), double> totalDebts = GetTotalDebtsNoBackAndForth(group);
            var userList = group.Users.ToList();

            string[] usernames = group.Users.Select(user => user.FullName).ToArray();
            Dinics solver = new Dinics(group.Users.Count, usernames);
            List<Edge> edges = new List<Edge>();
            List<Edge> visitedEdges = new List<Edge>();
            foreach (var totalDebt in totalDebts)
            {
                if (totalDebt.Value > 0)
                {
                    var from = userList.IndexOf(totalDebt.Key.Item2);
                    var to = userList.IndexOf(totalDebt.Key.Item1);
                    edges.Add(new Edge(from, to, totalDebt.Value));
                }
            }
            solver.AddEdges(edges);

            foreach (var edge in edges)
            {
                solver.Recompute();
                solver.Source = edge.From;
                solver.Sink = edge.To;
                List<Edge>[] residualGraph = solver.GetSolvedGraph();
                List<Edge> newEdges = new List<Edge>();

                foreach (List<Edge> allEdges in residualGraph)
                {
                    foreach (Edge e in allEdges)
                    {
                        double remainingFlow = ((e.Flow < 0) ? e.Capacity : (e.Capacity - e.Flow));
                        if (remainingFlow > 0)
                        {
                            newEdges.Add(new Edge(e.From, e.To, remainingFlow));
                        }
                    }
                }

                double maxFlow = solver.MaxFlow;

                int source = solver.Source;
                int sink = solver.Sink;

                solver = new Dinics(group.Users.Count, usernames);

                solver.AddEdges(newEdges);
                if (maxFlow > 0)
                {
                    solver.AddEdge(source, sink, maxFlow);
                }
            }

            totalDebts = [];

            foreach (var resEdge in solver.Edges)
            {
                var ower = userList[resEdge.From];
                var receiver = userList[resEdge.To];
                totalDebts[(receiver, ower)] = resEdge.Capacity;
            }


            return totalDebts;
        }

        private Dictionary<(ApplicationUser, ApplicationUser), double> GetTotalDebtsNoSimplification(Group group)
        {
            Dictionary<(ApplicationUser, ApplicationUser), double> totalDebts = [];

            foreach (var receiver in group.Users)
            {
                foreach (var ower in group.Users)
                {
                    if (receiver != ower)
                    {
                        var roTuple = (receiver, ower);
                        var orTuple = (ower, receiver);
                        totalDebts[roTuple] = group.Debts.Where(debt => (debt.UploaderUser == receiver && debt.OwerUser == ower)).Sum(debt => debt.Amount);
                    }
                }
            }

            return totalDebts;
        }

        private bool GroupExists(string id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}
