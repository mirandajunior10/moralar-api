using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Domain.ViewModels.QuestionAnswer
{
    public class QuestionAnswerListViewModel
    {
        public QuestionAnswerListViewModel()
        {
            //CreditCards = new List<string>();
            Answers = new List<string>();
        }
        public string QuestionAneswerId { get; set; }
        public string FamilyNumber { get; set; }
        public string FamilyHolderName { get; set; }
        public string FamilyHolderCpf { get; set; }

        public  string Title { get; set; }
        public long Date { get; set; }
        public string Question { get; set; }
        public List<string> Answers { get; set; }

    }
}
