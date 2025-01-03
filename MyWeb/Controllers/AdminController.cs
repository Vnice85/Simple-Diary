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

        public ActionResult Index()
        {
            return View(db.Contents.OrderByDescending(c => c.id).ToList());
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
        [HttpPost]
        public ActionResult Delete(List<int> id)
        {
            foreach (var item in id)
            {
                var x = db.Contents.Find(item);
                db.Contents.Remove(x);
                db.SaveChanges();
            }
            return PartialView("Show", db.Contents.OrderByDescending(x => x.id).ToList());
        }

        public ActionResult Index2()
        {

            return View(db.Contents.OrderByDescending(c => c.id).ToList());
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
