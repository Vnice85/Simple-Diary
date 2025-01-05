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

        public ActionResult Home(int? page, int? size)
        {
            if (Session["vnice"] == null)
            {
                return RedirectToAction("Login", "Authentication");
            }
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
            if (Session["vnice"] == null)
            {
                return RedirectToAction("Login", "Authentication");
            }
            ViewBag.id_color = db.Colors.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Content content, HttpPostedFileBase main_image, string imageLink)
        {
            try
            {
                var tmp = new Content();
                imageLink = imageLink.Trim();
                if (main_image != null && main_image.ContentLength > 0)
                {
                    string root = Server.MapPath("/Data");
                    string path = Path.Combine(root, Path.GetFileName(main_image.FileName));
                    main_image.SaveAs(path);
                    tmp.main_image = "/Data/" + Path.GetFileName(main_image.FileName);
                }
                else if (imageLink != "")
                {
                    tmp.main_image = imageLink;
                }
                else
                {
                    ViewBag.Status = "ERROR";
                    ViewBag.id_color = db.Colors.ToList();
                    return View(content);
                }
                tmp.main_content = content.main_content;
                tmp.id_color = content.id_color;
                tmp.date_upload = DateTime.Now;
                tmp.content1 = content.content1;
                db.Contents.Add(tmp);
                db.SaveChanges();
            }
            catch
            {
                ViewBag.id_color = db.Colors.ToList();
                ViewBag.Status = "ERROR";
                return View(content);
            }
            ViewBag.id_color = db.Colors.ToList();
            return View(content);

        }
        public ActionResult Edit(int id)
        {
            if (Session["vnice"] == null)
            {
                return RedirectToAction("Login", "Authentication");
            }
            var x = db.Contents.Find(id);
            ViewBag.id_color = new SelectList(db.Colors, "id_color", "color_name", x.id_color);

            return View(x);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Content content, HttpPostedFileBase main_image, string imageLink)
        {
            try
            {
                var editedContent = db.Contents.Find(content.id);
                imageLink = imageLink.Trim();
                if (main_image != null && main_image.ContentLength > 0)
                {
                    string root = Server.MapPath("/Data");
                    string path = Path.Combine(root, Path.GetFileName(main_image.FileName));
                    main_image.SaveAs(path);
                    editedContent.main_image = "/Data/" + Path.GetFileName(main_image.FileName);
                }
                else if (imageLink != "")
                {
                    editedContent.main_image = imageLink;
                }
                editedContent.main_content = content.main_content;
                editedContent.id_color = content.id_color;
                editedContent.content1 = content.content1;
                db.SaveChanges();
            }
            catch
            {
                ViewBag.Status = "ERROR";
                return View();
            }
            return RedirectToAction("detail", new { id = content.id });
        }

        public ActionResult Delete(List<int> id)
        {
            foreach (var item in id)
            {
                var x = db.Contents.Find(item);
                db.Contents.Remove(x);
                db.SaveChanges();
            }
            return RedirectToAction("Home");
        }

        public ActionResult AddGalleryImage()
        {
            if (Session["vnice"] == null)
            {
                return RedirectToAction("Login", "Authentication");
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddGalleryImage(List<HttpPostedFileBase> image)
        {
            try
            {
                if (Session["vnice"] == null)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                
                foreach (var item in image)
                {
                    if (item != null && item.ContentLength > 0)
                    {
                        string root = Server.MapPath("/Images");
                        string extension = Path.GetExtension(item.FileName);
                        string uniqueName = Path.GetFileNameWithoutExtension(item.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                        string path = Path.Combine(root, uniqueName);
                        item.SaveAs(path);
                    }
                    else
                    {
                        ViewBag.Status = "ERROR";
                        return View();
                    }
                }
             
            }
            catch
            {
                ViewBag.Status = "ERROR";
                return View();
            }
            return RedirectToAction("Home");
        }
        public ActionResult DeleteGalleryImage()
        {
            if (Session["vnice"] == null)
            {
                return RedirectToAction("Login", "Authentication");
            }
            var imglist = new List<string>();
            string root = Server.MapPath("/Images"); // Thư mục gốc chứa ảnh

            if (Directory.Exists(root))
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" }; // Các định dạng file ảnh
                var files = Directory.GetFiles(root);

                foreach (var file in files)
                {
                    // Kiểm tra xem file có thuộc định dạng ảnh không
                    if (Array.Exists(imageExtensions, ext => ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase)))
                    {
                        // Chuyển đường dẫn tuyệt đối thành đường dẫn tương đối
                        string relativePath = "/Images/" + Path.GetFileName(file);
                        imglist.Add(relativePath);
                    }
                }
            }
            return View(imglist);
        }
        [HttpPost]
        public ActionResult DeleteGalleryImage(List<string> deleteItems)
        {
            string root = Server.MapPath("/Images");

            if (deleteItems != null && deleteItems.Any())
            {
                foreach (var item in deleteItems)
                {
                    string filePath = Server.MapPath(item); // Chuyển đổi đường dẫn tương đối thành đường dẫn tuyệt đối

                    // Kiểm tra nếu tệp tin tồn tại
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath); // Xóa tệp tin
                        }
                        catch (Exception ex)
                        {
                            // Xử lý lỗi nếu có (Ví dụ: Không thể xóa tệp)
                            ModelState.AddModelError("", "Lỗi khi xóa ảnh: " + ex.Message);
                        }
                    }
                }
            }
            return RedirectToAction("DeleteGalleryImage");
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

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                string root = Server.MapPath("/Data");
                string x = Path.Combine(root, Path.GetFileName(upload.FileName));
                upload.SaveAs(x);

            }
            return Json(new
            {
                uploaded = true,
                url = Url.Content("~/Data/" + Path.GetFileName(upload.FileName))
            });

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
