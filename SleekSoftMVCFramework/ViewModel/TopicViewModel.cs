using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SleekSoftMVCFramework.ViewModel
{
    public class TopicPostContentVm
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }

    public class ActiveTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    public class LastPost
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public string Avatar { get; set; }

        public string DatePosted { get; set; }
    }

    public class TopicViewModel
    {
        public int? Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a Title for the Topic.")]
        [StringLength(256, ErrorMessage = "The tag Title must be 256 characters or shorter.")]
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a Post for the Topic.")]
        [Display(Name = "Post Content")]
        public string PostContent { get; set; }
        public string TopicImage { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must select a Category for the Topic.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Categorys { get; set; }
        public string TopicCategory { get; set; }
        public int? TopicView { get; set; }
        public int? NoOfPost { get; set; }
        public long? CreatedBy { get; set; }
        public string DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public string CreateByFullName { get; set; }
        public string CreateByUserName { get; set; }

        public string CreateByUserAvatar { get; set; }
        //Post
        public List<PostViewModel> Posts { get; set; }
        public PostViewModel StarterPost { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        //LastPost
        public LastPost LastPost { get; set; }

        public int? IsFollowing { get; set; }



    }
}