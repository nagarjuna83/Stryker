using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
namespace LuisBot.Dialogs
{
    [Serializable]
    public class QnAMaker : QnAMakerDialog
    {
        public QnAMaker() : base(new QnAMakerService(new QnAMakerAttribute("da6a5d9566ad47dba0fe275c79970bf1", "7a361945-bd06-4e9b-a13e-ebe73759c18b")))
        {
        }
    }
}