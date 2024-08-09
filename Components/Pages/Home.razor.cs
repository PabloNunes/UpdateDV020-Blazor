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

namespace LangtonsAntBlazorFluent.Components.Pages
{
    public partial class Home
    {
        private GameBuffer? buffer;
        private Timer? gameTimer;
        PlayUIMode playUiState;
        EditUIMode editUiState;

        string rule = "LRL";
        const int imagesPerSecond = 5;
        const int nGenerations = 10;
        const int generationsPerStep = 1;

        private string currentRule = string.Empty;
        private string generationN = "Ant Generation #0";
        private bool btnPlayVisibility = true;
        private bool btnPauseVisibility = true;

        private bool btnEditing = false;
        private bool btnEditRulesVisibility = true;
        private bool btnEditAntVisibility = true;
        private bool btnEditCellVisibility = true;

        private ElementReference canvasElement;
        private bool isClickEventEnabled = false;
        private bool isLoading = true;
        private string statusMessage = "Status: Ready";

        FluentInputFile? fileInput = default!;
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
            //newGame = new Game(128, null);
            newGame = new Game(32, null);
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
                        }, null, 0, 1000 / imagesPerSecond);

                        // Updating the UI
                        btnPlayVisibility = false;
                        btnPauseVisibility = true;
                        break;
                    case PlayUIMode.Paused:
                        // Killing the timer
                        gameTimer?.Dispose();

                        // Updating the UI
                        btnPauseVisibility = false;
                        btnPlayVisibility = true;
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
                        // SetColorsEditMode(true);
                        break;
                    case EditUIMode.EditingAnt:
                        PlayUIState = PlayUIMode.Paused;
                        btnEditAntVisibility = false;
                        // SetAntEditMode(true);
                        break;
                    case EditUIMode.EditingRule:
                        PlayUIState = PlayUIMode.Paused;
                        btnEditRulesVisibility = false;
                        // SetRuleEditMode(true);
                        break;
                    case EditUIMode.NotEditing:
                        PlayUIState = PlayUIMode.Paused;
                        btnEditRulesVisibility = true;
                        btnEditAntVisibility = true;
                        btnEditCellVisibility = true;
                        if (editUiState != EditUIMode.NotEditing)
                        {
                            buffer.FlushBuffer();
                        }
                        break;
                    default:
                        // SetRuleEditMode(false);
                        // SetAntEditMode(false);
                        // SetColorsEditMode(false);

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
        /// Switch UI into "rule edit" mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetRuleEditMode(bool value)
        {
            // Disable everything else
            SetButtonsNonEditMode(!value);

            if (value)
            {
                // pnlRuleText.Visibility = false;
                // btnEditRuleStart.Visibility = false;
                // txtEditRule.Visibility = true;
                // btnEditRuleApply.Visibility = true;
                // btnEditRuleCancel.Visibility = true;
            }
            else
            {
                // pnlRuleText.Visibility = true;
                // btnEditRuleStart.Visibility = true;
                // txtEditRule.Visibility = false;
                // btnEditRuleApply.Visibility = false;
                // btnEditRuleCancel.Visibility = false;
            }
        }

        /// <summary>
        /// Enable or disable buttons not related to editing
        /// </summary>
        /// <param name="value">Enable buttons</param>
        void SetButtonsNonEditMode(bool value)
        {
            // Play - Stop - Pause
            // btnPlay.IsEnabled = value;
            // btnStop.IsEnabled = value;
            // btnPause.IsEnabled = value;

            // Previous and next buttons
            // btnStepBackward.IsEnabled = value;
            // btnStepForward.IsEnabled = value;

            // Edit ants and colors
            // btnEditAnt.IsEnabled = value;
            // btnEditCellColor.IsEnabled = value;
            // btnEditRuleStart.IsEnabled = value;

            // Save and load
            // btnSave.IsEnabled = value;
            // btnLoad.IsEnabled = value;
        }

        /// <summary>
        /// Switch UI into Ant edit mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetAntEditMode(bool value)
        {
            SetButtonsAntOrColorsEditMode(value);
            // lblEditingAnts.Visibility = value ? true : false;
        }

        /// <summary>
        /// Switch UI into Cell Colors edit mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetColorsEditMode(bool value)
        {
            SetButtonsAntOrColorsEditMode(value);
            // lblEditingColors.Visibility = value ? true : false;
        }

        /// <summary>
        /// Switch UI into Ant or cell colors edit mode
        /// </summary>
        /// <param name="value">is on</param>
        void SetButtonsAntOrColorsEditMode(bool value)
        {
            SetButtonsNonEditMode(!value);

            if (value)
            {
                // btnEditAnt.Visibility = false;
                // btnEditCellColor.Visibility = false;
                // btnBackToGame.Visibility = true;
            }
            else
            {
                // btnEditAnt.Visibility = true;
                // btnEditCellColor.Visibility = true;
                // btnBackToGame.Visibility = false;
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

        private bool IsRuleValid(string proposedRule)
        {
            return Regex.IsMatch(proposedRule, "^[L|R]{2,14}$");
        }

        private void SetRuleText(string rule)
        {
            // List<TextBlock> tbs = CreateColoredRuleControls(rule);
            // pnlRuleText.Children.Clear();
            // foreach (TextBlock tb in tbs)
            // {
            //     pnlRuleText.Children.Add(tb);
            // }
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
                Rule = currentRule;
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
                await JSRuntime.InvokeVoidAsync("LangtonsAnt.saveFile", "gameState.json", json);
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
                UpdateGameView(buffer.Current!, false);
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
            //ImageSource source;
            //source = GameImageRenderer.GetGenerationImageSourceX2(gameState);
            Task task = GameImageRenderer.GetGenerationImageSourceX2(JSRuntime, canvasElement, gameState, refreshPage);

            //imgGame.Source = source;

            generationN = $"Ant Generation #{gameState.GenerationN}";

            if (isLoading)
            {
                // After finishing the loading screen, we need to delete the loading screen
                task.ContinueWith(async task =>
                {
                    isLoading = false;
                    await InvokeAsync(StateHasChanged);

                });
            } else
            {
                // Forcing the Generations field to be synchronous
                InvokeAsync(StateHasChanged);
            }
        }

        // private async void Draw()
        // {
        //     await JSRuntime.InvokeVoidAsync("LangtonsAnt.drawCanvas", canvasElement);
        // }

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
