﻿using GDE.App.Main.Containers.KeyBindingContainers;
using GDE.App.Main.Panels;
using GDE.App.Main.Screens.Edit.Components;
using GDE.App.Main.Screens.Edit.Components.IDMigration;
using GDE.App.Main.Toasts;
using GDE.App.Main.UI;
using GDE.App.Main.UI.FileDialogComponents;
using GDEdit.Application.Editor;
using GDEdit.Utilities.Enumerations;
using GDEdit.Utilities.Objects.General;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GDE.App.Main.Colors.GDEColors;
using static GDEdit.Utilities.Objects.General.SourceTargetRange;

namespace GDE.App.Main.Screens.Edit
{
    public class IDMigrationPanel : Panel
    {
        private static Color4 greenEnabledColor = FromHex("246c48");
        private static Color4 redEnabledColor = FromHex("6c2424");
        private static Color4 grayEnabledColor = FromHex("242424");

        private ToastNotification notification;

        private NumberTextBox sourceFrom;
        private NumberTextBox sourceTo;
        private NumberTextBox targetFrom;
        private NumberTextBox targetTo;

        private FadeButton performMigration;
        private FadeButton createStep;
        private FadeButton removeSteps;
        private FadeButton cloneSteps;
        private FadeButton selectAll;
        private FadeButton deselectAll;
        private FadeButton loadSteps;
        private FadeButton saveSteps;
        private FadeButton saveStepsAs;

        private Container stepListContainer;

        private IDMigrationStepList[] stepLists = new IDMigrationStepList[4];
        private IDMigrationStepList currentStepList;

        private EditorScreen editorScreen;

        public Editor Editor => editorScreen.Editor;

        public readonly IDMigrationTabControl TabControl;

        /// <summary>The common <seealso cref="SourceTargetRange"/> of the currently selected ID migration steps.</summary>
        public readonly Bindable<SourceTargetRange> CommonIDMigrationStep = new Bindable<SourceTargetRange>();

        public Bindable<OpenFileDialog> OpenFileDialogBindable => editorScreen.OpenFileDialogBindable;
        public Bindable<SaveFileDialog> SaveFileDialogBindable => editorScreen.SaveFileDialogBindable;

        public IDMigrationStepList CurrentStepList
        {
            get => currentStepList;
            set
            {
                var previous = currentStepList;

                previous.TabSelected = false;

                previous.FadeTo(0, 200).OnComplete(RemoveOnCompleted);
                previous.StepSelected = null;
                previous.StepDeselected = null;
                previous.SelectionChanged = null;

                UnbindFileDialogBindables(previous);

                stepListContainer.Add(currentStepList = value);

                currentStepList.TabSelected = true;

                currentStepList.FadeTo(1, 200);
                currentStepList.StepSelected = HandleStepSelected;
                currentStepList.StepDeselected = HandleStepDeselected;
                currentStepList.SelectionChanged = HandleSelectionChanged;

                UpdateFileDialogBindables(currentStepList);

                CommonIDMigrationStep.UnbindAll();
                CommonIDMigrationStep.BindTo(currentStepList.CommonIDMigrationStep);
                CommonIDMigrationStep.ValueChanged += CommonIDMigrationStepChanged;
                CommonIDMigrationStep.TriggerChange();

                Editor.SelectedIDMigrationMode = currentStepList.IDMigrationMode;

                // Since the step list has been changed, technically the selection has changed too; so triggering this is not as hacky as it seems
                HandleSelectionChanged();
            }
        }

        public IDMigrationPanel(EditorScreen e)
        {
            editorScreen = e;

            Size = new Vector2(700, 650);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            CornerRadius = 10;
            Masking = true;

            for (int i = 0; i < 4; i++)
                stepLists[i] = GetNewStepList(Editor, (IDMigrationMode)i);

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                Masking = true,
                Children = new Drawable[]
                {
                    TabControl = new IDMigrationTabControl(),
                    new Container
                    {
                        Size = new Vector2(700, 650 - IDMigrationTabControl.DefaultHeight),
                        Y = IDMigrationTabControl.DefaultHeight,
                        CornerRadius = 10,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = FromHex("1a1a1a")
                            },
                            new IDMigrationActionContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    stepListContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 520,
                                        CornerRadius = 10,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = FromHex("111111"),
                                            },
                                            currentStepList = stepLists[0]
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding
                                        {
                                            Top = 5,
                                            Horizontal = 10,
                                        },
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 160,
                                        Children = new Drawable[]
                                        {
                                            GetNewSpriteText("Source From"),
                                            sourceFrom = GetNewNumberTextBox(),
                                            GetNewSpriteText("Source To"),
                                            sourceTo = GetNewNumberTextBox(),
                                            GetNewSpriteText("Target From"),
                                            targetFrom = GetNewNumberTextBox(),
                                            GetNewSpriteText("Target To"),
                                            targetTo = GetNewNumberTextBox(),
                                        },
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomRight,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(10),
                                        Margin = new MarginPadding(10),
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 160,
                                        Children = new Drawable[]
                                        {
                                            performMigration = GetNewFadeButton(8, "Perform Migration", greenEnabledColor, PerformMigration),
                                            removeSteps = GetNewFadeButton(0, "Remove Steps", redEnabledColor, CurrentStepList.RemoveSelectedSteps),
                                            cloneSteps = GetNewFadeButton(0, "Clone Steps", grayEnabledColor, CurrentStepList.CloneSelectedSteps),
                                            deselectAll = GetNewFadeButton(0, "Deselect All", grayEnabledColor, CurrentStepList.DeselectAll),
                                            selectAll = GetNewFadeButton(0, "Select All", grayEnabledColor, CurrentStepList.SelectAll),
                                            loadSteps = GetNewFadeButton(0, "Load Steps", grayEnabledColor, CurrentStepList.LoadSteps),
                                            saveStepsAs = GetNewFadeButton(0, "Save Steps As", grayEnabledColor, CurrentStepList.SaveStepsAs),
                                            saveSteps = GetNewFadeButton(0, "Save Steps", grayEnabledColor, CurrentStepList.SaveSteps),
                                            createStep = GetNewFadeButton(0, "Create Step", greenEnabledColor, CreateNewStep),
                                        },
                                    },
                                }
                            }
                        }
                    },
                    notification = new ToastNotification
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(200, 30),
                        Margin = new MarginPadding { Bottom = 15 },
                        Text = "Finished Operation",
                    },
                }
            });

            TabControl.TabSelected += TabChanged;

            sourceFrom.NumberChanged += HandleSourceFromChanged;
            sourceTo.NumberChanged += HandleSourceToChanged;
            targetFrom.NumberChanged += HandleTargetFromChanged;
            targetTo.NumberChanged += HandleTargetToChanged;

            CommonIDMigrationStep.ValueChanged += CommonIDMigrationStepChanged;

            Editor.IDMigrationOperationCompleted += HandleIDMigrationOperationCompleted;

            // After everything's loaded, initialize the property for things to work properly
            UpdateFileDialogBindables(CurrentStepList = currentStepList);

            UpdateFadeButtonEnabledStates();
        }

        private void RemoveOnCompleted(IDMigrationStepList toRemove) => stepListContainer.Remove(toRemove);

        private void CommonIDMigrationStepChanged(ValueChangedEvent<SourceTargetRange> v)
        {
            var newStep = v.NewValue;
            if (newStep != null)
            {
                newStep.SourceTargetRangeChanged += (sf, st, tf, tt) =>
                {
                    HandleStepChanged(newStep);
                    UpdateTextBoxes(newStep);
                };
                HandleStepChanged(newStep);
            }
            UpdateTextBoxes(newStep);
        }

        private void TabChanged(IDMigrationMode newMode)
        {
            CurrentStepList = stepLists[(int)newMode];
        }

        private void HandleStepChanged(SourceTargetRange newStep)
        {
            foreach (var s in CurrentStepList.SelectedSteps)
            {
                if (newStep.SourceFrom > 0)
                    s.SourceFrom = newStep.SourceFrom;
                if (newStep.SourceTo > 0)
                    s.SourceTo = newStep.SourceTo;
                if (newStep.TargetFrom > 0)
                    s.TargetFrom = newStep.TargetFrom;
            }
        }

        private void HandleSourceFromChanged(int newValue)
        {
            if (newValue > 0 && newValue <= CommonIDMigrationStep.Value.SourceTo)
                CommonIDMigrationStep.Value.SourceFrom = newValue;
        }
        private void HandleSourceToChanged(int newValue)
        {
            if (newValue > 0 && newValue >= CommonIDMigrationStep.Value.SourceFrom)
                CommonIDMigrationStep.Value.SourceTo = newValue;
        }
        private void HandleTargetFromChanged(int newValue)
        {
            if (newValue > 0)
                CommonIDMigrationStep.Value.TargetFrom = newValue;
        }
        private void HandleTargetToChanged(int newValue)
        {
            if (newValue > 0 && newValue >= CommonIDMigrationStep.Value.TargetFrom)
                CommonIDMigrationStep.Value.TargetTo = newValue;
        }

        private void HandleStepSelected(IDMigrationStepCard card) => UpdateFadeButtonEnabledStates();
        private void HandleStepDeselected(IDMigrationStepCard card) => UpdateFadeButtonEnabledStates();
        private void HandleSelectionChanged() => UpdateFadeButtonEnabledStates();

        private void HandleIDMigrationOperationCompleted()
        {
            performMigration.Text = "Perform Migration";
            SetEnabledStateOnFadeButtons(true);
            UpdateFadeButtonEnabledStates();
            notification.Show();
        }

        private void PerformMigration()
        {
            Task.Run(Editor.PerformMigration);
            performMigration.Text = "Performing Migration";
            SetEnabledStateOnFadeButtons(false);
        }
        private void CreateNewStep()
        {
            CurrentStepList.CreateNewStep();
            performMigration.EnabledState = true;
        }

        private void UpdateFileDialogBindables(IDMigrationStepList stepList)
        {
            stepList.OpenFileDialogBindable.BindTo(OpenFileDialogBindable);
            stepList.SaveFileDialogBindable.BindTo(SaveFileDialogBindable);
        }
        private void UnbindFileDialogBindables(IDMigrationStepList stepList)
        {
            stepList.OpenFileDialogBindable.UnbindAll();
            stepList.SaveFileDialogBindable.UnbindAll();
        }

        private void UpdateFadeButtonEnabledStates()
        {
            deselectAll.EnabledState = CurrentStepList.SelectedSteps.Count > 0;
            removeSteps.EnabledState = CurrentStepList.SelectedSteps.Count > 0;
            cloneSteps.EnabledState = CurrentStepList.SelectedSteps.Count > 0;
            performMigration.EnabledState = Editor.CurrentlySelectedIDMigrationSteps.Count > 0;
        }
        private void SetEnabledStateOnFadeButtons(bool newState)
        {
            performMigration.EnabledState = createStep.EnabledState = saveSteps.EnabledState = saveStepsAs.EnabledState = loadSteps.EnabledState = selectAll.EnabledState = deselectAll.EnabledState = cloneSteps.EnabledState = removeSteps.EnabledState = newState;
        }

        private void UpdateTextBoxes(SourceTargetRange range)
        {
            UpdateTextBox(sourceFrom, range?.SourceFrom);
            UpdateTextBox(sourceTo, range?.SourceTo);
            UpdateTextBox(targetFrom, range?.TargetFrom);
            UpdateTextBox(targetTo, range?.TargetTo);
        }
        private void UpdateTextBox(NumberTextBox textBox, int? newValue)
        {
            bool enabled = newValue.HasValue;
            bool isValid = enabled && newValue > InvalidValue;
            textBox.InvokeEvents = false;
            if (isValid)
                textBox.Number = newValue.Value;
            else
                textBox.Text = "";
            textBox.InvokeEvents = true;
            textBox.Enabled = enabled;
        }

        private static FadeButton GetNewFadeButton(float topMargin, string text, Color4 enabledColor, Action action) => new FadeButton
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = topMargin },
            Text = text,
            EnabledColor = enabledColor,
            Action = action,
        };
        private static NumberTextBox GetNewNumberTextBox() => new NumberTextBox(false)
        {
            RelativeSizeAxes = Axes.X,
            Height = 30,
        };
        private static SpriteText GetNewSpriteText(string text) => new SpriteText
        {
            Text = text,
        };
        private static IDMigrationStepList GetNewStepList(Editor editor, IDMigrationMode mode) => new IDMigrationStepList(editor, mode)
        {
            RelativeSizeAxes = Axes.Y,
            Width = 500,
            Padding = new MarginPadding { Vertical = 10 },
            Alpha = 0,
        };
    }
}