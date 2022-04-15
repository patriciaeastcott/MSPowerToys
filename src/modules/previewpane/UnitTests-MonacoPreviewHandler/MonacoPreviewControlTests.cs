// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.PowerToys.PreviewHandler.Monaco;
using Microsoft.PowerToys.STATestExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.WinForms;
using Moq;

namespace MonacoPreviewHandlerUnitTests
{
    [STATestClass]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "new Exception() is fine in test projects.")]
    public class MonacoPreviewControlTests
    {
        private static readonly int ThreeSecondsInMilliseconds = 3000;
        private static readonly int SleepTimeInMilliseconds = 200;

        [TestMethod]
        public void MonacoPreviewControlShouldAddExtendedBrowserControlWhenDoPreviewCalled()
        {
            // Arrange
            using (var monacoPreviewControl = new MonacoPreviewHandlerControl())
            {
                // Act
                monacoPreviewControl.DoPreview<string>("HelperFiles/test.json");

                int beforeTick = Environment.TickCount;

                while (monacoPreviewControl.Controls.Count == 0 && Environment.TickCount < beforeTick + ThreeSecondsInMilliseconds)
                {
                    Application.DoEvents();
                    Thread.Sleep(SleepTimeInMilliseconds);
                }

                if (monacoPreviewControl.Controls[0] is Label)
                {
                    Trace.WriteLine(monacoPreviewControl.Controls[0].Text);
                }

                string loadingText = Microsoft.PowerToys.PreviewHandler.Monaco.Properties.Resources.Loading_Screen_Message;

                while (monacoPreviewControl.Controls[0] is Label)
                {
                    if (monacoPreviewControl.Controls[0].Text != loadingText)
                    {
                        Assert.Fail("Hit exception");
                    }
                }

                // Assert
                Assert.AreEqual(1, monacoPreviewControl.Controls.Count);
                Assert.IsInstanceOfType(monacoPreviewControl.Controls[0], typeof(WebView2));
            }
        }

        [TestMethod]
        public void MonacoPreviewControlShouldFillDockForWebView2WhenDoPreviewCalled()
        {
            // Arrange
            using (var monacoPreviewControl = new MonacoPreviewHandlerControl())
            {
                // Act
                monacoPreviewControl.DoPreview<string>("HelperFiles/test.json");

                int beforeTick = Environment.TickCount;

                while (monacoPreviewControl.Controls.Count == 0 && Environment.TickCount < beforeTick + ThreeSecondsInMilliseconds)
                {
                    Application.DoEvents();
                    Thread.Sleep(SleepTimeInMilliseconds);
                }

                // Assert
                Assert.AreEqual(DockStyle.Fill, ((WebView2)monacoPreviewControl.Controls[0]).Dock);
            }
        }

        private static IStream GetMockStream(string streamData)
        {
            var mockStream = new Mock<IStream>();
            var streamBytes = Encoding.UTF8.GetBytes(streamData);

            var streamMock = new Mock<IStream>();
            var firstCall = true;
            streamMock
                .Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IntPtr>()))
                .Callback<byte[], int, IntPtr>((buffer, countToRead, bytesReadPtr) =>
                {
                    if (firstCall)
                    {
                        Array.Copy(streamBytes, 0, buffer, 0, streamBytes.Length);
                        Marshal.WriteInt32(bytesReadPtr, streamBytes.Length);
                        firstCall = false;
                    }
                    else
                    {
                        Marshal.WriteInt32(bytesReadPtr, 0);
                    }
                });

            return streamMock.Object;
        }
    }
}
