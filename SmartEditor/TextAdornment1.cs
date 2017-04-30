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
        //ProtocolDescription
        string answer = "";
        private string firstMessage = "{\"type\":\"GetSession\",\"apiVersion\": \"1.0.0\", \"application\": \"SmartEditor\", \"applicationKey\": \"1db76251256adfe71263bcdf2312bfac\"}";
        private string sessionID = "";
        private string type = "";
        private string apiVersion = "";
        private string visibleText = "";

        public struct Language
        {
            public string sessionId;
            public string type;
            public string apiVersion;
            public string path;
            public string visibleText;
            public int visibleTextStartPos;
            public int cursorPos;
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
            this.brush = new SolidColorBrush(Colors.LightGray);
            this.brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, 1);
            this.pen.Freeze();
        }
        string MakeJsonString() {
            Language lang;
            lang.type = "OpenDocument";
            lang.apiVersion = "1.0.0";
            lang.sessionId = sessionID;
            lang.path = "C:/Test/test.txt";
            lang.visibleText = visibleText;
            lang.visibleTextStartPos = 0;
            lang.cursorPos = 0;


            return JsonConvert.SerializeObject(lang);

        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
                     
            try
                {
                    SendAnMessage(firstMessage);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            
            

            try
            {
                ParseAnswer(answer);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            String secondMessage = "{\"type\":\"Ping\",\"apiVersion\": \"1.0.0\",\"sessionId:\""+sessionID+",\"timestamp\":0}";
                //"{\"type\":\"Ping\",\"apiVersion\": \"1.0.0\","+sessionID+",\"timestamp\":0}";
            //Second Connect to Server
            try
            {
                SendAnMessage(secondMessage);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("***Second Send Error" + exception);
                throw;
            }
            try
            {
                ParseAnswer(answer);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("***Second Answer Error"+exception);
                throw;
            }
            GetText();

            //String thirdMessage = "{\"type\":\"OpenDocument\", \"apiVersion\": \"1.0.0\", "+sessionID+", \"path\":\"C:/Test/test.txt\", \"visibleText\":\""+visibleText+"\", \"visibleTextStartPos\":0,\"cursorPos\":0}";
            
            //Third Connect to Server Вот здесь какая то пизда получается

            try
            {
                SendAnMessage(MakeJsonString());
            }
            catch (Exception exception)
            {
                Debug.WriteLine("***Third Send Error" + exception);
                throw;
            }
            try
            {
                //ParseAnswer(answer);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("***Third Answer Error" + exception);
                throw;
            }






            //
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                //this.CreateVisuals(line,0,129);
            }
            
        }
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
                if (answer.Length != 0)
                {

                    Debug.WriteLine("Got answer" + answer);
                    

                }
                
                return true;
            }

            
        }

        //Method to parse answers
        private void ParseAnswer(string response)
        {
            


            JObject parJObject = JObject.Parse(response);
            //Trying to parse JOBJECT
            try
            {
                sessionID = parJObject.Property("sessionId").ToString().Substring(14).Trim(1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                
            }
            try
            {
                type = parJObject.Property("type").ToString();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            try
            {
                apiVersion = parJObject.Property("apiVersion").ToString();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            Debug.WriteLine("********"+sessionID+ "********");
            Debug.WriteLine("********" + type + "********");
        }

        private void GetText()
        {
            ITextSnapshot snapshot = this.view.TextSnapshot;
            visibleText = snapshot.GetText();

            Debug.WriteLine("!!!!!!!!!!!!!!!!"+visibleText);
        }

        //DON'T TOUCH
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
