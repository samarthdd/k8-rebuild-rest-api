using System;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildFromBase64Method
{
    [TestFixture]
    public class WhenBase64CannotBeDecoded : RebuildControllerTestBase
    {
        private IActionResult _result;

        [SetUp]
        public void OnetimeSetup()
        {
            CommonSetup();
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSource))]
        public void Bad_Request_Is_Returned(string base64)
        {
            _result = ClassInTest.RebuildFromBase64(new Base64Request
            {
                Base64 = base64
            });

            Assert.That(_result, Is.Not.Null);
            Assert.That(_result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSource))]
        public void Bad_Request_Contains_Message(string base64)
        {
            _result = ClassInTest.RebuildFromBase64(new Base64Request
            {
                Base64 = base64
            });

            var result = _result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<string>());
            Assert.That(result.Value, Is.EqualTo("Input file could not be decoded from base64."));
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSource))]
        public void Metrics_Are_Recorded(string base64)
        {
            ClassInTest.RebuildFromBase64(new Base64Request
            {
                Base64 = base64
            });

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.Base64DecodeTime),
                        It.Is<TimeSpan>(x => x > TimeSpan.Zero)),
                Times.Once);

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.FileSize),
                        It.Is<int>(x => x == 0)),
                Times.Once);

            MetricServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSource))]
        public void No_Engine_Actions_Are_Performed(string base64)
        {
            ClassInTest.RebuildFromBase64(new Base64Request
            {
                Base64 = base64
            });

            GlasswallVersionServiceMock.VerifyNoOtherCalls();
            FileTypeDetectorMock.VerifyNoOtherCalls();
            FileProtectorMock.VerifyNoOtherCalls();
        }

        private static string[] TestCaseSource()
        {
            return new[]
            {
                null,
                "",
                " ",
                "Some352_invalid_text_data"
            };
        }
    }
}