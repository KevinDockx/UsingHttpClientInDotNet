using Moq;
using Moq.Protected;
using Movies.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Movies.UnitTests
{
    public class TestableClassWithApiAccessUnitTests
    {
        [Fact]
        public async void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException()
        {
            var httpClient = new HttpClient(new Return401UnauthorizedResponseHandler())
            {
                BaseAddress = new Uri("http://localhost:57863")
            };

            var testableClass = new TestableClassWithApiAccess(httpClient);

            await Assert.ThrowsAsync<UnauthorizedApiAccessException>(
                  () => testableClass.GetMovie(CancellationToken.None));
        }

        [Fact]
        public async void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException_WithMoq()
        {
            var unauthorizedResponseHttpMessageHandlerMock = new Mock<HttpMessageHandler>();
            unauthorizedResponseHttpMessageHandlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
              .ReturnsAsync(new HttpResponseMessage()
              {
                  StatusCode = HttpStatusCode.Unauthorized
              });

            var httpClient = new HttpClient(unauthorizedResponseHttpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:57863")
            };

            var testableClass = new TestableClassWithApiAccess(httpClient);

            await Assert.ThrowsAsync<UnauthorizedApiAccessException>(
                () => testableClass.GetMovie(CancellationToken.None));
        }

    }
}
