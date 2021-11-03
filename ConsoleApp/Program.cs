using System;
using System.Net.Http;
using System.Threading.Tasks;
using gRPC_v3;
using Grpc.Net.Client;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions()
            {
                HttpHandler = GetHttpClientHandler()
            });

            Console.Write("Напишите ваше имя: ");
            var username = Console.ReadLine();

            Console.WriteLine("Начинаем игру! (не в кальмара, у нас тут 'Самый умный!')");
            var question = "";
            var client = new Game.GameClient(channel);
            while (true)
            {
                if (string.IsNullOrEmpty(question))
                {
                    var firstQuestion = await client.SendAnswerAsync(new Answer{Question = "", Username = username});
                    question = firstQuestion.Question;
                    Console.WriteLine(firstQuestion.Message);
                }
                
                Console.Write("Ответ убил: ");
                var answer = Console.ReadLine();

                // обмениваемся сообщениями с сервером
                var requst = new Answer {Question = question, Message = answer, Username = username};
                var reply = await client.SendAnswerAsync(requst);
                Console.WriteLine(reply.Message);
                question = reply.Question;
            }
        }

        //этот костыль нужен для MacOS из-за https сертификатов, мб для винды тоже нужен, хз
        private static HttpClientHandler GetHttpClientHandler()
        {
            var httpHandler = new HttpClientHandler();
            // Return `true` to allow certificates that are untrusted/invalid
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return httpHandler;
        }
    }
}