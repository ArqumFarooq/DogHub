using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DogHub.HelpingClasses
{
    public class DogBreedDTO
    {
        public int PK_DogBreedId { get; set; }
        public string DogName { get; set; }
        public int? ParentBreedId { get; set; }
        public string ParentBreedName { get; set; }
        public string Origin { get; set; }
        public string LifeSpan { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string SysCreatedDate { get; set; }
        public string SysModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}