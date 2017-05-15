using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IC6.EmotionAPI
{
    public class FaceRectangle
    {
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
    }

    public class Scores
    {
        public double Anger { get; set; }
        public double Contempt { get; set; }
        public double Disgust { get; set; }
        public double Fear { get; set; }
        public double Happiness { get; set; }
        public double Neutral { get; set; }
        public double Sadness { get; set; }
        public double Surprise { get; set; }
    }

    public class EmotionsResponse
    {
        public FaceRectangle FaceRectangle { get; set; }
        public Scores Scores { get; set; }
    }
}
