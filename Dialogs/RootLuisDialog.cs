namespace LuisBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using System.Net.Mail;
    using System.Threading;
    using LuisBot.Services;

    [LuisModel("04808423-d5b5-45fe-87bf-4946b38c14bc", "9f69440c3add42219149bc256245118d")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
      
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Welcome")]
        public async Task Welcome(IDialogContext context, LuisResult result)
        {
            var name = context.Activity.From.Name;
            await context.PostAsync("Hi " + name + ".");
            await context.PostAsync("Welcome to Stryker supports.");
            await context.PostAsync("How can I help you today?");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("IssueNotify")]
        public async Task IssueNotify(IDialogContext context, LuisResult result)
        {
            var name = context.Activity.From.Name;
            if (result.Entities.Count == 2)
            {
                context.ConversationData.SetValue<string>(StrickerConstants.SystemName, result.Entities.First(x => x.Type == StrickerConstants.SystemName).Entity);
                context.ConversationData.SetValue<string>(StrickerConstants.IssueType, result.Entities.First(x => x.Type == StrickerConstants.IssueType).Entity);
            }
            else if (result.Entities.Count == 1 && result.Entities.Any(x => x.Type == StrickerConstants.SystemName))
            {
                context.ConversationData.SetValue<string>(StrickerConstants.SystemName, result.Entities.First(x => x.Type == StrickerConstants.SystemName).Entity);

            }
            else if (result.Entities.Count == 1 && result.Entities.Any(x => x.Type == StrickerConstants.IssueType))
            {
                context.ConversationData.SetValue<string>(StrickerConstants.IssueType, result.Entities.First(x => x.Type == StrickerConstants.IssueType).Entity);

            }

            await context.PostAsync("Do you have any error messages?");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("HaveErrorMessage")]
        public async Task HaveErrorMessage(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.ConversationData.SetValue<string>(StrickerConstants.HaveErrorMessage, "Yes");
            await context.PostAsync("Please provide error details");

            context.Wait(this.ResumeAfterError);
        }

        private async Task ResumeAfterError(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            context.ConversationData.SetValue<string>(StrickerConstants.ErrorMessage, message.Text);
            var qnaService = new QnAService();
            var query = await qnaService.QueryQnABot(message.Text);
            if (query.Answer != "No Match Found")
            {
                await context.PostAsync(query.Answer);
                await context.PostAsync("Let me know if I can help you with anything else.");
            }
            else
            {
                await context.PostAsync("What is the priority of the issue (P1 to P5)?");
            }
            context.Wait(this.MessageReceived);
        }

        private async Task ResumeAfterQnA(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("DontHaveErrorMessage")]
        public async Task DontHaveErrorMessage(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.ConversationData.SetValue<string>(StrickerConstants.HaveErrorMessage, "No");
            context.ConversationData.SetValue<string>(StrickerConstants.ErrorMessage, "");

            await context.PostAsync("What is the priority of the issue (P1 to P5)?");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Priority")]
        public async Task Priority(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.ConversationData.SetValue<string>(StrickerConstants.Priority, result.Entities.First(x => x.Type == StrickerConstants.Priority).Entity);

            await context.PostAsync("What is the urgency of the issue (Low/Medium/high)?");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Urgency")]
        public async Task Urgency(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.ConversationData.SetValue<string>(StrickerConstants.Urgency, result.Entities.First(x => x.Type == StrickerConstants.Urgency).Entity);
            await context.PostAsync("Thank you for reaching out.");
            SendDetails(context);
            await context.PostAsync("We are taking neccesary actions, We will get back to you.");
            context.Wait(this.MessageReceived);
        }

        public void SendDetails(IDialogContext context)
        {
            var Issue = new Issue
            {
                SystemName = context.ConversationData.GetValue<string>(StrickerConstants.SystemName),
                IssueType = context.ConversationData.GetValue<string>(StrickerConstants.IssueType),
                HaveErrorMessage = context.ConversationData.GetValue<string>(StrickerConstants.HaveErrorMessage),
                ErrorMessage = context.ConversationData.GetValue<string>(StrickerConstants.ErrorMessage),
                Priority = context.ConversationData.GetValue<string>(StrickerConstants.Priority),
                Urgency = context.ConversationData.GetValue<string>(StrickerConstants.Urgency),
            };
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential("ganeshanthati@gmail.com", "oejlyekdforlpmoj");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            MailMessage mail = new MailMessage();

            //Setting From , To and CC
            mail.From = new MailAddress("ganeshanthati@gmail.com");
            mail.To.Add(new MailAddress("Nagarjuna.Podapati@neudesic.com"));
            mail.To.Add(new MailAddress("ganesh.anthati@neudesic.com"));
            mail.Subject = "Striker L1 Issue!";
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = GetBody(Issue);
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            smtpClient.Send(mail);
        }

        public void SendDetails()
        {

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential("ganeshanthati@gmail.com", "oejlyekdforlpmoj");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            MailMessage mail = new MailMessage();

            //Setting From , To and CC
            mail.From = new MailAddress("ganeshanthati@gmail.com");
            mail.To.Add(new MailAddress("somethingbyzero@gmail.com"));
            mail.To.Add(new MailAddress("ganesh.anthati@neudesic.com"));
            mail.Subject = "Striker L1 Issue!";
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = "test";//GetBody(Issue);
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            //mail.IsBodyHtml = true;
            smtpClient.Send(mail);
        }

        private string GetBody(Issue issue)
        {
            var body = "<table><tr>" +
                 "<th>System Name</th>" +
                 "<th>Issue Type</th>" +
                 "<th>Priority</th>" +
                 "<th>Urgency</th>" +
                 "<th>Error</th>" +
              "<tr/><tr>" +
                 "<td>" + issue.SystemName + "</td>" +
                 "<td>" + issue.IssueType + "</td>" +
                 "<td>" + issue.Priority + "</td>" +
                 "<td>" + issue.Urgency + "</td>" +
                "<td>" + issue.ErrorMessage + "</td>" +
              "</tr></table>";
            return body;
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I can help you with logging issue in Stryker.");
            await context.PostAsync("start by saying Hi.");

            context.Wait(this.MessageReceived);
        }
    }
}
