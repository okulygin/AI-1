using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalendarSkill.Dialogs.ApproachingMeeting.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Extensions;
using Microsoft.Bot.Solutions.Skills;

namespace CalendarSkill.Dialogs.ApproachingMeeting
{
    public class ApproachingMeetingDialog : CalendarSkillDialog
    {
        public ApproachingMeetingDialog(
            SkillConfiguration services,
            IStatePropertyAccessor<CalendarSkillState> accessor,
            IServiceManager serviceManager)
            : base(nameof(ApproachingMeetingDialog), services, accessor, serviceManager)
        {
            var approachingMeeting = new WaterfallStep[]
            {
                //GetAuthToken,
                //AfterGetAuthToken,
                ShowApproachingEvent,
                NavigateToMeetingLocation,
            };

            // Define the conversation flow using a waterfall model.
            AddDialog(new WaterfallDialog(Actions.ShowApproachingEvent, approachingMeeting));
            AddDialog(new ConfirmPrompt(Actions.NavigateToMeetingLocation));

            // Set starting dialog for component
            InitialDialogId = Actions.ShowApproachingEvent;
        }

        public async Task<DialogTurnResult> ShowApproachingEvent(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var state = await _accessor.GetAsync(sc.Context);
                if (string.IsNullOrEmpty(state.APIToken))
                {
                    return await sc.EndDialogAsync(true);
                }

                var calendarService = _serviceManager.InitCalendarService(state.APIToken, state.EventSource);

                var eventList = await calendarService.GetUpcomingEvents();
                EventModel nextEvent = null;

                // get the first event
                //foreach (var item in eventList)
                //{
                //    if (item.IsCancelled != true)
                //    {
                //        nextEvent = item;
                //        break;
                //    }
                //}

                nextEvent = new EventModel (EventSource.Microsoft) { Title = "Test", TimeZone = TimeZoneInfo.Utc, StartTime = DateTime.UtcNow.AddMinutes(-30), Attendees = new System.Collections.Generic.List<EventModel.Attendee> { new EventModel.Attendee { Address = "a", DisplayName = "Lauren" } }, Location = "CCP"};
                var timeDiff = DateTime.UtcNow.Subtract(nextEvent.StartTime).TotalMinutes;
                if (nextEvent != null && (!nextEvent.IsAllDay.HasValue || nextEvent.IsAllDay == false) && timeDiff < 60)
                {
                    var speakParams = new StringDictionary()
                    {
                        { "EventName", nextEvent.Title },
                        { "EventMinutesLeft", timeDiff.ToString("N0") },
                        { "PeopleList", string.Join(",", nextEvent.Attendees.Select(a => a.DisplayName)) },
                        { "EventLocation", nextEvent.Location },
                    };

                    // ask the user whether or not navigate to the location
                    return await sc.PromptAsync(Actions.NavigateToMeetingLocation, new PromptOptions
                    {
                        Prompt = sc.Context.Activity.CreateReply(ApproachingMeetingResponses.ApproachingMeetingNavigateToLocationMessage, _responseBuilder, speakParams),
                    });
                }

                state.Clear();
                return await sc.EndDialogAsync(true);
            }
            catch
            {
                await HandleDialogExceptions(sc);
                throw;
            }
        }

        public async Task<DialogTurnResult> NavigateToMeetingLocation(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = (bool)sc.Result;
                if (result)
                {
                    // We trigger a Token Request from the Parent Bot by sending a "TokenRequest" event back and then waiting for a "TokenResponse"
                    // TODO Error handling - if we get a new activity that isn't an event
                    var response = sc.Context.Activity.CreateReply();
                    response.Type = ActivityTypes.Event;
                    response.Name = "tokens/request";

                    // Send the tokens/request Event
                    await sc.Context.SendActivityAsync(response);
                }

                return await sc.EndDialogAsync();
            }
            catch
            {
                await HandleDialogExceptions(sc);
                throw;
            }
        }
    }
}