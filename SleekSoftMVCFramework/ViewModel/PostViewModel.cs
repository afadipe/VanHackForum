using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SleekSoftMVCFramework.ViewModel
{

    public class EditPostViewModel
    {
        public int Id { get; set; }
        [Required]
        public string PostComment { get; set; }
    }

    public class PostCommentViewModel
    {
        public int TopicId { get; set; }
        [Required]
        public string PostComment { get; set; }
    }
    public class  ActivePostsViewModel
    {
        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }

        public TopicViewModel TopicViewModel { get; set; }
        public PostCommentViewModel PostCommentViewModel { get; set; }
    };

    public class PostViewModel
    {

        public int Id { get; set; }
        public int TopicId { get; set; }
        public string IPAddress { get; set; }
        public string Content { get; set; }
        public int? VoteCount { get; set; }
        public Boolean IsTopicStarter { get; set; }
        //The person that created this post
        public long? CreatedBy { get; set; }
        public string DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public string CreateByFullName { get; set; }
        public string CreateByUserName { get; set; }
        public string CreateByAvatar { get; set; }

        public int? HasVoted { get; set; }
        //Getting the likes
        public int? PostLikeCount { get; set; }

        public List<PostLikeViewModel> Votes { get; set; }


    }


    public class PostViewListModel
    {
        public TopicViewModel Topic { get; set; }
        public List<PostViewModel> Posts { get; set; }
    }
}