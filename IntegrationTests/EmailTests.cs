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
    public class EmailTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public EmailTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task TestRightEmailAsync()
        {
            // Arrange
            var request = new
            {
                Url = "/api/email/sendemail?format=json",
                Body = new
                {
                    Body="body",
                    Subject="subject",
                    Receivers=new List<string>() { "kaibadov@xalqbank.az", "kaibadov@xalqbank.az" }
                }
            };

            // Act
            
           HttpContent req = ContentHelper.GetStringContent(request.Body);
           var response = await Client.PostAsync(request.Url, req);
           var value = await response.Content.ReadAsStringAsync();
           response.EnsureSuccessStatusCode();
           
            var result = JsonConvert.DeserializeObject<SendEmailResponseDTO>(value);


            // Assert
            Assert.True(result.IsSuccess);
           
        }

        [Fact]
        public async Task TestWrongEmailAsync()
        {
            // Arrange
            var request = new
            {
                Url = "/api/email/sendemail?format=json",
                Body = new
                {
                    Body = "body",
                    Subject = "subject",
                    Receivers = new List<string>() { "kakibadov@xalqbank.az"}
                }
            };

            // Act

            HttpContent req = ContentHelper.GetStringContent(request.Body);
            var response = await Client.PostAsync(request.Url, req);
            var value = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            
            var result = JsonConvert.DeserializeObject<SendEmailResponseDTO>(value);

            // Assert
            Assert.True(!result.IsSuccess);

        }


    }
}
