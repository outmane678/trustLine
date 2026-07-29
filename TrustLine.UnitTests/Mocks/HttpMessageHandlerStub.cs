using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TrustLine.Tests.Mocks
{
    public class HttpMessageHandlerStub : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public HttpMessageHandlerStub(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}