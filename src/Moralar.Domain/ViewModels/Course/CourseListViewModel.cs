using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Course
{
    public class CourseListViewModel:BaseViewModel
    {
       
        public string Title { get; set; }



      
        public string Img { get; set; }



        public DateTime StartDate { get; set; }


        public DateTime EndDate { get; set; }



     
        public string Schedule { get; set; }




        public string Place { get; set; }




        public string WorkLoad { get; set; }



      
        public string Description { get; set; }



      
        public int StartTargetAudienceAge { get; set; }


     
        public int EndTargetAudienceAge { get; set; }

      
        public TypeGenre TypeGenre { get; set; }


       
        public int NumberOfVacancies { get; set; }
    }
}
