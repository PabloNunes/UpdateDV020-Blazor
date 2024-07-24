using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using UI.ImageRendering;


namespace LangtonsAnt
{
    class GameImageRenderer
    {
        //public static BitmapSource GetGenerationImageSourceX2(IGame g)
        public static object GetGenerationImageSourceX2(IJSRuntime js, ElementReference canvas, IGame g)
        {
            // Define parameters used to create the BitmapSource.
            //PixelFormat pf = PixelFormats.Bgr24;
            int width = g.Size * 2;
            int height = g.Size * 2;
            int rawStride = 8; //(width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            // Initialize the image with data.
            //byte[] pxl_color;
            var pxl_color = "#000000";

            //clean canvas
            CleanCanvas(js, canvas);

            for (int i = 0; i < g.Size; i++)
            {
                for (int j = 0; j < g.Size; j++)
                {
                    // Any ants there?
                    IAnt? ant = g.Ants.FirstOrDefault(a => (i == a.I) && (j == a.J));
                    if (ant != null)
                    {
                        // Draw one of the ants
                        var headColor = ToHex(Color.Red);
                        var tailColor = ToHex(Color.Blue);
                        switch (ant.Direction)
                        {
                            case AntDirection.Up:
                                Draw4in2XScaled(js, canvas, i, j, headColor, tailColor, headColor, tailColor);
                                break;
                            case AntDirection.Down:
                                Draw4in2XScaled(js, canvas, i, j, tailColor, headColor, tailColor, headColor);
                                break;
                            case AntDirection.Left:
                                Draw4in2XScaled(js, canvas, i, j, tailColor, tailColor, headColor, headColor);
                                break;
                            case AntDirection.Right:
                                Draw4in2XScaled(js, canvas, i, j, headColor, headColor, tailColor, tailColor);
                                break;
                        }
                    }
                    else
                    {
                        //TODO: Need to implement this!!
                        var tempColor = ColorBytes.ColorSequence[g.Field[i, j]];
                        pxl_color = ToHex(tempColor);
                        DrawAnhillCell(js, canvas, i, j, pxl_color);
                    }

                }
            }
            //BitmapSource bitmap = BitmapSource.Create(width, height,
            //    96, 96, pf, null,
            //    rawImage, rawStride);
            //return bitmap;
            return null;
        }

        private static async Task Draw4in2XScaled(IJSRuntime js, ElementReference canvas, int i, int j,
            string colorUpperLeft, string colorLowerLeft, string colorUpperRight, string colorLowerRight){

            var w = 0.5;
            var h = 0.5;

            //TODO: could be optimize by drawing to rectangles instead of 4 squares

            //top left
            await js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i, j, w, h, colorUpperLeft);
            //top right
            await js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i+w, j, w, h, colorUpperRight);
            //bottom left
            await js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i, j+h, w, h, colorLowerLeft);
            //bottom right
            await js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i+w, j+h, w, h, colorLowerRight);
        }

        private static async Task DrawAnhillCell(IJSRuntime js, ElementReference canvas, int i, int j, string color)
        {
            var w = 1;
            var h = 1;

            await js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i, j, w, h, color);
        }

        private static async Task CleanCanvas(IJSRuntime js, ElementReference canvas) {
            await js.InvokeVoidAsync("LangtonsAnt.clearCanvas", canvas);
        }

        private static String ToHex(System.Drawing.Color c)
        {
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private static String ToHex(byte[] ca)
        {
            return $"#{ca[2]:X2}{ca[1]:X2}{ca[0]:X2}";
        }
    }
}
