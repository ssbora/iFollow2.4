using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wad.iFollow.Web.Models;

namespace Wad.iFollow.Web.Controllers
{
    public class WallController : Controller
    {
        //
        // GET: /Main/

        public ActionResult MainPage()
        {
            user currentUser = Session["user"] as user;
            WallPostsModel wpm = new WallPostsModel();

            using (var entities = new ifollowdatabaseEntities4())
            {

                //If there is no user, we are redirected to Login page
                
                try
                {

                    currentUser = entities.users.First(u => u.id == currentUser.id);
                }
                catch
                {
                    return RedirectToAction("Index", "Home");
                }

                ICollection<post> posts = currentUser.posts;
                ICollection<image> images = currentUser.images;
                DbSet<follower> followers = entities.followers;
 
                foreach(follower f in followers)
                {
                    if (f.followerId == currentUser.id)
                    {
                        using (var conn = new ifollowdatabaseEntities4())
                        {
                            user ff = conn.users.First(u => u.id == f.followedId);
                            foreach (image i in ff.images)
                            {
                                images.Add(i);
                            }

                            foreach (post p in ff.posts)
                            {
                                posts.Add(p);
                            }
                        }
                    }
                }

                wpm.BuildFromImagesAndPosts(currentUser.posts, currentUser.images);
            }

            return View(wpm);          
        }

        public ActionResult WallPost()
        {
            return PartialView("_WallPosts");
        }

        public JsonResult AutoCompleteSearch(string term)
        {
             using (var entities = new ifollowdatabaseEntities4())
            {
                 //var user = Session["user"] as User
                var result = (from r in entities.users
                              where r.firstName.ToLower().Contains(term.ToLower())
                              select new { r.firstName }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void Rate(string id, string val)
        { }


        public ActionResult Settings()
        {
            ViewBag.Message = "Your contact page.";

            SettingsModel sm = new SettingsModel();
            user currentUser = Session["user"] as user;

            using(var conn = new ifollowdatabaseEntities4())
            {
                try
                {
                    currentUser = conn.users.First(u => u.id == currentUser.id);
                }
                catch
                {
                    Url.Action("LogOff", "Account");
                    //return RedirectToAction("Index", "Home");
                }
                
                if (currentUser != null)
                {
                    sm.firstName = currentUser.firstName;
                    sm.lastName = currentUser.lastName;
                    sm.country = currentUser.country;
                    sm.city = currentUser.city;
                    //sm.birthDate = (System.DateTime)currentUser.birthdate;
                }
            }

            return PartialView("_SettingsModal");
        }

        public ActionResult Post()
        {
            ViewBag.Message = "Your posting page.";
            return PartialView("_PostSettings");
        }

        public ActionResult Profile(string user)
        {
            long currentUserId;
            ProfileModel pm = new ProfileModel();


            if (user == "current")
            {
                currentUserId = (Session["user"] as user).id;
            }
            else
            {
                currentUserId = (long)Convert.ToDouble(user);
            }
                        
            using (var entities = new ifollowdatabaseEntities4())
            {
                user currentUser = entities.users.First(u => u.id == currentUserId);
                pm.userName = currentUser.firstName;
                pm.postsCount = currentUser.posts.Count();
                pm.followersCount = currentUser.followers.Count();
                pm.followedCount = currentUser.followers1.Count();
                pm.elements.BuildFromImagesAndPosts(currentUser.posts, currentUser.images);
            }
            ViewBag.Message = "Your profile page.";
            return PartialView("_ProfilePage", pm);
        }

        public ActionResult Followers()
        {
            ViewBag.Message = "Your followers page.";
            user currentUser = Session["user"] as user;
            
            FollowersModel fm = new FollowersModel();
            try
            {
                fm.BuildModelForUser(currentUser.id);
            }
            catch
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }
           

            return PartialView("_Followers", fm);
        }
    }
}
