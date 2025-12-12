using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShareCare.Data;
using ShareCare.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ShareCare.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        const double epsilon = 00000.1;

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
            ViewData["Debts"] = new Dictionary<string, string>();
            return View();
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GroupId,UploaderUserId,TotalAmount,Note")] Purchase purchase, IFormFile imageFile)
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
            ViewData["Group"] = group;
            ViewData["User"] = user;
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", group.Id);
            ViewData["UploaderUserId"] = new SelectList(_context.Users, "Id", "FullName", user.Id);
            ModelState.Remove("imageFile");
            if (ModelState.IsValid && ValidateDebts(group, purchase.TotalAmount))
            {
                purchase.Debts = MakeDebts(group, user);

                if (imageFile != null && imageFile.Length > 0)
                {
                    if ((purchase.ImageData = GetCompressedImage(imageFile)) != null)
                    {
                        purchase.ImageMimeType = "image/jpeg";
                    }
                    else
                    {
                        return View(purchase);
                    }
                }

                _context.Add(purchase);
                await _context.SaveChangesAsync();
                return RedirectToAction("Purchases", "Groups", new { id = purchase.GroupId });

            }
            ViewData["Debts"] = new Dictionary<string, string>();
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

        //// POST: Purchases/Approve/5
        //[HttpPost, ActionName("Approve")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Approve(int id)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }
        //    var purchase = await _context.Purchase.Include(purchase => purchase.Debts).FirstOrDefaultAsync(purchase => purchase.Id == id);
        //    if (purchase != null)
        //    {
        //        var debt = purchase.Debts.FirstOrDefault(debt => debt.OwerUser == user);
        //        if (debt == null)
        //        {
        //            return NotFound();
        //        }
        //        debt.ApprovalState = eApprovalState.eApproved;
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("PurchasesToApprove", "Groups", new { id = purchase.GroupId });
        //}

        //// POST: Purchases/Reject/5
        //[HttpPost, ActionName("Reject")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Reject(int id)
        //{
        //    var purchase = await _context.Purchase.FindAsync(id);
        //    if (purchase != null)
        //    {
        //        return NotFound();
        //    }

        //    //await _context.SaveChangesAsync();
        //    return RedirectToAction("PurchasesToApprove", "Groups", new { id = purchase.GroupId });
        //}

        // POST: Purchases/PayDebt/5
        [HttpPost, ActionName("PayDebt")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDebt(string groupId, string owerId, string receiverId, double value)
        {
            Purchase payment = new Purchase();
            payment.GroupId = groupId;
            payment.Note = "Kiegyenlítés";
            payment.UploaderUserId = owerId;
            payment.TotalAmount = value;
            payment.IsPayment = true;

            Debt debt = new Debt();
            debt.Amount = value;
            debt.UploaderUserId = owerId;
            debt.OwerUserId = receiverId;
            debt.GroupId = groupId;
            debt.ApprovalState = eApprovalState.eIdle;
            debt.PaymentState = ePaymentState.eInvalid;
            _context.Add(debt);

            payment.Debts.Add(debt);

            _context.Add(payment);

            await _context.SaveChangesAsync();

            return RedirectToAction("Debts", "Groups", new { id = groupId });
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchase.Any(e => e.Id == id);
        }
        private bool ValidateDebts(Group group, double totalAmount)
        {
            bool allCorrect = true;
            double sum = 0;
            foreach (var member in group.Users)
            {
                double value;

                if (!ValidateDebtInput(Request.Form[member.Id], out value))
                {
                    ViewData[$"DebtError{member.Id}"] = "Csak nem negatív szám adható meg!";
                    allCorrect = false;
                    continue;
                }

                sum += value;
            }

            if (Math.Abs(sum - totalAmount) > epsilon)
            {
                ViewData["NotMatchingSumError"] = "A vásárlás összegének meg kell egyeznie az elosztások összegével!";
                allCorrect = false;
            }

            return allCorrect;
        }

        private bool ValidateDebtInput(string input, out double value)
        {
            if (!double.TryParse(input, out value))
            {
                return false;
            }

            if (value < 0)
            {
                return false;
            }

            return true;
        }
        private ICollection<Debt> MakeDebts(Group group, ApplicationUser uploaderUser)
        {
            ICollection<Debt> debts = [];
            var persistentDebts = new Dictionary<string, string>();
            foreach (var member in group.Users)
            {
                double value = Double.Parse(Request.Form[member.Id]);
                persistentDebts[member.Id] = value.ToString();
                if (Math.Abs(value) > epsilon)
                {
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
            }
            ViewData["Debts"] = persistentDebts;
            return debts;
        }

        public async Task<IActionResult> GetImage(int id)
        {
            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase?.ImageData != null)
            {
                return File(purchase.ImageData, purchase.ImageMimeType);
            }
            else
            {
                return NotFound();
            }
        }

        private byte[] GetCompressedImage(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // 1. Validate file size (e.g., max 500MB)
                const long maxFileSize = 500 * 1024 * 1024; // 2MB in bytes
                if (imageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("imageFile", "A kép nem lehet nagyobb 500MB-nál.");
                    return null;
                }

                // 2. Validate file extension/type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("imageFile", "Csak .jpg, .jpeg, .png, és .gif kiterjesztésű fájl tölthető fel.");
                    return null;
                }

                using var inputStream = imageFile.OpenReadStream();
                using var image = Image.Load(inputStream);

                // Optional: Resize (e.g. max width/height of 1024px)
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1024, 1024) // Adjust as needed
                }));

                using var outputStream = new MemoryStream();

                // Save as JPEG with quality compression (e.g. 75%)
                image.SaveAsJpeg(outputStream, new JpegEncoder
                {
                    Quality = 75
                });

                return outputStream.ToArray();
            }
            return null;
        }
    }
}
