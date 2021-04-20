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
using System.Collections.Generic;
using System.IO;

namespace Evie.Chatbot.Dialogs
{
    public class ViewToDoDialog : ComponentDialog
    {
        public ViewToDoDialog(IConfiguration configuration) : base(nameof(ViewToDoDialog))
        {
            string[] paths = { ".", "Dialogs", "ViewToDoDialog", "ViewToDoDialog.lg" };
            string fullPath = Path.Combine(paths);
            var _recognizer = new LuisAdaptiveRecognizer()
            {
                Id = nameof(LuisAdaptiveRecognizer),
                ApplicationId = configuration["LuisAppId"],
                EndpointKey = configuration["LuisAPIKey"],
                Endpoint = "https://" + configuration["LuisAPIHostName"]
            };
            // Create instance of adaptive dialog.
            var ViewToDoDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                Recognizer = new CrossTrainedRecognizerSet()
                {
                    Recognizers = new List<Recognizer>()
                        {
                                CustomRegexRecognizer.CreateViewRecognizer(),
                                //_recognizer
                        }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            
                            // See if any list has any items.
                            new IfCondition()
                            {
                                Condition = "user.profile.name !=null",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("${ShowList()}"),
                                    new CancelDialog()
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${NoItemsInLists()}"),
                                    new CancelDialog()
                                }
                            },
                            // See if any list has any items.
                            new IfCondition()
                            {
                                Condition = "count(user.lists.timber) != 0 || count(user.lists.paint) != 0 || count(user.lists.tiles) != 0",
                                Actions = new List<Dialog>()
                                {
                                    // Get list type
                                    new TextInput()
                                    {
                                        Property = "dialog.listType",
                                        Prompt = new ActivityTemplate("${GetListType()}"),
                                        Value = "=@listType",
                                        AllowInterruptions = "!@listType && turn.recognized.score >= 0.7",
                                        Validations = new List<BoolExpression>()
                                        {
                                            // Verify using expressions that the value is one of timber or tiles or paint
                                            "contains(createArray('timber', 'tiles', 'paint', 'all'), toLower(this.value))",
                                        },
                                        OutputFormat = "=toLower(this.value)",
                                        InvalidPrompt = new ActivityTemplate("${GetListType.Invalid()}"),
                                        MaxTurnCount = 2,
                                        DefaultValue = "Soft wood",
                                        DefaultValueResponse = new ActivityTemplate("${GetListType.DefaultValueResponse()}")
                                    },
                                    new SendActivity("${ShowList()}")
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${NoItemsInLists()}")
                                }
                            },
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(ViewToDoDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
    }
}