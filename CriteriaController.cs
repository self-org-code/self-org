using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SelfOrg.Data;
using SelfOrg.Models;
//----------------------------------Управление критериями. Много-много стандартных методов--------------------------------------
namespace SelfOrg.Controllers
{
    public class CriteriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CriteriaController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Criteria
        public async Task<IActionResult> Index()
        {
            return View(await _context.Criteria.ToListAsync());
        }

        // GET: Criteria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criterion = await _context.Criteria
                .SingleOrDefaultAsync(m => m.CriterionId == id);
            if (criterion == null)
            {
                return NotFound();
            }

            return View(criterion);
        }

        // GET: Criteria/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Criteria/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CriterionId,name,description")] Criterion criterion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(criterion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(criterion);
        }

        // GET: Criteria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criterion = await _context.Criteria.SingleOrDefaultAsync(m => m.CriterionId == id);
            if (criterion == null)
            {
                return NotFound();
            }
            return View(criterion);
        }

        // POST: Criteria/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CriterionId,name,description")] Criterion criterion)
        {
            if (id != criterion.CriterionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(criterion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CriterionExists(criterion.CriterionId))
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
            return View(criterion);
        }

        // GET: Criteria/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criterion = await _context.Criteria
                .SingleOrDefaultAsync(m => m.CriterionId == id);
            if (criterion == null)
            {
                return NotFound();
            }

            return View(criterion);
        }

        // POST: Criteria/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var criterion = await _context.Criteria.SingleOrDefaultAsync(m => m.CriterionId == id);
            _context.Criteria.Remove(criterion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CriterionExists(int id)
        {
            return _context.Criteria.Any(e => e.CriterionId == id);
        }
    }
}
