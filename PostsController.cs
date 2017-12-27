using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SelfOrg.Data;
using SelfOrg.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
//Посты
namespace SelfOrg.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        //SignInManager<User> SignInManager;
        //UserManager<User> UserManager;
        public static string Truncate(string value, int maxLength) //метод укорачивания поста для предпросмотра
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); //если меньше максимальной длины, то ставим полностью, иначе - обрезаем
        }

        public static string TruncBody (string input) //создание предпросмотра
        {
            string result = null;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(input);
            //Regex.Replace(input, "<.*?>", string.Empty);
            string text = doc.DocumentNode.InnerText; //выкидываем HTML-разметку во избежание ошибок
            result = Truncate(text, 200); //режем до 200 символов
            return result;
        }



        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Posts
        public async Task<IActionResult> Index() //стандартный метод
        {
            var applicationDbContext = _context.Posts.Include(p => p.Category).Include(p => p.User);
            //var applicationDbContext = _context.PostTags.Include(p => p.Post).Include(p => p.Post.Category).Include(p => p.Post.User).Include(p => p.Post.PostTags).Include(p => p.Tag);
            return View(await applicationDbContext.ToListAsync());
        }

       
        public async Task<IActionResult> category(int id) //выбираем посты, принадлежащие к конкретной категории
        {
            var applicationDbContext = _context.Posts.Include(p => p.Category).Include(p => p.User).Where(p => p.CategoryId == id).OrderByDescending(p => p.PostDate);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> tags(int id) //выбираем посты, содержащие конкретный тег
        {
            var applicationDbContext = _context.PostTags.Include(p => p.Post).Include(p => p.Post.Category).Include(p => p.Post.User).Include(p => p.Tag).Where(p => p.TagId == id).OrderByDescending(p => p.Post.PostDate);
            return View(await applicationDbContext.ToListAsync());
        }
        //public async Task<IActionResult> gettags(int id)
        //{
        //    var applicationDbContext = _context.PostTags.Include(p => p.Post).Include(p => p.Post.Category).Include(p => p.Post.User).Include(p => p.Tag).Where(p => p.TagId == id);
        //    return PartialView(await applicationDbContext.ToListAsync());
        //}
        //-------------------------------------------------------Стандартный метод---------------------------------------------------
        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .SingleOrDefaultAsync(m => m.PostID == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatName");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostViewModel model) //создание поста
        {
            
            if (ModelState.IsValid)
            {
                Post post = new Post(); //создаём модель
                post.PostName = model.Name;
                post.CategoryId = model.Cat;
                post.content = model.Text; //кладём данные из входной модели
                post.PostDescr = TruncBody(post.content); //обрезаем текст лоя создания првеью
               
                ClaimsPrincipal currentUser = this.User;
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                post.UserId = currentUserID; //автором ставим текущего пользователя
                post.PostDate = DateTime.Now; //дата тоже текущая
                string postslug = Slugifier.Transliterate(post.PostName); //создаётся slug-ссылка
                postslug = Slugifier.Slugify(postslug);
                post.PostSlug = postslug;
                String[] rawtags = model.Tags.Split(','); //входная строка тегов разбивается запятыми на отдельные теги
                _context.Add(post); //пост добавлне, работаем с тегами
                await _context.SaveChangesAsync();                
                var addedpost = await _context.Posts.SingleOrDefaultAsync(m => m.PostSlug == postslug); //находим id нового поста
                addedpost.PostSlug += "-" +addedpost.PostID.ToString(); //уникальность слагов
                _context.Update(addedpost);
                int redir = addedpost.PostID;
                foreach (string separtag in rawtags) //добавление, если необходимо, тегов
                {
                    string tagname;
                    if (separtag.Substring(0, 1) == " ") //Обрезает первый пробел
                    {
                        tagname = separtag.Substring(1, separtag.Length-1);
                    }
                    else tagname = separtag; //сохраняем имя тега
                   string tagtoadd = Slugifier.Transliterate(tagname);
                    tagtoadd = Slugifier.Slugify(tagtoadd); //подготовка слага
                    var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagSlug == tagtoadd);
                    int tagid;
                    if (tag == null) //добавление тега
                    {
                        Tag added = new Tag();
                        added.TagName = tagname;
                        added.TagSlug = tagtoadd;
                        _context.Tags.Add(added);
                        await _context.SaveChangesAsync();
                        var addedtag = await _context.Tags.SingleOrDefaultAsync(m => m.TagSlug == tagtoadd);
                        tagid = addedtag.TagId;
                    }
                    else
                    {
                        tagid = tag.TagId;
                    }
                    PostTag link = new PostTag(); //добавление связи "тег-пост"
                    link.PostId = addedpost.PostID;
                    link.TagId = tagid;
                    _context.PostTags.Add(link);
                    await _context.SaveChangesAsync();
                    //addedpost.PostTags.Add(link);
                    //_context.Posts.Update(addedpost);
                    //await _context.SaveChangesAsync();


                }
                var Cat = _context.Categories.SingleOrDefault(p => p.CategoryId == model.Cat);
                Cat.postcount++; //увеличиваем количество постов в категории, к которой принадлежит данный пост
                _context.Update(Cat);
                await _context.SaveChangesAsync();
                return Redirect("/Comments/viewpost/"+redir);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatName", model.Cat);
           
            return View(model);
        }
        //-------------------------------------------------------Стандартные методы---------------------------------------------------
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.SingleOrDefaultAsync(m => m.PostID == id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatName", post.CategoryId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostID,PostName,content,CategoryId")] Post post)
        {
            if (id != post.PostID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostID))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CatDescr", post.CategoryId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .SingleOrDefaultAsync(m => m.PostID == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.SingleOrDefaultAsync(m => m.PostID == id);
            var Cat = _context.Categories.SingleOrDefault(p => p.CategoryId == post.CategoryId);
            Cat.postcount--; //удалили из категории пост - уменьшили счётчик постов
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostID == id);
        }
    }
}
