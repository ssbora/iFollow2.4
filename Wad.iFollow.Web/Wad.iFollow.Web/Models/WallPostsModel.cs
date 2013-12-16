using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Diagnostics;

namespace Wad.iFollow.Web.Models
{
    public class WallData
    {
        public string Path { get; set; }
        public string Message { get; set; }
        public string Author { get; set; }
    }

    public class WallPostsModel
    {
        public List<WallData> wallElements { get; set; }

        public class PostsComparer: IComparer<post>
        {
            public int Compare(post p1, post p2)
            {
                int returnValue = 1;
                returnValue = ((System.DateTime)p1.dateCreated).CompareTo((System.DateTime)p2.dateCreated);
                return returnValue;
            }
        }

        public void BuildFromImagesAndPosts(ICollection<post> posts, ICollection<image> images)
        {
            List<post> orderedList = posts.ToList();
            orderedList.Sort(new PostsComparer());
            wallElements = new List<WallData>();

            for (int index = orderedList.Count() - 1; index >= 0; index--)
            {
                post currentPost = orderedList.ElementAt(index);
                WallData newModel = new WallData();
                newModel.Message = currentPost.message;

                using(var conn = new ifollowdatabaseEntities4())
                {
                    user author = conn.users.First(u => u.id == currentPost.ownerId);
                    newModel.Author = author.firstName + " " + author.lastName;
                }

                if (orderedList.ElementAt(index).imageId != null)
                {
                    string imagePath = images.First(i => i.id == currentPost.imageId).url;
                    string path = @"~/Images/UserPhotos/" + imagePath;
                    newModel.Path = path;
                }
                else
                {
                    newModel.Path = "";//"~/Images/1.jpg";
                }

                wallElements.Add(newModel);
            }
        }
    }
}