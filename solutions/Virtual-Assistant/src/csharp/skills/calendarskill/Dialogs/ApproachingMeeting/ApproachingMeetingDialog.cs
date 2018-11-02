using CalendarSkill.Dialogs.ApproachingMeeting.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Extensions;
using Microsoft.Bot.Solutions.Skills;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CalendarSkill.Dialogs.ApproachingMeeting
{
    public class ApproachingMeetingDialog : CalendarSkillDialog
    {
        public ApproachingMeetingDialog(
            SkillConfiguration services,
            IStatePropertyAccessor<CalendarSkillState> accessor,
            IServiceManager serviceManager)
            : base(nameof(NextMeetingDialog), services, accessor, serviceManager)
        {
            var nextMeeting = new WaterfallStep[]
            {
                GetAuthToken,
                AfterGetAuthToken,
                ShowApproachingEventAsync,
            };

            // Define the conversation flow using a waterfall model.
            AddDialog(new WaterfallDialog(Actions.ShowEventsSummary, nextMeeting));

            // Set starting dialog for component
            InitialDialogId = Actions.ShowEventsSummary;
        }

        public async Task<DialogTurnResult> ShowApproachingEventAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var state = await _accessor.GetAsync(sc.Context);
                if (string.IsNullOrEmpty(state.APIToken))
                {
                    return await sc.EndDialogAsync(true);
                }

                var calendarService = _serviceManager.InitCalendarService(state.APIToken, state.EventSource, state.GetUserTimeZone());

                var eventList = await calendarService.GetUpcomingEvents();
                EventModel nextEvent = null;

                // get the first event
                foreach (var item in eventList)
                {
                    if (item.IsCancelled != true)
                    {
                        nextEvent = item;
                        break;
                    }
                }

                if (nextEvent != null && nextEvent.IsAllDay == false)
                {
                    var speakParams = new StringDictionary()
                    {
                        { "EventName", nextEvent.Title },
                        { "EventTime", nextEvent.StartTime.ToString("h:mm tt") },
                        { "PeopleList", string.Join(",", nextEvent.Attendees) },
                        { "Location", nextEvent.Location },
                    };

                    if (string.IsNullOrEmpty(nextEvent.Location))
                    {
                        // call in if no location
                        await sc.Context.SendActivityAsync(sc.Context.Activity.CreateReply(ApproachingMeetingResponses.ShowApproachingMeetingCallInMessage, _responseBuilder, speakParams));
                    }
                    else
                    {
                        // calculate time if there's location
                        speakParams.Add("Location", nextEventList[0].Location);
                        await sc.Context.SendActivityAsync(sc.Context.Activity.CreateReply(NextMeetingResponses.ShowNextMeetingMessage, _responseBuilder, speakParams));
                    }

                    await ShowMeetingList(sc, nextEventList, true);
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
    }
}