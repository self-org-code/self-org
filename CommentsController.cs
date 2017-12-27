using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SelfOrg.Data;
using SelfOrg.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SelfOrg.Components;
//�����������
namespace SelfOrg.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Comments
        public async Task<IActionResult> Index() //�����������
        {
            var applicationDbContext = _context.Comments.Include(c => c.Post).Include(c => c.User);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> viewpost(int id) //�������� �����
        {
            CommentViewModel model = new CommentViewModel(); //�������� ������
            var post = await _context.Posts.Include(p => p.User).Include(p => p.Category).SingleOrDefaultAsync(p => p.PostID == id); //����� ����� �� id
            model.post = post;
            model.comments = _context.Comments.Where(p => p.PostId == id).Include(p => p.User); //��������, �� �����
            model.commmodel = new CommentsModel(); //�������� ������ ������������
            model.commmodel.comments = _context.Comments.Where(p => p.PostId == id).Include(p => p.User); //����� ������������ � ����� �����
            model.crits = _context.CatCrits.Where(p => p.CategoryId == post.CategoryId).Include(p => p.Category).Include(p => p.Criterion); //����� ���������
            var ratings = _context.Ratings.Where(p => p.PostId == id).Include(p => p.User);//����� � �� ���������� ����������� ����������� ��������� ������� �����
            float sum = 0;
            foreach (Rating item in ratings)
            {
                sum += item.rating*item.User.Weight;
            }
            model.post.rating = sum;
            //--------------------------�������� �� ����������� ������---------------------------------

            bool islogged = (User.Identity.IsAuthenticated); //��������, ���� �� ����

            model.commmodel.islogged = islogged; //��� �� ����� � � ������ ������������
            if (islogged) //���� ������������ �����
            {
                ClaimsPrincipal currentUser = this.User;
                string userid = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value; //�������� ��� id
                model.rateable = true; //�� ��������� ������������ ����� ����� ��������� ������
                if (post.UserId == userid) //���� id ������ ����� ��������� � id ������������, �� ������������, ��� ����� �����, �� ����� ��� ���������
                {
                    model.rateable = false;
                }
                else //���� ������������ - �� ����� �����
                {
                    bool check = _context.Ratings.Any(p => (p.PostId == post.PostID) && (p.UserId == userid)); //���������, ������ �� �� ��� ����� ����� ������
                    if (check == true) //���� ������ - ������ ��������� �� �����
                    {
                        model.rateable = false;
                    }
                    else model.rateable = true;
                }
                model.userrating = 0; 
                if (model.rateable == false) //���� ������������ ��� �������� ������, ������� ������ �� ������
                {
                    foreach (Rating your in ratings)
                    {
                        if (your.UserId == userid)
                            model.userrating += your.rating * your.User.Weight;
                    }
                }
                //---------------------�������� �� ����������� ��������������---------------------------
                if (userid == post.UserId) //�������, ����� ��� ���
                {
                    model.editable = true; //����� ����� ������������� ����
                }
                else
                {
                    model.editable = false;
                }

                //--------------------------��������� ������������-----------------------------
                //���� ����� ������ ��������, ����� ����������� ������������ ������ ��� ���������. 
                //�� -�� ������� � �������� �� ������ �� ������������, ���� ���������� ��� ���������
                int ratecount = 0;
                model.commmodel.commrates = new int[model.commmodel.comments.Count()];
                //var theserates = _context.CommRates.Where(p => p.UserId == userid).Include(p => p.Comment)
                foreach (Comment selected in model.commmodel.comments) //�������� �� ������������ � �����
                {
                    //var comrate = _context.CommRates.SingleOrDefault(p => ((p.CommentId == selected.CommentId) && (p.UserId == userid))); //��� ������� ���� ������, ������ ���� �������������
                    //if (comrate != null)
                    //{
                    //    model.commmodel.commrates[ratecount] = comrate.value; //���� ���� - ����������
                    //}
                    //else
                    //{
                    //    model.commmodel.commrates[ratecount] = 0;
                    //}
                    model.commmodel.commrates[ratecount] = 0;
                    ratecount++;
                }
            }

            else //���� ����� ���, ������������� �������
            {
                model.editable = false;
            }
            
            model.islogged = islogged;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> comment([FromBody] ReplyViewModel inmodel) //��������������� �����
        {
           
            Comment newcom = new Comment(); //������ ����� �����������
            ClaimsPrincipal currentUser = this.User;
            string userid = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value; 
            newcom.UserId = userid; //������� ����� �������� ������������
            int properid = Convert.ToInt32(inmodel.id);
            newcom.PostId = properid; //���������� id �����, ������� ������������
            newcom.Text = inmodel.comment; //���������� ����� ��������
            newcom.CommentDate = DateTime.Now; //���� ����� �������
            _context.Comments.Add(newcom); //��������� ������ � ��
            await _context.SaveChangesAsync();
            CommentsModel model = new CommentsModel(); //������ ������ ��� ���������� ����� ������������ �� ��������
            model.islogged = true; //��� ����������� ������, �� ������������ ������� ����� � �������
            model.comments = _context.Comments.Where(p => p.PostId == properid).Include(p => p.User); //����������� ��� �����������
            //---------------------------------------------����� � ����� ������ ������ ����������� ��������. � �� ����, ����� �� ��� ���-�� ����������, �� ����������� � ���� ����� �� �����������. �� ����� ����� � ������� �������----------

            //--------------------------��������� ������������-----------------------------

            int ratecount = 0;
            model.commrates = new int[model.comments.Count()];
            foreach (Comment selected in model.comments) //�������� �� ������������ � �����
            {
                //var comrate = _context.CommRates.SingleOrDefault(p => ((p.CommentId == selected.CommentId) && (p.UserId == userid))); //��� ������� ���� ������, ������ ���� �������������
                //if (comrate != null)
                //{
                //    model.commmodel.commrates[ratecount] = comrate.value; //���� ���� - ����������
                //}
                //else
                //{
                //    model.commmodel.commrates[ratecount] = 0;
                //}
                model.commrates[ratecount] = 0;
                ratecount++;
            }
            return PartialView("postcomments", model);
        }
        [HttpPost]
        public async Task<IActionResult> editpost([FromBody] PostUpdateModel upmodel) //������� ����� ��������� ��������. ���� ����� ������ �� ����� ������?
        {
            int postid = Convert.ToInt32(upmodel.postid); //���� id ����������� �����
            Post changedpost = _context.Posts.Single(p => p.PostID == postid); //������� ��� � ��
            changedpost.content = upmodel.postbody; //����� ��� ����� �����
            changedpost.LastModified = DateTime.Now; //���� ���������� ��������� - �������
            _context.Update(changedpost); //���������
            _context.SaveChanges();
            CommentViewModel model = new CommentViewModel();
            var post = await _context.Posts.Include(p => p.User).Include(p => p.Category).SingleOrDefaultAsync(p => p.PostID == postid);
            model.post = post;           
            model.crits = _context.CatCrits.Where(p => p.CategoryId == post.CategoryId).Include(p => p.Category).Include(p => p.Criterion);
            var ratings = _context.Ratings.Where(p => p.PostId == postid).Include(p => p.User);
            float sum = 0;
            foreach (Rating item in ratings)
            {
                sum += item.rating * item.User.Weight;
            }
            model.post.rating = sum;
           //--------------------------�������� �� ����������� ������---------------------------------

            bool islogged = (User.Identity.IsAuthenticated);
            if (islogged)
            {
                ClaimsPrincipal currentUser = this.User;
                string userid = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                model.rateable = true;
                if (post.UserId == userid)
                {
                    model.rateable = false;
                }
                else
                {
                    bool check = _context.Ratings.Any(p => (p.PostId == post.PostID) && (p.UserId == userid));
                    if (check == true)
                    {
                        model.rateable = false;
                    }
                    else model.rateable = true;
                }
                model.userrating = 0;
                if (model.rateable == false)
                {
                    foreach (Rating your in ratings)
                    {
                        if (your.UserId == userid)
                            model.userrating += your.rating * your.User.Weight;
                    }
                }
                //---------------------�������� �� ����������� ��������������---------------------------
                if (userid == post.UserId)
                {
                    model.editable = true;
                }
                else
                {
                    model.editable = false;
                }
               
            }

            else
            {
                model.editable = false;
            }
            model.islogged = islogged;
            return PartialView("posthead", model);

        }
        [HttpPost]
        public async Task<IActionResult> reply([FromBody] ReplyViewModel inmodel) //����� �� �����������
        {
            //���������� ������ comment, �� ����� � ���������� ����� �����������, �� ����� ����������� ��������� �����
            Comment newcom = new Comment(); 
            ClaimsPrincipal currentUser = this.User;
            string userid = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            newcom.UserId = userid;
            int neededid = Convert.ToInt32(inmodel.id);
            Comment com = await _context.Comments.Where(p => p.CommentId == neededid).SingleOrDefaultAsync();
            newcom.PostId = com.PostId;
            newcom.Text = inmodel.comment;
            newcom.CommentDate = DateTime.Now;
            newcom.ReplyTo = com.CommentId; //id ������������� �����������
            _context.Comments.Add(newcom);
            await _context.SaveChangesAsync();
            CommentsModel model = new CommentsModel();
            model.islogged = true;
            model.comments = _context.Comments.Where(p => p.PostId == com.PostId).Include(p => p.User);
            //--------------------------��������� ������������-----------------------------

            int ratecount = 0;
            model.commrates = new int[model.comments.Count()];
            foreach (Comment selected in model.comments) //�������� �� ������������ � �����
            {
                //var comrate = _context.CommRates.SingleOrDefault(p => ((p.CommentId == selected.CommentId) && (p.UserId == userid))); //��� ������� ���� ������, ������ ���� �������������
                //if (comrate != null)
                //{
                //    model.commmodel.commrates[ratecount] = comrate.value; //���� ���� - ����������
                //}
                //else
                //{
                //    model.commmodel.commrates[ratecount] = 0;
                //}
                model.commrates[ratecount] = 0;
                ratecount++;
            }
            return PartialView("postcomments", model);

        }
        [HttpPost]
        public async Task<IActionResult> ratecom([FromBody] CommentRateModel ratemodel) //������ �����������
        {
            ClaimsPrincipal currentUser = this.User;//���� id ������������
            int commid = Convert.ToInt32(ratemodel.commentid); //� �����������
            int value;
            //���� ����, �� ������� ����� ���� �� +1, ���� ������� - �� -1
            if (ratemodel.action == "up") 
            {
                value = 1;
            }
            else
            {
                value = -1;
            }
            string uid = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            CommRate existing = _context.CommRates.SingleOrDefault(p => ((p.CommentId == commid) && (p.UserId == uid))); //���������� �� ����������� �����
            Comment rated = _context.Comments.Single(p => p.CommentId == commid);
            if (existing == null) //���� ������ ��� �� ����
            {
                CommRate added = new CommRate();
                added.CommentId = commid;
                added.UserId = uid;
                added.value = value;
                _context.CommRates.Add(added);
                await _context.SaveChangesAsync();
                rated.rating += value;
                _context.Update(rated);
                await _context.SaveChangesAsync();
            }
            else //���� ������ ��� ����
            {
                //rated.rating -= value;
                //_context.Update(rated);
                //await _context.SaveChangesAsync();
                if (existing.value == value) //���� ������ ���������
                {
                    rated.rating -= value;
                    _context.Update(rated); //�������� �������
                    await _context.SaveChangesAsync();
                    _context.CommRates.Remove(existing); //������ ������ �� ����
                    await _context.SaveChangesAsync();
                }
                else //�� ��������� - ������ ���� �� ������� ��� ��������
                {
                    existing.value = value; //������ ���������� ����� ��������
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    rated.rating += value * 2;
                    _context.Update(rated);
                    await _context.SaveChangesAsync();
                }
            }
            CommentsModel model = new CommentsModel();
            model.islogged = true;
            model.comments = _context.Comments.Where(p => p.PostId == rated.PostId).Include(p => p.User);
            //--------------------------��������� ������������-----------------------------

            int ratecount = 0;
            model.commrates = new int[model.comments.Count()];
            foreach (Comment selected in model.comments) //�������� �� ������������ � �����
            {
                //var comrate = _context.CommRates.SingleOrDefault(p => ((p.CommentId == selected.CommentId) && (p.UserId == userid))); //��� ������� ���� ������, ������ ���� �������������
                //if (comrate != null)
                //{
                //    model.commmodel.commrates[ratecount] = comrate.value; //���� ���� - ����������
                //}
                //else
                //{
                //    model.commmodel.commrates[ratecount] = 0;
                //}
                model.commrates[ratecount] = 0;
                ratecount++;
            }
            return PartialView("postcomments", model);

        }

        [HttpPost]
        public async Task<IActionResult> rate ([FromBody] RatingViewModel[] ratings)  //������ �����
        {
            ClaimsPrincipal currentUser = this.User;
           string userid = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            double amount = 0;
            foreach (RatingViewModel item in ratings) //�������, ������� ����� ���������. ��� ����� ��� �������� ����
            {
                amount += Math.Pow(2, Convert.ToInt32(item.weight));
            }
            double alpha = 1 / amount; //������� ���� ���������
            float result = 0;
            //result += Convert.ToSingle(alpha);
            foreach (RatingViewModel item in ratings)
            {
                result += Convert.ToSingle(Convert.ToInt32(item.rating) * Math.Pow(2, Convert.ToInt32(item.weight)) * alpha); //���������� �������� ��� �������� �� ��� ������ ������������
                Rating newrate = new Rating(); //������ ������ ������
                newrate.CriterionId = Convert.ToInt16(item.criterion); //����� ��������
                newrate.PostId = Convert.ToInt16(item.post); //id �����
                newrate.UserId = userid; //������������
                newrate.rating = Convert.ToSingle(Math.Round(Convert.ToInt32(item.rating) * Math.Pow(2, Convert.ToInt32(item.weight)) * alpha, 3)); //������ ��������� �� 3 ������ ����� �������
                _context.Add(newrate);
                await _context.SaveChangesAsync();
            }
            Math.Round(result, 3);
            int theid = Convert.ToInt32(ratings[0].post);
            Post ratedpost = await _context.Posts.Include(p => p.User).Include(p => p.Category).SingleOrDefaultAsync(p => p.PostID == theid); //����� ������� ������� ���������� �� ��� ������������ � ��������� � �������� �����
            //� �� ������, ���� �� ���, �� ������ ���� �����
            ratedpost.rating += result;
            _context.Update(ratedpost);
            await _context.SaveChangesAsync();
            //---- 
            CommentViewModel model = new CommentViewModel();
            model.post = ratedpost;
            model.crits = _context.CatCrits.Where(p => p.CategoryId == ratedpost.CategoryId).Include(p => p.Category).Include(p => p.Criterion);
            var rates = _context.Ratings.Where(p => p.PostId == theid).Include(p => p.User);
            float sum = 0;
            foreach (Rating item in rates)
            {
                sum += item.rating * item.User.Weight;
            }
            model.post.rating = sum;
            //--------------------------�������� �� ����������� ������---------------------------------
            model.rateable = true;
            if (ratedpost.UserId == userid)
            {
                model.rateable = false;
            }
            else
            {
                bool check = _context.Ratings.Any(p => (p.PostId == ratedpost.PostID) && (p.UserId == userid));
                if (check == true)
                {
                    model.rateable = false;
                }
                else model.rateable = true;
            }
            model.userrating = 0;
            if (model.rateable == false)
            {
                foreach (Rating your in rates)
                {
                    if (your.UserId == userid)
                        model.userrating += your.rating * your.User.Weight;
                }
            }
            //---------------------�������� �� ����������� ��������������---------------------------
            if (userid == ratedpost.UserId)
            {
                model.editable = true;
            }
            else
            {
                model.editable = false;
            }
            return PartialView("posthead", model);
        }
        //-------------------------------------------------------����������� ������---------------------------------------------------
        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Post)
                .Include(c => c.User)
                .SingleOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostID", "PostDescr");
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,UserId,Text,CommentDate,PostId,LastModified,ReplyTo")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostID", "PostDescr", comment.PostId);
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", comment.UserId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.SingleOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostID", "PostDescr", comment.PostId);
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", comment.UserId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,UserId,Text,CommentDate,PostId,LastModified,ReplyTo")] Comment comment)
        {
            if (id != comment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.CommentId))
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostID", "PostDescr", comment.PostId);
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", comment.UserId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Post)
                .Include(c => c.User)
                .SingleOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.SingleOrDefaultAsync(m => m.CommentId == id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
