using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCvSharp;
using System.Threading.Tasks;



[Description("")]
[WorkflowElementCategory(ElementCategory.Source)]
public class SourceScript : Source<OpenCV.Net.IplImage>
{
[Description("The URL of the RTSP stream.")]
    public string StreamUrl { get; set; }

    public override IObservable<OpenCV.Net.IplImage> Generate()
    {
        // Start a new task to read frames
        return Observable.Create<OpenCV.Net.IplImage>((observer, cancellationToken) =>
        {
            return Task.Factory.StartNew(() =>
            {
                // Initialize the VideoCapture object with the RTSP stream URL
                using (var capture = new VideoCapture(StreamUrl))
                {
                    if (!capture.IsOpened())
                    {
                        observer.OnError(new Exception("Failed to open RTSP stream."));
                        return;
                    }

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var frame = new Mat();
                        if (!capture.Read(frame) || frame.Empty())
                        {
                            observer.OnCompleted();
                            break;
                        }

                        var output = new MatWrapper(frame);
                        observer.OnNext(output);
                    }
                }
            },
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        });
    }

    class MatWrapper : OpenCV.Net.IplImage
    {
        readonly Mat owner;

        public MatWrapper(Mat frame)
            : base(new OpenCV.Net.Size(frame.Width, frame.Height),
                            OpenCV.Net.IplDepth.U8, 3,
                            frame.Data)
        {
            owner = frame;
        }
    }
}