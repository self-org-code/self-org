using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SelfOrg.Data;
using SelfOrg.Models;
//Важность критериев для категорий
namespace SelfOrg.Controllers
{
    public class CatCritsController : Controller //--------------------------------Используется только для отладки-------------------------------------------------------
    {
        private readonly ApplicationDbContext _context;

        public CatCritsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: CatCrits
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CatCrits.Include(c => c.Category).Include(c => c.Criterion);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CatCrits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catCrit = await _context.CatCrits
                .Include(c => c.Category)
                .Include(c => c.Criterion)
                .SingleOrDefaultAsync(m => m.CatCritId == id);
            if (catCrit == null)
            {
                return NotFound();
            }

            return View(catCrit);
        }

        // GET: CatCrits/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatDescr");
            ViewData["CriterionId"] = new SelectList(_context.Criteria, "CriterionId", "description");
            return View();
        }

        // POST: CatCrits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CatCritId,CategoryId,CriterionId,prio")] CatCrit catCrit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(catCrit);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatDescr", catCrit.CategoryId);
            ViewData["CriterionId"] = new SelectList(_context.Criteria, "CriterionId", "description", catCrit.CriterionId);
            return View(catCrit);
        }

        // GET: CatCrits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catCrit = await _context.CatCrits.SingleOrDefaultAsync(m => m.CatCritId == id);
            if (catCrit == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatDescr", catCrit.CategoryId);
            ViewData["CriterionId"] = new SelectList(_context.Criteria, "CriterionId", "description", catCrit.CriterionId);
            return View(catCrit);
        }

        // POST: CatCrits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CatCritId,CategoryId,CriterionId,prio")] CatCrit catCrit)
        {
            if (id != catCrit.CatCritId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(catCrit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatCritExists(catCrit.CatCritId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatDescr", catCrit.CategoryId);
            ViewData["CriterionId"] = new SelectList(_context.Criteria, "CriterionId", "description", catCrit.CriterionId);
            return View(catCrit);
        }

        // GET: CatCrits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catCrit = await _context.CatCrits
                .Include(c => c.Category)
                .Include(c => c.Criterion)
                .SingleOrDefaultAsync(m => m.CatCritId == id);
            if (catCrit == null)
            {
                return NotFound();
            }

            return View(catCrit);
        }

        // POST: CatCrits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var catCrit = await _context.CatCrits.SingleOrDefaultAsync(m => m.CatCritId == id);
            _context.CatCrits.Remove(catCrit);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CatCritExists(int id)
        {
            return _context.CatCrits.Any(e => e.CatCritId == id);
        }
    }
}
