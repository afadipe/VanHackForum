using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SleekSoftMVCFramework.ViewModel
{
    public class TagViewModel
    {
        public int?  Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a Title for the Tag.")]
        [StringLength(256, ErrorMessage = "The tag Title must be 256 characters or shorter.")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
    }
}