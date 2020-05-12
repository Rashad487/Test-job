using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using wsNotifierService;
using wsNotifierService.DataTransferObjects;
using Xunit;

namespace IntegrationTests
{
    public class SmsTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public SmsTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task TestRightSmsAsync()
        {
            // Arrange
            var request = new
            {
                Url = "/api/Sms/sendSms?format=json",
                Body = new
                {
                    ReceiverPhone = "514401122",
                    SmsText = "subject"
                }
            };

            // Act
            
           HttpContent req = ContentHelper.GetStringContent(request.Body);
           var response = await Client.PostAsync(request.Url, req);
           var value = await response.Content.ReadAsStringAsync();
           response.EnsureSuccessStatusCode();
           
            var result = JsonConvert.DeserializeObject<SendSmsResponseDTO>(value);


            // Assert
            Assert.True(result.IsSuccess);
           
        }

        [Fact]
        public async Task TestWrongSmsAsync()
        {
            // Arrange
            var request = new
            {
                Url = "/api/Sms/sendSms?format=json",
                Body = new
                {
                    ReceiverPhone = "xxxxxx",
                    SmsText = "subject"
                }
            };

            // Act

            HttpContent req = ContentHelper.GetStringContent(request.Body);
            var response = await Client.PostAsync(request.Url, req);
            var value = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            
            var result = JsonConvert.DeserializeObject<SendSmsResponseDTO>(value);

            // Assert
            Assert.True(!result.IsSuccess);

        }


    }
}
