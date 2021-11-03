using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace gRPC_v3
{
    public class GameService : Game.GameBase
    {
        private Dictionary<string, int> _players = new();
        private Dictionary<string, string> _questionsAnswers = new();

        public GameService()
        {
            _questionsAnswers.Add("Зайдет ли улитка в бар?", "Да");
            _questionsAnswers.Add("Рассказывает ли Поручик Ржевский анекдоты?", "Нет, насвистывает");
            _questionsAnswers.Add("Сколько баллов я получу за эту лабу?", "100");
        }

        private bool IsCorrectAnswer(Answer answer)
        {
            if (_questionsAnswers[answer.Question].ToLower() == answer.Message.ToLower())
            {
                _players[answer.Username] += 1;
                return true;
            }

            return false;
        }

        private string GetRandomQuestion()
        {
            return _questionsAnswers.ElementAt(new Random().Next(0, _questionsAnswers.Count)).Key;
        }

        public override Task<Reply> SendAnswer(Answer answer, ServerCallContext context)
        {
            var reply = new Reply();
            var username = answer.Username;
            
            //Это первая игра?
            if (_players.ContainsKey(username) is false)
            {
                //добавляем игрока в нашу "бд" с 0 правильных ответов
                _players.Add(username, 0);

                var question = GetRandomQuestion();
                reply.Question = question;
                reply.Message = $"Welcome, {username}! Your first question is: {question}";
            }
            else //Ну если нет, то
            {
                var nextRandomQuestion = GetRandomQuestion();
                reply.Question = nextRandomQuestion;
                
                reply.Message = $"{username}!"
                                + $"Your answer is {IsCorrectAnswer(answer)}"
                                + $" and now you have {_players[username]} points!"
                                + "\n"
                                + $"Next question: {nextRandomQuestion}";
            }
            
            return Task.FromResult(reply);
        }
    }
}