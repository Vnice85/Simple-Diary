using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class AdminController : Controller
    {
        private MyDiary db = new MyDiary();

        public ActionResult Index(int? page, int? size)
        {
            int pageNumber = page ?? 1;
            int pageSize = size ?? 10;
            if (Session["filter"] == null || (int)Session["filter"] == 0)
            {
                Session["filter"] = 0;
                var pl = db.Contents.OrderByDescending(c => c.date_upload).ToPagedList(pageNumber, pageSize);
                ViewBag.Count = db.Contents.Count();
                return View(pl);
            }
            var pagedList = db.Contents.OrderBy(c => c.date_upload).ToPagedList(pageNumber, pageSize);
            ViewBag.Count = db.Contents.Count();
            return View(pagedList);
        }
        public ActionResult Detail(int id)
        {
            var x = db.Contents.Find(id);
            return View(x);
        }

        public ActionResult Create()
        {
            ViewBag.id_color = db.Colors.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Content content, HttpPostedFileBase main_image)
        {
            var tmp = new Content();
            if (main_image != null && main_image.ContentLength > 0)
            {
                string root = Server.MapPath("/Data");
                string path = Path.Combine(root, Path.GetFileName(main_image.FileName));
                main_image.SaveAs(path);
                tmp.main_image = "/Data/" + Path.GetFileName(main_image.FileName);
            }
            else
            {
                ViewBag.id_color = db.Colors.ToList();
                return View(content);
            }
            tmp.main_content = content.main_content;
            tmp.id_color = content.id_color;
            tmp.date_upload = DateTime.Now;
            tmp.content1 = content.content1;
            db.Contents.Add(tmp);
            db.SaveChanges();
            ViewBag.id_color = db.Colors.ToList();
            return View(content);

        }
 
        public ActionResult Delete(List<int> id)
        {
            foreach (var item in id)
            {
                var x = db.Contents.Find(item);
                db.Contents.Remove(x);
                db.SaveChanges();
            }
            return RedirectToAction("index");
        }

        public ActionResult AddGalleryImage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddGalleryImage(List<HttpPostedFileBase> image)
        {
            foreach(var item in image)
            {
                if (item != null && item.ContentLength > 0)
                {
                    string root = Server.MapPath("/Images");
                    string extension = Path.GetExtension(item.FileName);
                    string uniqueName = Path.GetFileNameWithoutExtension(item.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string path = Path.Combine(root, uniqueName);
                    item.SaveAs(path);
                }
            }
            return View();
        }
        public PartialViewResult Find(string find_keyword)
        {
            var x = (from a in db.Contents
                     join b in db.Colors
                     on a.id_color equals b.id_color
                     where b.color_name.Contains(find_keyword.Trim()) || a.main_content.Contains(find_keyword.Trim()) || a.date_upload.ToString().Contains(find_keyword.Trim())
                     orderby a.date_upload descending
                     select new
                     {
                         a.id,
                         b.id_color,
                         a.content1,
                         a.main_content,
                         a.main_image,
                         a.date_upload,
                     }).ToList();

            var ls = x.Select(z => new WebApplication2.Models.Content
            {
                id = z.id,
                id_color = z.id_color,
                content1 = z.content1,
                main_content = z.main_content,
                main_image = z.main_image,
                date_upload = z.date_upload
            });
            return PartialView(ls.ToList());
        }

        public PartialViewResult Filter(int? page, int? size, string find_keyword, int orderby = 0)
        {
            find_keyword = string.IsNullOrEmpty(find_keyword) ? string.Empty : find_keyword.Trim();
            int pageNumber = page ?? 1;
            int pageSize = size ?? 10;
            var query = (from a in db.Contents
                         join b in db.Colors
                         on a.id_color equals b.id_color
                         where b.color_name.Contains(find_keyword)
                         || a.main_content.Contains(find_keyword)
                         || a.date_upload.ToString().Contains(find_keyword)
                         || string.IsNullOrEmpty(find_keyword)
                         select new
                         {
                             a.id,
                             b.id_color,
                             a.content1,
                             a.main_content,
                             a.main_image,
                             a.date_upload,
                         });
            var result = query.ToList().Select(z => new WebApplication2.Models.Content
            {
                id = z.id,
                id_color = z.id_color,
                content1 = z.content1,
                main_content = z.main_content,
                main_image = z.main_image,
                date_upload = z.date_upload
            });
            if (orderby == 0)
            {
                Session["filter"] = 0;
                result = result.OrderByDescending(x => x.date_upload);
            }
            if (orderby != 0)
            {
                Session["filter"] = 1;
                result = result.OrderBy(x => x.date_upload);
            }
            return PartialView(result.ToPagedList(pageNumber, pageSize));
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
