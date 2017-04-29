//------------------------------------------------------------------------------
// <copyright file="TextAdornment1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;

namespace SmartEditor
{
      internal sealed class TextAdornment1
    {
        //initialization
        private readonly IAdornmentLayer layer;
        private readonly IWpfTextView view;
        private readonly Brush brush;
        private readonly Pen pen;

        private string answer = "";
        private string appKey = "";
        private string sessionID = "";
        private string type = "";
        
        //This was a standart addon from MS Library changed.
        public TextAdornment1(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            this.layer = view.GetAdornmentLayer("TextAdornment1");

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;

            // Create the pen and brush to color the box behind the a's
            this.brush = new SolidColorBrush(Colors.LightGray);
            this.brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, 1);
            this.pen.Freeze();
        }
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            try
            {
                SendAnMessage("");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            
            try
            {
                parseAnswer(answer);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //throw;
            }

            Debug.WriteLine(sessionID+ " and " + appKey);
            
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line,0,129);
            }
            
        }

        private void parseAnswer(string s)
        {
            
        }

        private void CreateVisuals(ITextViewLine line, int startPosition, int stopPosition )
        {

            
            //CreateBoxWithCoordinates(startPosition, startPosition);
            IWpfTextViewLineCollection textViewLines = this.view.TextViewLines;
            SnapshotSpan span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(startPosition, stopPosition));
            Geometry geometry = textViewLines.GetMarkerGeometry(span);
            
            if (geometry != null)
            {
                var drawing = new GeometryDrawing(this.brush, this.pen, geometry);
                drawing.Freeze();

                var drawingImage = new DrawingImage(drawing);
                drawingImage.Freeze();

                var image = new Image
                {
                    Source = drawingImage,
                };

                // Align the image with the top of the bounds of the text geometry
                Canvas.SetLeft(image, geometry.Bounds.Left);
                Canvas.SetTop(image, geometry.Bounds.Top);

                this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);

            }

        }
        //MARK: This is sending Message to service
        public bool SendAnMessage(string message)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:45001");
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"type\":\"GetSession\",\"apiVersion\": \"1.0.0\", \"application\": \"SmartEditor\", \"applicationKey\": \"1db76251256adfe71263bcdf2312bfac\"}";


            streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                answer = streamReader.ReadToEnd();
                if (answer.Length != 0)
                {
                    System.Diagnostics.Debug.WriteLine("Answer got");
                    Debug.WriteLine(answer);
                }
                return true;
            }
            

        }
        
        

    }
}
