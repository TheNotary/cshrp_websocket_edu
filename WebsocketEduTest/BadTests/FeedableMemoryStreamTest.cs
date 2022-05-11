using System;
using System.Text;
using System.Threading;
using Xunit;
using FluentAssertions;

namespace WebsocketEduTest
{
    public class FeedableMemoryStreamTest
    {
        [Fact]
        public void ItCanUseFeedableMemoryStreams()
        {
            // Given
            FeedableMemoryStream fms = new FeedableMemoryStream();
            StringBuilder sb = new StringBuilder();

            // When
            var t = new Thread(() =>
            {
                while (true)
                {
                    if (fms.Position < fms.Length)
                    {
                        int myByte = fms.ReadByte();
                        if (myByte == 255) break; // A 255 byte can signal the end of the stream
                        sb.Append(myByte.ToString());
                    }
                    Thread.Sleep(5);
                }
            }); t.Start();

            // And
            fms.PutByte(1);
            fms.PutByte(2);
            fms.PutByte(3);
            fms.PutByte(255);

            // Then
            t.Join();
            Assert.Equal("123", sb.ToString());
        }

        [Fact]
        public void ItDoesntOverwriteTheInitialStreamWhenPutBytesIsUsed()
        {
            // Given
            FeedableMemoryStream fms = new FeedableMemoryStream("Hello");

            // When
            fms.PutByte(1);
            fms.PutByte(2);
            fms.PutByte(3);

            string actual = "ABCDEFGHI";
            actual.Should().StartWith("AB").And.EndWith("HI").And.Contain("EF").And.HaveLength(9);

            // Then
            Byte[] actualBytes = new byte[8];
            fms.Read(actualBytes, 0, actualBytes.Length);
            Assert.Equal(new byte[] { 72, 101, 108, 108, 111, 1, 2, 3 }, actualBytes);
        }
    }
}
