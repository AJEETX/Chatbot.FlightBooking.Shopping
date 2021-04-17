using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace Evie.Chatbot.Dialogs
{
    public class DeleteToDoDialog : ComponentDialog
    {
        public DeleteToDoDialog(IConfiguration configuration) : base(nameof(DeleteToDoDialog))
        {
            string[] paths = { ".", "Dialogs", "DeleteToDoDialog", "DeleteToDoDialog.lg" };
            string fullPath = Path.Combine(paths);
            // Create instance of adaptive dialog.
            var DeleteToDoDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                Recognizer = CreateLuisRecognizer(configuration),
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            // Handle case where there are no items in timber list
                            new IfCondition()
                            {
                                // All conditions are expressed using adaptive expressions.
                                // See https://aka.ms/adaptive-expressions to learn more
                                Condition = "count(user.lists.timber) == 0 && count(user.lists.paint) == 0 && count(user.lists.tiles) == 0",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("${DeleteEmptyList()}"),
                                    new EndDialog()
                                }
                            },

                            // User could have specified the item and/ or list type to delete.
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.itemTitle",
                                        Value = "=@itemTitle"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.listType",
                                        Value = "=@listType"
                                    }
                                }
                            },

                            // Ask for list type first.
                            new TextInput()
                            {
                                Property = "dialog.listType",
                                Prompt = new ActivityTemplate("${GetListType()}"),
                                Value = "=@listType",
                                AllowInterruptions = "!@listType && turn.recognized.score >= 0.7",
                                Validations = new List<BoolExpression>()
                                {
                                    // Verify using expressions that the value is one of timber or tiles or paint
                                    "contains(createArray('timber', 'color', 'tiles'), toLower(this.value))",
                                },
                                OutputFormat = "=toLower(this.value)",
                                InvalidPrompt = new ActivityTemplate("${GetListType.Invalid()}"),
                                MaxTurnCount = 2,
                                DefaultValue = "timber",
                                DefaultValueResponse = new ActivityTemplate("${GetListType.DefaultValueResponse()}")
                            },

                            new IfCondition()
                            {
                                Condition = "count(user.lists[dialog.listType]) == 0",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("${NoItemsInList()}"),
                                    new EndDialog()
                                }
                            },

                            // Ask for title to delete
                            new ChoiceInput()
                            {
                                Choices = "user.lists[dialog.listType]",
                                Property = "dialog.itemTitle",
                                OutputFormat = ChoiceOutputFormat.Value,
                                Style = ListStyle.List,
                                Prompt = new ActivityTemplate("${GetItemTitleToDelete()}")
                            },

                            // remove item
                            new EditArray()
                            {
                                ItemsProperty = "user.lists[dialog.listType]",
                                Value = "=dialog.itemTitle",
                                ChangeType = EditArray.ArrayChangeType.Remove
                            },

                            new SendActivity("${DeleteConfirmationReadBack()}")
                        }
                    },
                    // Shows how to use dialog event to capture intent recognition event for more than one intent.
                    // Alternate to this would be to add two separate OnIntent events.
                    // This ensures we set any entities recognized by these two intents.
                    new OnDialogEvent()
                    {
                        Event = AdaptiveEvents.RecognizedIntent,
                        Condition = "#GetTitleToDelete || #GetListType",
                        Actions = new List<Dialog>()
                        {
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.listType",
                                        Value = "=@listType"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.itemTitle",
                                        Value = "=@itemTitle"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(DeleteToDoDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private async Task<DialogTurnResult> GetToDoTitleToDelete(DialogContext dc, System.Object options)
        {
            // Demonstrates using a custom code step to extract entities and set them in state.
            var todoList = dc.State.GetValue<string[]>("user.todos");
            string todoTitleStr = null;
            string[] todoTitle;
            // By default, recognized intents from a recognizer are available under turn.intents scope.
            // Recognized entities are available under turn.entities scope.
            dc.State.TryGetValue("turn.entities.todoTitle", out todoTitle);
            if (todoTitle != null && todoTitle.Length != 0)
            {
                if (Array.Exists(todoList, e => e == todoTitle[0]))
                {
                    todoTitleStr = todoTitle[0];
                }
            }
            if (todoTitleStr != null)
            {
                // Set the timber title in turn.todoTitle scope.
                dc.State.SetValue("turn.todoTitle", todoTitleStr);
            }
            return await dc.EndDialogAsync(options);
        }

        private static Recognizer CreateLuisRecognizer(IConfiguration Configuration)
        {
            return new RegexRecognizer
            {
                Intents = new List<IntentPattern>
                {
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)travel"),
                    new IntentPattern("BookFlight","(?i)fly"),
                    new IntentPattern("BookFlight","(?i)flight"),
                    new IntentPattern("Greeting","(?i)hi"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Greeting","(?i)hello"),
                    new IntentPattern("Greeting","(?i)hey"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("Help","(?i)query"),
                    new IntentPattern("Cancel","(?i)cancel"),
                    new IntentPattern("Exit","(?i)exit"),
                    new IntentPattern("Exit","(?i)bye"),
                    new IntentPattern("Cancel","(?i)no"),
                    new IntentPattern("Cancel","(?i)nope"),
                    new IntentPattern("Cancel","(?i)no thanks"),
                    new IntentPattern("BuyProduct","(?i)add"),
                    new IntentPattern("GetWeather","(?i)weather")
                }
            };
        }
    }
}