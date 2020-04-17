﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Nexmo.Api.Voice;
using System.Web;
using System.Globalization;

namespace Nexmo.Api.Test.Unit
{
    public class VoiceClientTests : TestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateCall(bool passCreds)
        {
            var expectedUri = "https://api.nexmo.com/v1/calls/";
            var expectedResponse = @"{
              ""uuid"": ""63f61863-4a51-4f6b-86e1-46edebcf9356"",
              ""status"": ""started"",
              ""direction"": ""outbound"",
              ""conversation_uuid"": ""CON-f972836a-550f-45fa-956c-12a2ab5b7d22""
            }";
            var expectedRequesetContent = @"{""to"":[{""number"":""14155550100"",""dtmfAnswer"":""p*123#"",""type"":""phone""}],""from"":{""number"":""14155550100"",""dtmfAnswer"":""p*123#"",""type"":""phone""},""ncco"":[{""text"":""Hello World"",""action"":""talk""}],""answer_url"":[""https://example.com/answer""],""answer_method"":""GET"",""event_url"":[""https://example.com/event""],""event_method"":""POST"",""machine_detection"":""continue"",""length_timer"":1,""ringing_timer"":1}";

            Setup(expectedUri, expectedResponse, expectedRequesetContent);

            var request = new Voice.CallCommand
            {
                To = new[] 
                { 
                    new Voice.Nccos.Endpoints.PhoneEndpoint
                    {
                        Number="14155550100",
                        DtmfAnswer="p*123#"
                    }
                },
                From = new Voice.Nccos.Endpoints.PhoneEndpoint
                {
                    Number = "14155550100",
                    DtmfAnswer = "p*123#"
                },
                Ncco = new Voice.Nccos.Ncco(new Voice.Nccos.TalkAction { Text="Hello World"}),
                AnswerUrl = new [] { "https://example.com/answer" },
                AnswerMethod="GET",
                EventUrl= new[] { "https://example.com/event" },
                EventMethod="POST",
                MachineDetection="continue",
                LengthTimer=1,
                RingingTimer=1,
            };
            var creds = Request.Credentials.FromAppIdAndPrivateKey(AppId, PrivateKey);
            var client = new NexmoClient(creds);
            CallResponse response;
            if (passCreds)
            {
                response = client.VoiceClient.CreateCall(request, creds);
            }
            else
            {
                response = client.VoiceClient.CreateCall(request);
            }
            Assert.Equal("63f61863-4a51-4f6b-86e1-46edebcf9356", response.Uuid);
            Assert.Equal("CON-f972836a-550f-45fa-956c-12a2ab5b7d22", response.ConversationUuid);
            Assert.Equal("outbound", response.Direction);
            Assert.Equal("started", response.Status);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]        
        public void TestListCalls(bool passCreds, bool kitchenSink)
        {
            var expectedResponse = @"{
                  ""count"": 100,
                  ""page_size"": 10,
                  ""record_index"": 0,
                  ""_links"": {
                                ""self"": {
                                    ""href"": ""/calls?page_size=10&record_index=20&order=asc""
                                }
                            },
                  ""_embedded"": {
                                ""calls"": [
                                  {
                        ""_links"": {
                          ""self"": {
                            ""href"": ""/calls/63f61863-4a51-4f6b-86e1-46edebcf9356""
                          }
                        },
                        ""uuid"": ""63f61863-4a51-4f6b-86e1-46edebcf9356"",
                        ""conversation_uuid"": ""CON-f972836a-550f-45fa-956c-12a2ab5b7d22"",
                        ""to"": [
                          {
                            ""type"": ""phone"",
                            ""number"": ""447700900000""
                          }
                        ],
                        ""from"": [
                          {
                            ""type"": ""phone"",
                            ""number"": ""447700900001""
                          }
                        ],
                        ""status"": ""started"",
                        ""direction"": ""outbound"",
                        ""rate"": ""0.39"",
                        ""price"": ""23.40"",
                        ""duration"": ""60"",
                        ""start_time"": ""2020-01-01 12:00:00"",
                        ""end_time"": ""2020-01-01 12:00:00"",
                        ""network"": ""65512""
                      }
                    ]
                  }
                }";
            var expectedUri = $"{ApiUrl}/v1/calls?status=started&date_start={HttpUtility.UrlEncode("2016-11-14T07:45:14Z").ToUpper()}&date_end={HttpUtility.UrlEncode("2016-11-14T07:45:14Z").ToUpper()}&page_size=10&record_index=0&order=asc&conversation_uuid=CON-f972836a-550f-45fa-956c-12a2ab5b7d22&";
            Setup(expectedUri, expectedResponse);

            var creds = Request.Credentials.FromAppIdAndPrivateKey(AppId, PrivateKey);
            var client = new NexmoClient(creds);
            var filter = new CallSearchFilter
            {
                ConversationUuid = "CON-f972836a-550f-45fa-956c-12a2ab5b7d22",
                DateStart = DateTime.Parse("2016-11-14T07:45:14"),
                DateEnd = DateTime.Parse("2016-11-14T07:45:14"),
                PageSize = 10,
                RecordIndex = 0,
                Order = "asc",
                Status = "started"
            };
            Common.PageResponse<CallList> callList;
            if (passCreds)
            {
                callList = client.VoiceClient.GetCalls(filter, creds);
            }
            else
            {
                callList = client.VoiceClient.GetCalls(filter);
            }

            var callRecord = callList.Embedded.Calls[0];
            Assert.True(100 == callList.Count);
            Assert.True(10 == callList.PageSize);
            Assert.True(0 == callList.PageIndex);
            Assert.Equal("/calls?page_size=10&record_index=20&order=asc", callList.Links.Self.Href);
            Assert.Equal("/calls/63f61863-4a51-4f6b-86e1-46edebcf9356", callRecord.Links.Self.Href);
            Assert.Equal("63f61863-4a51-4f6b-86e1-46edebcf9356", callRecord.Uuid);
            Assert.Equal("CON-f972836a-550f-45fa-956c-12a2ab5b7d22", callRecord.ConversationUuid);
            Assert.Equal("447700900000", callRecord.To[0].Number);
            Assert.Equal("phone", callRecord.To[0].Type);
            Assert.Equal("phone", callRecord.From[0].Type);
            Assert.Equal("447700900001", callRecord.From[0].Number);
            Assert.Equal("started", callRecord.Status);
            Assert.Equal("outbound", callRecord.Direction);
            Assert.Equal("0.39", callRecord.Rate);
            Assert.Equal("23.40", callRecord.Price);
            Assert.Equal("60", callRecord.Duration);
            Assert.Equal(DateTime.ParseExact("2020-01-01T12:00:00.000Z", "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal |
                                       DateTimeStyles.AdjustToUniversal), (callRecord.StartTime));
            Assert.Equal(DateTime.ParseExact("2020-01-01T12:00:00.000Z", "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal |
                                       DateTimeStyles.AdjustToUniversal), (callRecord.EndTime));

        }
    }
}