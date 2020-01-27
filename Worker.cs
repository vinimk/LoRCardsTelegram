using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoRCards.WorkerOptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.InlineQueryResults;

namespace LoRCards
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static Telegram.Bot.TelegramBotClient _botClient;
        private static List<Card> _cards;

        public Worker(ILogger<Worker> logger, TelegramWorkerOptions telegramWorkerOptions)
        {
            _logger = logger;
            _botClient = new Telegram.Bot.TelegramBotClient(telegramWorkerOptions.APIKey);
            _cards = JsonImporter.JsonImporter.ReadCardsFromJson();
            _botClient.OnInlineQuery += _botClient_OnInlineQuery;
            //_botClient.OnInlineResultChosen += _botClient_OnInlineResultChosen;
            _botClient.StartReceiving();
        }

        //private void _botClient_OnInlineResultChosen(object sender, Telegram.Bot.Args.ChosenInlineResultEventArgs e)
        //{
        //    Card chosenCard = _cards.Where(x => x.cardCode == e.ChosenInlineResult.ResultId).FirstOrDefault();
        //    if (chosenCard != null)
        //    {
        //        _botClient.SendPhotoAsync(e.ChosenInlineResult.From.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(chosenCard.imageUrl), chosenCard.name);
        //    }
        //}

        private void _botClient_OnInlineQuery(object sender, Telegram.Bot.Args.InlineQueryEventArgs e)
        {
            //var chosenCard = _cards[0];
            //_botClient.SendPhotoAsync(e.InlineQuery.From.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(chosenCard.imageUrl), chosenCard.name);
            if (e.InlineQuery.Query == null)
                return;
            if (e.InlineQuery.Query == "")
                return;
            try
            {
                var inlineQueryResults = GetInlineQueryResultPhotos(e.InlineQuery.Query);
                _logger.LogInformation($"Query:{e.InlineQuery.Query} results:{inlineQueryResults.Count()}");

                var task = _botClient.AnswerInlineQueryAsync(e.InlineQuery.Id, inlineQueryResults, 6000000, true);
                task.Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString(), ex);
            }
        }

        private static IEnumerable<InlineQueryResultBase> GetInlineQueryResultPhotos(string query)
        {
            List<InlineQueryResultBase> result = new List<InlineQueryResultBase>();

            var searchResults = _cards.Where(x => x.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                    x.type.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                    x.descriptionRaw.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                    x.region.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                    x.keywords.Where(y => y.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1).Any()).Take(50);
            int i = 0;
            foreach (var card in searchResults)
            {
                result.Add(new InlineQueryResultPhoto(id: card.cardCode, card.imageUrl, card.imageUrl)
                {
                    Caption = card.name,
                    PhotoHeight = 680,
                    PhotoWidth = 1024,
                    
                });
                //        InlineQueryResultArticle resultPhoto = new InlineQueryResultArticle(
                //// id's should be unique for each type of response
                //id: card.cardCode,
                //// Title of the option
                //title: card.name,
                //// This is what is returned
                //new InputTextMessageContent("text that is returned") { ParseMode = Telegram.Bot.Types.Enums.ParseMode.Default })
                //        {
                //            // This is just the description shown for the option
                //            Description = "test"
                //        };
                //InlineQueryResultPhoto resultPhoto = new InlineQueryResultPhoto(card.cardCode, card.imageUrl, card.imageUrl);
                //resultPhoto.Description = card.name;
                //resultPhoto.Caption = card.name;
                //resultPhoto.Title = card.name;
                //result.Add(resultPhoto);
                i++;

            }

            return result;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
