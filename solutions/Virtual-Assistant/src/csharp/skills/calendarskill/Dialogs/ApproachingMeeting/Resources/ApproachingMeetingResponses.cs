  
// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Solutions.Dialogs;

namespace CalendarSkill.Dialogs.ApproachingMeeting.Resources
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public static class ApproachingMeetingResponses
    {
        private static readonly ResponseManager _responseManager;

		static ApproachingMeetingResponses()
        {
            var dir = Path.GetDirectoryName(typeof(ApproachingMeetingResponses).Assembly.Location);
            var resDir = Path.Combine(dir, @"Dialogs\ApproachingMeeting\Resources");
            _responseManager = new ResponseManager(resDir, "ApproachingMeetingResponses");
        }

        // Generated accessors  
        public static BotResponse ApproachingMeetingMessage => GetBotResponse();
          
        public static BotResponse ShowApproachingMeetingCallInMessage => GetBotResponse();
          
        public static BotResponse ShowNextMeetingNoLocationMessage => GetBotResponse();
          
        public static BotResponse ShowNextMeetingMessage => GetBotResponse();
          
        public static BotResponse ShowMultipleNextMeetingMessage => GetBotResponse();
                
        private static BotResponse GetBotResponse([CallerMemberName] string propertyName = null)
        {
            return _responseManager.GetBotResponse(propertyName);
        }
    }
}