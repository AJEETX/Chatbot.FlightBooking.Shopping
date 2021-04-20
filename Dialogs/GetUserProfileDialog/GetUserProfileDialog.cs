using System.Collections.Generic;
using System.IO;
using AdaptiveExpressions.Properties;
using Evie.Chatbot.Recognizers;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;

namespace Evie.Chatbot.Dialogs
{
    public class GetUserProfileDialog : ComponentDialog
    {
        private readonly IConfiguration configuration;
        private Templates _templates;

        public GetUserProfileDialog(IConfiguration configuration) : base(nameof(GetUserProfileDialog))
        {
            this.configuration = configuration;
            _templates = Templates.ParseFile(Path.Combine(".", "Dialogs", "GetUserProfileDialog", "GetUserProfileDialog.lg"));

            // Create instance of adaptive dialog.
            var userProfileDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(_templates),
                Recognizer = new CrossTrainedRecognizerSet()
                {
                    Recognizers = new List<Recognizer>()
                        {
                                new CustomerRegexRecognizer().CreateRecognizer(),
                                //new LuisAdaptiveRecognizer()
                                //{
                                //    Id="LuisAppId",
                                //    ApplicationId = configuration["LuisAppId"],
                                //    EndpointKey =  configuration["LuisAPIKey"],
                                //    Endpoint = "https://" + configuration["LuisAPIHostName"]
                                //}
                        }
                },
                Triggers = new List<OnCondition>()
                {
                    // Actions to execute when this dialog begins. This dialog will attempt to fill user profile.
                    // Each adaptive dialog can have its own recognizer. The LU definition for this dialog is under GetUserProfileDialog.lu
                    // This dialog supports local intents. When you say things like 'why do you need my name' or 'I will not give you my name'
                    // it uses its own recognizer to handle those.
                    // It also demonstrates the consultation capability of adaptive dialog. When the local recognizer does not come back with
                    // a high-confidence recognition, this dialog will defer to the parent dialog to see if it wants to handle the user input.
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                             new IfCondition()
                            {
                                // All conditions are expressed using adaptive expressions.
                                // See https://aka.ms/adaptive-expressions to learn more
                                Condition = "user.profile.name != null",
                                Actions = new List<Dialog>()
                                {
                                    // show profile.
                                    new SendActivity("${ProfileReadBackAgain()}"),
                                    new EndDialog()
                                }
                            },
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "user.profile.name",

                                        // Whenever an adaptive dialog begins, any options passed in to the dialog via 'BeginDialog' are available through dialog.xxx scope.
                                        // Coalesce is a prebuilt function available as part of Adaptive Expressions. Take the first non-null value.
                                        // @EntityName is a short-hand for turn.recognized.entities.<EntityName>. Other useful short-hands are
                                        //     #IntentName is a short-hand for turn.intents.<IntentName>
                                        //     $PropertyName is a short-hand for dialog.<PropertyName>
                                        Value = "=coalesce(dialog.userName, @userName, @personName)"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "user.profile.age",
                                        Value = "=coalesce(dialog.userAge, @age)"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "user.profile.mobile",
                                        Value = "=@mobile"
                                    }
                                }
                            },
                            new TextInput()
                            {
                                Property = "user.profile.name",
                                Prompt = new ActivityTemplate("${AskFirstName()}"),
                                Validations = new List<BoolExpression>()
                                {
                                    // Name must be 3-50 characters in length.
                                    // Validations are expressed using Adaptive expressions
                                    // You can access the current candidate value for this input via 'this.value'
                                    "count(this.value) >= 3",
                                    "count(this.value) <= 50"
                                },
                                InvalidPrompt = new ActivityTemplate("${AskFirstName.Invalid()}"),

                                // Because we have a local recognizer, we can use it to extract entities.
                                // This enables users to say things like 'my name is vishwac' and we only take 'vishwac' as the name.
                                Value = "=@personName",

                                OutputFormat = "${join(foreach(split(this.value, ' '), item, concat(toUpper(substring(item, 0, 1)), substring(item, 1))), ' ')}",

                                // We are going to allow any interruption for a high confidence interruption intent classification .or.
                                // when we do not get a value for the personName entity.
                                AllowInterruptions = "turn.recognized.score >= 0.9 || !@personName",
                            },
                            new TextInput()
                            {
                                Property = "user.profile.age",
                                Prompt = new ActivityTemplate("${AskUserAge()}"),
                                Validations = new List<BoolExpression>()
                                {
                                    // Age must be within 1-150.
                                    "int(this.value) >= 1",
                                    "int(this.value) <= 150"
                                },
                                InvalidPrompt = new ActivityTemplate("${AskUserAge.Invalid()}"),
                                UnrecognizedPrompt = new ActivityTemplate("${AskUserAge.Unrecognized()}"),

                                // We have both a number recognizer as well as age prebuilt entity recognizer added. So take either one we get.
                                // LUIS returns a complex type for prebuilt age entity. Take just the number value.
                                Value = "=coalesce(@age.number, @number)",

                                // Allow interruption if we do not get either an age or a number.
                                AllowInterruptions = "!@age && !@number"
                            },
                            new TextInput()
                            {
                                Property = "user.profile.mobile",
                                Prompt = new ActivityTemplate("${AskUserMobile()}"),
                                Validations = new List<BoolExpression>()
                                {
                                    //mobile number should more than 5 digits
                                    "length(this.value) > 5"
                                },
                                MaxTurnCount = 3,
                                DefaultValue = "04000000000",
                                DefaultValueResponse = new ActivityTemplate("${DefaultUserMobileResponse()}"),
                                UnrecognizedPrompt = new ActivityTemplate("${AskUserMobile()}"),
                                InvalidPrompt = new ActivityTemplate("${AskUserMobile()}"),
                                Value="=@mobile",
                                AllowInterruptions = "!@mobile"
                            },
                            new SendActivity("${ProfileReadBack()}")
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(userProfileDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
    }
}