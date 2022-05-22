using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
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
            frame.opcode = 0x01;
            frame.isMasked = false;
            frame.cleartextPayload = Encoding.UTF8.GetBytes("Hello");
            frame.payloadLength = (ulong)frame.cleartextPayload.Length;

            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte[] expectedBytes = new byte[] { 0x81, 0x05, 0x48, 0x65, 0x6C, 0x6C, 0x6F };

            // when
            byte[] bytes = serializer.ToBytes();

            // then
            bytes.Should().Equal(expectedBytes);
        }

        [Fact]
        public void ItCanSerializeFirstHeaderByte()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            frame.fin = true;
            frame.opcode = 0x01;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte expectedByte = 129;

            // when
            byte firstByte = serializer.SerializeFirstHeaderByte();

            // then
            firstByte.Should().Be(expectedByte);
        }

        [Fact]
        public void ItCanSerializeSecondMaskedHeaderByte()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            frame.isMasked = true;
            frame.payloadLength = 5;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte expectedByte = 133;

            // when
            byte secondByte = serializer.SerializeSecondHeaderByte();

            // then
            secondByte.Should().Be(expectedByte);
        }

        [Fact]
        public void ItCanSerializeSecondUnMaskedHeaderByte()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            frame.isMasked = false;
            frame.payloadLength = 5;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte expectedByte = 5;

            // when
            byte secondByte = serializer.SerializeSecondHeaderByte();

            // then
            secondByte.Should().Be(expectedByte);
        }

        [Fact]
        public void ItCanSerializePayloadLengthWhenSmall()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            MemoryStream m = new MemoryStream();
            int stringSize = 10;
            frame.cleartextPayload = Encoding.UTF8.GetBytes(String.Empty.PadLeft(stringSize, 'h'));
            frame.isMasked = false;
            frame.payloadLength = (ulong)frame.cleartextPayload.Length;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);

            // when
            byte[] payloadBytes = serializer.SerializeExtendedPayloadLengthBytes();

            // then
            payloadBytes.Length.Should().Be(0);
        }

        [Fact]
        public void ItCanSerializePayloadLengthWhenMedium()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            MemoryStream m = new MemoryStream();
            int stringSize = 126;
            frame.cleartextPayload = Encoding.UTF8.GetBytes(String.Empty.PadLeft(stringSize, 'h'));
            frame.isMasked = false;
            frame.payloadLength = (ulong)frame.cleartextPayload.Length;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte[] expectedBytes = new byte[] { 0, 126 };

            // when
            byte[] payloadBytes = serializer.SerializeExtendedPayloadLengthBytes();

            // then
            payloadBytes.Should().Equal(expectedBytes);
        }

        [Fact]
        public void ItCanSerializePayloadLengthWhenLarge()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            MemoryStream m = new MemoryStream();
            int stringSize = 65536;
            frame.cleartextPayload = Encoding.UTF8.GetBytes(String.Empty.PadLeft(stringSize, 'h'));
            frame.isMasked = false;
            frame.payloadLength = (ulong)frame.cleartextPayload.Length;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte[] expectedBytes = new byte[] { 0, 0, 0, 0,
                                                0, 1, 0, 0 };

            // when
            byte[] payloadBytes = serializer.SerializeExtendedPayloadLengthBytes();

            // then
            payloadBytes.Should().Equal(expectedBytes);
        }

        // This is a good test but requires me to fix a bug where dotnet can't build strings big enough to fit in a websocketframe
        // So I really need to add a feature for splitting frames up for large messages... that's a won't fix
        //[Fact]
        //public void ItCanSerializePayloadLengthOfMaxSize()
        //{
        //    // given
        //    WebsocketFrame frame = new WebsocketFrame();
        //    MemoryStream m = new MemoryStream();
        //    int stringSize = 1000000000; // 2 147 483 647  // sadly... this number is the maximum string size which is less than the max size of a websocket payload (2^63 or 9223372036854775808)
        //    frame.cleartextPayload = Encoding.UTF8.GetBytes(String.Empty.PadLeft(stringSize, 'h'));
        //    frame.isMasked = false;
        //    frame.payloadLength = (ulong)frame.cleartextPayload.Length;
        //    WebsocketSerializer serializer = new WebsocketSerializer(frame);
        //    byte[] expectedBytes = new byte[] { 0, 0, 0, 0,
        //                                        255, 255, 255, 255 };

        //    // when
        //    byte[] payloadBytes = serializer.SerializeExtendedPayloadLengthBytes();

        //    // then
        //    payloadBytes.Should().Equal(expectedBytes);
        //}

        [Fact]
        public void ItCanGenerateMaskingKey()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            MemoryStream m = new MemoryStream();
            frame.isMasked = true;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);

            // when
            byte[] maskingKey = serializer.GenerateMaskingKey();

            // then
            maskingKey.Length.Should().Be(4);
        }

        [Fact]
        public void ItCanSerializeUnmaskedPayloadData()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            MemoryStream m = new MemoryStream();
            frame.cleartextPayload = Encoding.UTF8.GetBytes("hello");
            frame.isMasked = false;
            frame.payloadLength = (ulong)frame.cleartextPayload.Length;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte[] expectedBytes = new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F };

            // when
            byte[] payloadBytes = serializer.SerializePayloadData();

            // then
            payloadBytes.Should().Equal(expectedBytes);
        }

        [Fact]
        public void ItCanSerializeMaskedPayloadData()
        {
            // given
            WebsocketFrame frame = new WebsocketFrame();
            MemoryStream m = new MemoryStream();
            frame.cleartextPayload = Encoding.UTF8.GetBytes("aaaaa");
            frame.isMasked = true;
            frame.mask = new byte[] { 0b0001, 0b0010, 0b0100, 0b1000 };
            frame.payloadLength = (ulong)frame.cleartextPayload.Length;
            WebsocketSerializer serializer = new WebsocketSerializer(frame);
            byte[] expectedBytes = new byte[] { 0x60, 0x63, 0x65, 0x69, 0x60 };

            // when
            byte[] payloadBytes = serializer.SerializePayloadData();

            // then
            payloadBytes.Should().Equal(expectedBytes);
        }
    }
}
