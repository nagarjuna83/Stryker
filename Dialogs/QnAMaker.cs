using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
namespace LuisBot.Dialogs
{
    [Serializable]
    public class QnAMaker : QnAMakerDialog
    {
        private static readonly string QnASubscriptiion = WebConfigurationManager.AppSettings["QnAMakerSubscriptionID"];
        private static readonly string QnApassword = WebConfigurationManager.AppSettings["QnAMakerPassword"];
        public QnAMaker() : base(new QnAMakerService(new QnAMakerAttribute(QnApassword, QnASubscriptiion)))
        {
        }

      }
}