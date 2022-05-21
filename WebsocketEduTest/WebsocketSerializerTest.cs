using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu;
using Xunit;

namespace WebsocketEduTest
{
    public class WebsocketSerializerTest : BaseTest
    {
        [Fact]
        public void ItCanSerializeWebsocketsIntoBytes()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            frame.fin = true;

            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte[] expectedBytes = new byte[] { 1, 2, 3 };

            // when
            byte[] bytes = serializer.ToBytes();

            // then
            bytes.Should().Equal(expectedBytes);
        }
        
        [Fact]
        public void ItCanSerializeFirstHeaderByte()
        {

        }

        [Fact]
        public void ItCanSerializeSecondHeaderByte()
        {

        }

        [Fact]
        public void ItCanSerializePayloadLengthWhenSmall()
        {

        }

        [Fact]
        public void ItCanSerializePayloadLengthWhenMedium()
        {

        }

        [Fact]
        public void ItCanSerializePayloadLengthWhenLarge()
        {

        }

        [Fact]
        public void ItCanGenerateMaskingKey()
        {
        }

        [Fact]
        public void ItCanSerializePayloadData()
        {
        }
    }
}
