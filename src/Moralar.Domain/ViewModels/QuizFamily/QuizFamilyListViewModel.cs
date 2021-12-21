using Moralar.Data.Enum;

namespace Moralar.Domain.ViewModels.Quiz
{
    public class QuizFamilyListViewModel
    {
        public string Id { get; set; }
        public string QuizId { get; set; }
        public string Title { get; set; }
        public string Created { get; set; }
        public string FamilyId { get; set; }
        public string HolderName { get; set; }
        public string HolderCpf { get; set; }
        public string HolderNumber { get; set; }
        public TypeStatus TypeStatus { get; set; }
        public TypeQuiz TypeQuiz { get; set; }
    }
}
