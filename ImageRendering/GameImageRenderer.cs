using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using UI.ImageRendering;

namespace LangtonsAnt
{
    class GameImageRenderer
    {
        private static IGame? previousGameState;

        public static async Task GetGenerationImageSourceX2(IJSRuntime js, ElementReference canvas, IGame g)
        {
            int width = g.Size * 2;
            int height = g.Size * 2;
            var tasks = new List<Task>();

            // Not clear canvas if it is the first time
            if (previousGameState == null)
            {
                await CleanCanvas(js, canvas);
            }

            for (int i = 0; i < g.Size; i++)
            {
                for (int j = 0; j < g.Size; j++)
                {
                    // Checking if the pixel needs to be redrawn
                    // The pixel needs to be redrawn if it is the first time or if the pixel has changed, comparing the current game state with the previous one
                    bool needsRedraw = previousGameState == null || previousGameState.Field[i, j] != g.Field[i, j] ||
                                       previousGameState.Ants.Any(a => a.I == i && a.J == j) != g.Ants.Any(a => a.I == i && a.J == j);
                    
                    // Redraw the pixel if needed
                    if (needsRedraw)
                    {
                        IAnt? ant = g.Ants.FirstOrDefault(a => (i == a.I) && (j == a.J));
                        if (ant != null)
                        {
                            var headColor = ToHex(Color.Red);
                            var tailColor = ToHex(Color.Blue);
                            switch (ant.Direction)
                            {
                                case AntDirection.Up:
                                    tasks.Add(Draw4in2XScaled(js, canvas, i, j, headColor, tailColor, headColor, tailColor));
                                    break;
                                case AntDirection.Down:
                                    tasks.Add(Draw4in2XScaled(js, canvas, i, j, tailColor, headColor, tailColor, headColor));
                                    break;
                                case AntDirection.Left:
                                    tasks.Add(Draw4in2XScaled(js, canvas, i, j, tailColor, tailColor, headColor, headColor));
                                    break;
                                case AntDirection.Right:
                                    tasks.Add(Draw4in2XScaled(js, canvas, i, j, headColor, headColor, tailColor, tailColor));
                                    break;
                            }
                        }
                        else
                        {
                            var tempColor = ColorBytes.ColorSequence[g.Field[i, j]];
                            var pxl_color = ToHex(tempColor);
                            tasks.Add(DrawAnhillCell(js, canvas, i, j, pxl_color));
                        }
                    }
                }
            }

            await Task.WhenAll(tasks);

            // Update the previous game state
            previousGameState = g.Clone();
        }

        private static async Task Draw4in2XScaled(IJSRuntime js, ElementReference canvas, int i, int j,
            string colorUpperLeft, string colorLowerLeft, string colorUpperRight, string colorLowerRight)
        {
            var w = 0.5;
            var h = 0.5;

            var tasks = new List<Task>
            {
                js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i, j, w, h, colorUpperLeft).AsTask(),
                js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i + w, j, w, h, colorUpperRight).AsTask(),
                js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i, j + h, w, h, colorLowerLeft).AsTask(),
                js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i + w, j + h, w, h, colorLowerRight).AsTask()
            };

            await Task.WhenAll(tasks);
        }

        private static async Task DrawAnhillCell(IJSRuntime js, ElementReference canvas, int i, int j, string color)
        {
            var w = 1;
            var h = 1;

            await js.InvokeVoidAsync("LangtonsAnt.drawPixel", canvas, i, j, w, h, color);
        }

        private static async Task CleanCanvas(IJSRuntime js, ElementReference canvas)
        {
            await js.InvokeVoidAsync("LangtonsAnt.clearCanvas", canvas);
        }

        private static string ToHex(Color c)
        {
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private static string ToHex(byte[] ca)
        {
            return $"#{ca[2]:X2}{ca[1]:X2}{ca[0]:X2}";
        }
    }
}
