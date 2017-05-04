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
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmartEditor
{
    
      internal sealed class TextAdornment1
    {
        //initialization
        private readonly IAdornmentLayer layer;
        private readonly IWpfTextView view;
        private readonly Brush brush;
        private readonly Pen pen;
        //Variables
        string answer = "";
        string sessionID = "";
        string visibleText = "";
        JArray[] items;
        List<Coordinates> coords = new List<Coordinates>();
        List<int> startX = new List<int>();
        List<int> endX = new List<int>();

        

        //JSON SERIALIZERS
        string FirstJsonString()
        {
            Lang1 lang2 = new Lang1();
            lang2.type = "GetSession";
            lang2.apiVersion = "1.0.0";
            lang2.application = "SmartEditor";
            lang2.applicationKey = "1db76251256adfe71263bcdf2312bfac";
            return JsonConvert.SerializeObject(lang2);
        }
        string SecondJsonString()
        {
            Lang2 lang;
            lang.type = "Ping";
            lang.apiVersion = "1.0.0";
            lang.sessionId = sessionID;
            lang.timestamp = 0;
            return JsonConvert.SerializeObject(lang);
        }
        string ThirdJsonString()
        {
            Lang3 lang;
            lang.type = "OpenDocument";
            lang.apiVersion = "1.0.0";
            lang.sessionId = sessionID;
            lang.path = "C:/test/test.txt";
            lang.visibleText = visibleText;
            lang.visibleTextStartPos = 0;
            lang.cursorPos = 0;

            return JsonConvert.SerializeObject(lang);


        }
        string FourthJsonString()
        {
            Lang4 lang;
            lang.type = "GetPatternSamples";
            lang.apiVersion = "1.0.0";
            lang.sessionId = sessionID;
            lang.document = "C:/test/test.txt";
            return JsonConvert.SerializeObject(lang);
        }
        
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
            //this.brush = new SolidColorBrush(Colors.LightGray);
            //this.brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, 1);
            this.pen.Freeze();
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (sessionID.Length == 0) {
                SendAnMessage(FirstJsonString());
                ParseID(answer);
                SendAnMessage(SecondJsonString());
            }
               
            GetText();
            SendAnMessage(ThirdJsonString());
            SendAnMessage(FourthJsonString());
            ParseItems(answer);
            Debug.WriteLine("This coordinates to put on start "+startX[0]+"and :" + endX[0]);
            for (int i = 0; i<startX.Count; i++)
            {

                foreach (ITextViewLine line in e.NewOrReformattedLines)
                {
                    this.CreateVisuals(line, startX[i], endX[i]+1);

                }
            }
            
        }


        private async void getSamplesCoordinates() {

           
            
        }

        private void ParseItems(string response)
        {
            Language parseLang = JsonConvert.DeserializeObject<Language>(response);
            Debug.WriteLine("!!!!*** " +parseLang.items.ToString());
            JArray ps = parseLang.items;
            
            foreach (JObject item in ps) {
                //coords.Add(new Coordinates { start = (int)item["from"], end = (int)item["to"] }
                startX.Add((int)item["from"]);
                endX.Add((int)item["to"]);


            }
            Debug.WriteLine("1111+++1111"+coords.ToString());
         

        }
        //DON'T TOUCH IT WORKS
        public bool SendAnMessage(string message)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:45001");
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = message;

                streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                answer = streamReader.ReadToEnd();
                return true;
            }


        }
        private void ParseID(string response)
        {
            Language parseLang = JsonConvert.DeserializeObject<Language>(response);
            sessionID = parseLang.sessionId;

        }
      
        private void GetText()
        {
            ITextSnapshot snapshot = this.view.TextSnapshot;
            visibleText = snapshot.GetText();

            
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
       
        }
}
