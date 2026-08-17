using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Dtos
{
    public class QuizQuestionWordDto
    {
        public int Id { get; set; }
        public string En { get; set; } = string.Empty;
        public string Tr { get; set; } = string.Empty;
        public string? Example { get; set; }
        public string? ListName { get; set; }
        public string? UserId { get; set; }
    }

    public class QuizQuestionResultDto
    {
        public QuizQuestionWordDto? Word { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string SelectedAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class QuizHistoryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Title { get; set; } = "Genel Test";
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public bool IsDailyQuiz { get; set; }
        public List<QuizQuestionResultDto> Results { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateQuizHistoryDto
    {
        public string? UserId { get; set; }

        public DateTime? Date { get; set; }

        [Required]
        public string Title { get; set; } = "Genel Test";

        public int Score { get; set; }

        public int MaxScore { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public bool IsDailyQuiz { get; set; }

        public List<QuizQuestionResultDto>? Results { get; set; }

        public string? ResultsJson { get; set; }
    }
}
