using FluentAssertions;
using WebsocketEdu;
using Xunit;

namespace WebsocketEduTest
{
    public class SimpleWebsocketServerTest : BaseTest
    {
        [Fact]
        public void ItCanSerializeWebsocketsIntoBytes()
        {
            // when
            string actualPassword = SimpleWebsocketServer.GenerateRandomPassword();

            // then
            actualPassword.Length.Should().Be(10);
        }

    }
}
