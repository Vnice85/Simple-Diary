using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Ajax.Utilities;
using WebApplication2.Models;
using System.Net.NetworkInformation;
using System.Web.UI.WebControls;
using System.Data.Entity;
using PagedList;
using System.Runtime.Remoting.Contexts;
using System.Web.UI;

namespace MyWeb.Controllers
{
    public class DiaryController : Controller
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

        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Instruction()
        {
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

        public ActionResult Detail(int id)
        {
            var x = db.Contents.Find(id);
            return View(x);
        }

        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }


        public ActionResult Gallery()
        {
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
            return View(imglist); // Trả danh sách đường dẫn ảnh tới View
        }
      
    }
}