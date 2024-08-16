using LangtonsAnt;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using UI;
using Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Text.Json;
using UI.ImageRendering;

namespace LangtonsAntBlazorFluent.Components.Pages
{
    public partial class Home
    {
        // Game states and UI states
        private GameBuffer? buffer;
        private Timer? gameTimer;
        PlayUIMode playUiState;
        EditUIMode editUiState;

        // Default values
        string rule = "LRL";
        const int imagesPerSecond = 5;
        const int nGenerations = 10;
        const int generationsPerStep = 1;

        private string currentRule = string.Empty;
        private string generationN = "Ant Generation #0";

        // Buttons visibility for different UI states
        private bool btnPlayVisibility = true;
        private bool btnPauseVisibility = true;
        private bool btnEditRulesVisibility = true;
        private bool btnEditAntVisibility = true;
        private bool btnEditCellVisibility = true;

        // Buttons enabled/disabled
        private bool btnPlayEnabled = true;
        private bool btnPauseEnabled = true;
        private bool btnStopEnabled = true;
        private bool btnRulesEditEnabled = true;
        private bool btnAntsEditEnabled = true;
        private bool btnCellEditEnabled = true;
        private bool btnPrevEnabled = true;
        private bool btnNextEnabled = true;
        private bool btnSaveEnabled = true;
        private bool btnLoadEnabled = true;

        // Loading screen and canvas
        private ElementReference canvasElement;
        private bool isLoading = true;
        private string statusMessage = "Status: Ready";
        private List<ColoredSquare> coloredSquares = new List<ColoredSquare>();

        // File input for loading game state
        FluentInputFile? fileInput = default!;

        // Reference to the current component for JSInterop
        DotNetObjectReference<Home> homeReference => DotNetObjectReference.Create(this);

        private GameBuffer CreateGameBuffer(IGame? initialState = null, int nGenerations = 100, string initialRule = "LR")
        {
            if (initialState == null)
            {
                initialState = CreateGame(initialRule);
            }
            return new GameBuffer(initialState, nGenerations);
        }

        private IGame CreateGame(string initialRule = "LR")
        {
            IGame newGame = null;
            newGame = new Game(64, null);
            newGame.Ants = new List<IAnt>(new IAnt[] {
                            new GeneralizedAnt(
                                i: newGame.Size / 2 + 1,
                                j: newGame.Size / 2 + 1,
                                direction: 0) { Rule = initialRule }
                        });
            return newGame;
        }



        #region Properties

        PlayUIMode PlayUIState
        {
            get { return playUiState; }
            set
            {
                switch (value)
                {
                    case PlayUIMode.Playing:

                        // Starting the timer and updating the game state depending on the speed
                        gameTimer = new Timer(_ =>
                        {
                            if (buffer.MoveNext(generationsPerStep))
                                UpdateGameView(buffer.Current!, false);
                            else
                            {
                                DialogService.ShowInfo("Game Over. We no longer have any ants.");
                                PlayUIState = PlayUIMode.Stopped;
                            }
                            // Forcing the Generations field to be synchronous
                            InvokeAsync(StateHasChanged);
                        }, null, 0, 1000 / imagesPerSecond);

                        // Updating the UI
                        btnPlayVisibility = false;
                        btnPauseVisibility = true;

                        SetButtonsNonPlayMode(false);
                        break;
                    case PlayUIMode.Paused:
                        // Killing the timer
                        gameTimer?.Dispose();

                        // Updating the UI
                        btnPauseVisibility = false;
                        btnPlayVisibility = true;

                        SetButtonsNonPlayMode(true);
                        break;
                    case PlayUIMode.Stopped:
                        // Killing the timer
                        gameTimer?.Dispose();

                        // Creating a new game state 
                        buffer = CreateGameBuffer(null, nGenerations, rule);
                        UpdateGameView(buffer.Current!, false);

                        //Resetting the UI
                        generationN = "Ant Generation #0";
                        btnPlayVisibility = true;
                        btnPauseVisibility = false;

                        SetButtonsNonPlayMode(true);
                        break;
                    default:
                        break;
                }
                playUiState = value;
            }
        }

        /// <summary>
        /// This property switches UI between "Normal" or "Play" state and "Edit" states
        /// </summary>
        EditUIMode EditUIState
        {
            get { return editUiState; }
            set
            {
                switch (value)
                {
                    case EditUIMode.EditingColors:
                        PlayUIState = PlayUIMode.Paused;
                        btnEditCellVisibility = false;
                        SetColorsEditMode(true);
                        break;
                    case EditUIMode.EditingAnt:
                        PlayUIState = PlayUIMode.Paused;
                        btnEditAntVisibility = false;
                        SetAntEditMode(true);
                        break;
                    case EditUIMode.EditingRule:
                        PlayUIState = PlayUIMode.Paused;
                        btnEditRulesVisibility = false;
                        SetRuleEditMode(true);
                        break;
                    case EditUIMode.NotEditing:
                    default:
                        SetRuleEditMode(false);
                        SetAntEditMode(false);
                        SetColorsEditMode(false);

                        PlayUIState = PlayUIMode.Paused;
                        btnEditRulesVisibility = true;
                        btnEditAntVisibility = true;
                        btnEditCellVisibility = true;
                        if (editUiState != EditUIMode.NotEditing)
                        {
                            buffer.FlushBuffer();
                        }
                        break;
                }
                editUiState = value;
            }
        }

        /// <summary>
        /// Enable or disable buttons not related to editing
        /// </summary>
        /// <param name="value">Enable buttons</param>
        void SetButtonsNonEditMode(bool value)
        {
            btnPlayEnabled = value;
            btnPauseEnabled = value;
            btnStopEnabled = value;
            btnPrevEnabled = value;
            btnNextEnabled = value;
            btnSaveEnabled = value;
            btnLoadEnabled = value;
        }

        /// <summary>
        /// Enable or disable buttons not related to playing
        /// </summary>
        /// <param name="value">Enable buttons</param>
        void SetButtonsNonPlayMode(bool value)
        {
            btnRulesEditEnabled = value;
            btnAntsEditEnabled = value;
            btnCellEditEnabled = value;
            btnNextEnabled = value;
            btnPrevEnabled = value;
            btnSaveEnabled = value;
            btnLoadEnabled = value;
        }


        /// <summary>
        /// Switch UI into "rule edit" mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetRuleEditMode(bool value)
        {
            // Disable everything else
            SetButtonsNonEditMode(!value);

            if (value)
            {
                btnAntsEditEnabled = false;
                btnCellEditEnabled = false;
                
            }
            else
            {
                btnAntsEditEnabled = true;
                btnCellEditEnabled = true;
            }
        }

        /// <summary>
        /// Switch UI into Ant edit mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetAntEditMode(bool value)
        {

            // Disable everything else
            SetButtonsNonEditMode(!value);

            if (value)
            {
                btnRulesEditEnabled = false;
                btnCellEditEnabled = false;

            }
            else
            {
                btnRulesEditEnabled = true;
                btnCellEditEnabled = true;
            }
        }

        /// <summary>
        /// Switch UI into Cell Colors edit mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetColorsEditMode(bool value)
        {
            // Disable everything else
            SetButtonsNonEditMode(!value);

            if (value)
            {
                btnRulesEditEnabled = false;
                btnAntsEditEnabled = false;

            }
            else
            {
                btnRulesEditEnabled = true;
                btnAntsEditEnabled = true;
            }
        }

        /// <summary>
        /// Set current ants rule string updating UI in the process
        /// </summary>
        public string Rule
        {
            get
            {
                return rule;
            }
            set
            {
                if (buffer.Current == null)
                    throw new InvalidOperationException("Cannot set ants rule when current game state is null");

                if (!IsRuleValid(value))
                    throw new InvalidOperationException("The rule can only consist from L and R characters and be from 2 to 14 in length. Example: LLRR");

                foreach (Ant a in buffer.Current.Ants)
                {
                    ((GeneralizedAnt)a).Rule = value;
                }
                buffer.FlushBuffer();

                SetRuleText(value);
                rule = value;
            }
        }

        /// <summary>
        /// If all the ants have the same rule, return the rule string, else return null
        /// </summary>
        /// <param name="ants">GeneralizedAnt list</param>
        /// <returns></returns>
        private string? CalculateCommonRule(IList<IAnt> ants)
        {
            string? commonRule = null;
            foreach (IAnt a in ants)
            {
                string rule = ((GeneralizedAnt)a).Rule;
                if (commonRule == null)
                    commonRule = rule;
                else if (rule != commonRule)
                    return null;
            }
            return commonRule;
        }

        private static bool IsRuleValid(string proposedRule)
        {
            return Regex.IsMatch(proposedRule, "^[L|R]{2,14}$");
        }

        private void SetRuleText(string rule)
        {
            var coloredSquares = CreateColoredRuleSquares(rule);
            UpdateRuleTextUI(coloredSquares);
        }

        private List<ColoredSquare> CreateColoredRuleSquares(string rule)
        {
            var coloredSquares = new List<ColoredSquare>();

            int i = 0;

            foreach (char c in rule)
            {
                var colorBytes = ColorBytes.ColorSequence[i].ToArray().Reverse(); // Getting the color and converting from BGR to RGB using Reverse
                var color = BitConverter.ToString(colorBytes.ToArray()).Replace("-", string.Empty); // Convert byte[] to hex string
                coloredSquares.Add(new ColoredSquare { Color = color, Character = c });
                i++;
            }

            return coloredSquares;
        }

        private void UpdateRuleTextUI(List<ColoredSquare> coloredSquares)
        {
            this.coloredSquares = coloredSquares;
            StateHasChanged();
        }

        private class ColoredSquare
        {
            public string Color { get; set; }
            public char Character { get; set; }
        }


        public int MaxColor
        {
            get { return (rule?.Length ?? 0) - 1; }
        }

        #endregion

        #region Event Handlers

        private void btnPlay_Click()
        {
            PlayUIState = PlayUIMode.Playing;
        }

        private void btnPause_Click()
        {
            PlayUIState = PlayUIMode.Paused;
        }

        private void btnStop_Click()
        {
            PlayUIState = PlayUIMode.Stopped;
        }

        private async Task btnPrev_Click(MouseEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;

            if (!buffer.MovePrevious())
            {
                DialogService.ShowInfo($"Cannot move back. We only store a limited number of previous generations of the game.");
            }
            UpdateGameView(buffer.Current!, false);
        }
        private async Task btnNext_Click(MouseEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;

            if (!buffer.MoveNext())
            {
                DialogService.ShowInfo("Game Over. We no longer have any ants.");
            }
            UpdateGameView(buffer.Current!, false);
        }
        private async void btnEditAnt_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.EditingAnt;
            await JSRuntime.InvokeVoidAsync("LangtonsAnt.handleCanvasClick", canvasElement);
        }
        private async void btnCancelEditAnt_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.NotEditing;
            await JSRuntime.InvokeVoidAsync("LangtonsAnt.removeClickEventListener", canvasElement);
        }
        private async void btnEditCellColor_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.EditingColors;
            await JSRuntime.InvokeVoidAsync("LangtonsAnt.handleCanvasClick", canvasElement);
        }
        private async void btnCancelEditCellColor_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.NotEditing;
            await JSRuntime.InvokeVoidAsync("LangtonsAnt.removeClickEventListener", canvasElement);
        }

        private void btnEditRuleStart_Click(MouseEventArgs e) {
            EditUIState = EditUIMode.EditingRule;
        }
        private void btnSaveRuleStart_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.EditingRule;

            try
            {
                if (IsRuleValid(currentRule)) { 
                    Rule = currentRule;
                }
                else
                {
                    DialogService.ShowError("Rules can only have the R or the L characters!");
                    currentRule = Rule;
                    throw new InvalidOperationException("Invalid rule");
                }
            }
            catch (Exception ex)
            {
                statusMessage = ex.Message;
            }

            EditUIState = EditUIMode.NotEditing;

        }
        private void btnEditRuleCancel_Click(MouseEventArgs e)
        {
            currentRule = Rule;
            EditUIState = EditUIMode.NotEditing;
        }

        private async Task btnSave_Click(MouseEventArgs e)
        {
            try
            {
                var json = GameJSONSerializer.ToJson(buffer.Current!);

                try
                {
                    await JSRuntime.InvokeVoidAsync("LangtonsAnt.saveFileDialog", "gameState.json", json);
                }
                catch (TaskCanceledException)
                {
                    DialogService.ShowError("File save was canceled.");
                }
            }
            catch (Exception)
            {
                DialogService.ShowError("Buffer Error!");
            }
        }

        private void JsonUploaded(IEnumerable<FluentInputFileEventArgs> files)
        {
            var json = files.First();
            try
            {
                var jsonFileContent = File.ReadAllText(json.LocalFile?.FullName);
                var gameState = GameJSONSerializer.FromJson(jsonFileContent);
                buffer.currentNode.Value = gameState;

                string? commonRule = CalculateCommonRule(buffer.Current.Ants);

                if (commonRule != null)
                {
                    Rule = commonRule;
                }

                currentRule = Rule;
                UpdateGameView(buffer.Current!, false);
                EditUIState = EditUIMode.NotEditing;

            }
            catch (Exception ex)
            {
                DialogService.ShowError(ex.Message);
            }

            json.LocalFile?.Delete();
        }
        #endregion

        #region JS - UI

        [JSInvokable("HandleCanvasClick")]
        public void HandleCanvasClick(int x, int y)
        {
            if (EditUIState == EditUIMode.EditingAnt)
            {
                // Ants
                IAnt? ant = buffer.Current!.Ants.FirstOrDefault(a => (x == a.I) && (y == a.J));
                if (ant != null)
                {
                    if (ant.Direction == AntDirection.Left)
                    {
                        // Remove the ant
                        buffer.Current.Ants.Remove(ant);
                    }
                    else
                    {
                        // Turn the ant
                        ant.RotateCW();
                    }
                }
                else
                {
                    // Add ant
                    ant = new GeneralizedAnt(i: x, j: y, direction: AntDirection.Up) { Rule = this.Rule };
                    buffer.Current.Ants.Add(ant);

                }
            }
            else if (EditUIState == EditUIMode.EditingColors)
            {
                // Colors
                buffer.Current!.Field[x, y] = (byte)((buffer.Current!.Field[x, y] + 1) % (MaxColor + 1));
            }
            UpdateGameView(buffer.Current!, false);
        }

        private void UpdateGameView(IGame gameState, bool refreshPage)
        {
            Task task = GameImageRenderer.GetGenerationImageSourceX2(JSRuntime, canvasElement, gameState, refreshPage);

            generationN = $"Ant Generation #{gameState.GenerationN}";

            if (isLoading)
            {
                // After finishing the loading screen, we need to delete the loading screen
                task.ContinueWith(async task =>
                {
                    isLoading = false;
                    await InvokeAsync(StateHasChanged);

                });
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("LangtonsAnt.initializeCanvas", canvasElement);
                await JSRuntime.InvokeVoidAsync("LangtonsAnt.ReferenceCache", homeReference);

                buffer = CreateGameBuffer(null, nGenerations, rule);
                currentRule = rule;
                Rule = rule;
                PlayUIState = PlayUIMode.Stopped;

            }
            else{
                // Need to think to trigger only in refresh
                UpdateGameView(buffer.Current!, false);

            }
        }

        #endregion
    }
}
