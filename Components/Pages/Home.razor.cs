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
        const int imagesPerSecond = 10;
        const int nGenerations = 100;
        const int generationsPerStep = 1;

        private string currentRule = string.Empty;
        private string generationN = "Ant Generation #0";
        private bool btnPlayVisibility = true;
        private bool btnPauseVisibility = true;

        private ElementReference canvasElement;
        private bool isClickEventEnabled = false;
        private bool isLoading = true;
        private string statusMessage = "Status: Ready";

        FluentInputFile? fileInput = default!;

        //protected override async Task OnInitializedAsync()
        // protected override  void OnInitialized()
        // {
        //     base.OnInitialized();
        //     buffer = CreateGameBuffer(null, nGenerations, rule);
        //     //buffer = CreateGameBuffer(null, nGenerations, rule);

        //     // FRANK: SKIPPING THE TIMER STUFF FOR NOW DOING MANUAL ONLY

        //     // gameTimer = new DispatcherTimer();
        //     // gameTimer.Interval = TimeSpan.FromMilliseconds(1000 / imagesPerSecond);
        //     // gameTimer.Tick += (sender, args) =>
        //     // {
        //     //     if (buffer.MoveNext(generationsPerStep))
        //     //         UpdateGameView(buffer.Current!);
        //     //     else
        //     //     {
        //     //         MessageBox.Show("Game Over. We no longer have any ants.");
        //     //         PlayUIState = PlayUIMode.Stopped;
        //     //     }
        //     // };
        //     currentRule = rule;
        //     Rule = rule;
        //     PlayUIState = PlayUIMode.Stopped;
        // }

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
                                UpdateGameView(buffer.Current!);
                            else
                            {
                                DialogService.ShowInfo("Game Over. We no longer have any ants.");
                                PlayUIState = PlayUIMode.Stopped;
                            }
                        }, null, 0, 1000 / imagesPerSecond);


                        btnPlayVisibility = false;
                        btnPauseVisibility = true;
                        break;
                    case PlayUIMode.Paused:
                        gameTimer?.Dispose();


                        btnPauseVisibility = false;
                        btnPlayVisibility = true;
                        break;
                    case PlayUIMode.Stopped:
                        // gameTimer.Stop();

                        // Creating a new game state 
                        buffer = CreateGameBuffer(null, nGenerations, rule);
                        UpdateGameView(buffer.Current!);

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
                        // SetColorsEditMode(true);
                        break;
                    case EditUIMode.EditingAnt:
                        PlayUIState = PlayUIMode.Paused;
                        // SetAntEditMode(true);
                        break;
                    case EditUIMode.EditingRule:
                        PlayUIState = PlayUIMode.Paused;
                        // SetRuleEditMode(true);
                        break;
                    case EditUIMode.NotEditing:
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
            UpdateGameView(buffer.Current!);
        }
        private async Task btnNext_Click(MouseEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;

            if (!buffer.MoveNext())
            {
                DialogService.ShowInfo("Game Over. We no longer have any ants.");
            }
            UpdateGameView(buffer.Current!);
        }
        private void btnEditAnt_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.EditingAnt;
        }
        private void btnEditCellColor_Click(MouseEventArgs e)
        {
            EditUIState = EditUIMode.EditingColors;
        }
        private void btnEditRuleStart_Click(MouseEventArgs e)
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
                UpdateGameView(buffer.Current!);
            }
            catch (Exception ex)
            {
                DialogService.ShowError(ex.Message);
            }

            json.LocalFile?.Delete();
        }
        #endregion

        #region JS - UI

        private void UpdateGameView(IGame gameState)
        {
            //ImageSource source;
            //source = GameImageRenderer.GetGenerationImageSourceX2(gameState);
            Task task = GameImageRenderer.GetGenerationImageSourceX2(JSRuntime, canvasElement, gameState);

            //imgGame.Source = source;

            generationN = "Ant Generation #" + gameState.GenerationN.ToString();

            if (isLoading)
            {
                task.ContinueWith(async task =>
                {
                    isLoading = false;
                    await InvokeAsync(StateHasChanged);

                });
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
                buffer = CreateGameBuffer(null, nGenerations, rule);
                currentRule = rule;
                Rule = rule;
                PlayUIState = PlayUIMode.Stopped;

            }
        }

        #endregion
    }
}
