using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SelfOrg.Data;
using SelfOrg.Models;
//Множители оценки, соответствующие текущему рейтингу пользователя
//-------------------------------------------------------Стандартные методы---------------------------------------------------
namespace SelfOrg.Controllers
{
    public class MultipliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MultipliersController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Multipliers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Multipliers.ToListAsync());
        }

        // GET: Multipliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var multiplier = await _context.Multipliers
                .SingleOrDefaultAsync(m => m.MultiplierId == id);
            if (multiplier == null)
            {
                return NotFound();
            }

            return View(multiplier);
        }

        // GET: Multipliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Multipliers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MultiplierId,Lower,Higher,Mul")] Multiplier multiplier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(multiplier);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(multiplier);
        }

        // GET: Multipliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var multiplier = await _context.Multipliers.SingleOrDefaultAsync(m => m.MultiplierId == id);
            if (multiplier == null)
            {
                return NotFound();
            }
            return View(multiplier);
        }

        // POST: Multipliers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MultiplierId,Lower,Higher,Mul")] Multiplier multiplier)
        {
            if (id != multiplier.MultiplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(multiplier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MultiplierExists(multiplier.MultiplierId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(multiplier);
        }

        // GET: Multipliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var multiplier = await _context.Multipliers
                .SingleOrDefaultAsync(m => m.MultiplierId == id);
            if (multiplier == null)
            {
                return NotFound();
            }

            return View(multiplier);
        }

        // POST: Multipliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var multiplier = await _context.Multipliers.SingleOrDefaultAsync(m => m.MultiplierId == id);
            _context.Multipliers.Remove(multiplier);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MultiplierExists(int id)
        {
            return _context.Multipliers.Any(e => e.MultiplierId == id);
        }
    }
}
