﻿// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2014 CYBUTEK
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#region Using Directives

using System;

using KerbalEngineer.Extensions;
using KerbalEngineer.Flight.Presets;
using KerbalEngineer.Flight.Readouts;
using KerbalEngineer.UIControls;

using UnityEngine;

#endregion

namespace KerbalEngineer.Flight.Sections {
    public class SectionEditor : MonoBehaviour {
        #region Constants

        public const float Height = 640.0f;
        public const float Width = 500.0f;

        #endregion

        #region Fields

        protected GUIStyle categoryButtonActiveStyle;
        protected GUIStyle categoryButtonStyle;
        protected PopOutElement categoryList;
        protected PopOutReadoutSettings readoutSettings;
        protected PopOutColorPicker backgroundColorPicker;
        protected GUIStyle colorPickerButtonStyle;
        protected GUIStyle categoryTitleButtonStyle;
        protected GUIStyle helpBoxStyle;
        protected GUIStyle helpTextStyle;
        protected GUIStyle panelTitleStyle;
        protected Rect position;
        protected PopOutElement presetList;
        protected GUIStyle readoutButtonStyle, readoutEditButtonStyle;
        protected GUIStyle readoutNameStyle;
        protected Vector2 scrollPositionAvailable;
        protected Vector2 scrollPositionInstalled;
        protected GUIStyle textStyle;
        protected GUIStyle windowStyle;
        protected GUIStyle windowSubtitleStyle;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets and sets the parent section for the section editor.
        /// </summary>
        public SectionModule ParentSection { get; set; }

        /// <summary>
        ///     Gets and sets the window position.
        /// </summary>
        public Rect Position {
            get { return this.position; }
            set { this.position = value; }
        }

        #endregion

        #region Methods: protected

        protected void Awake() {
            try {
                this.categoryList = this.gameObject.AddComponent<PopOutElement>();
                this.categoryList.DrawCallback = this.DrawCategories;

                this.presetList = this.gameObject.AddComponent<PopOutElement>();
                this.presetList.DrawCallback = this.DrawPresets;

                this.readoutSettings = this.gameObject.AddComponent<PopOutReadoutSettings>();
                this.readoutSettings.DrawCallback = () => { if (editingReadout != null) this.readoutSettings.Draw(editingReadout); };
                this.readoutSettings.ClosedCallback = this.SaveReadoutSettings;

                this.backgroundColorPicker = this.gameObject.AddComponent<PopOutColorPicker>();
                this.backgroundColorPicker.DrawCallback = () => {
                    var bg = this.backgroundColorPicker.DrawColorPicker(this.ParentSection.HudBackgroundColor, Unity.Flight.OOPSux.DEFAULT_HUD_BACKGROUND_COLOR, this.ParentSection.IsHudBackground, "HUD background");
                    if (bg.Item1 != this.ParentSection.HudBackgroundColor) {
                        this.ParentSection.IsHudBackground = true;
                        this.ParentSection.SetHudBackgroundColor(bg.Item1);
                    } else this.ParentSection.IsHudBackground = bg.Item2;
                };
            } catch (Exception ex) {
                MyLogger.Exception(ex);
            }
        }

        /// <summary>
        ///     Runs when the object is destroyed.
        /// </summary>
        protected void OnDestroy() {
            try {
            } catch (Exception ex) {
                MyLogger.Exception(ex);
            }
        }

        /// <summary>
        ///     Initialises the object's state on creation.
        /// </summary>
        protected void Start() {
            try {
                this.InitialiseStyles();
                //ReadoutCategory.Selected = ReadoutCategory.GetCategory("Orbital");
            } catch (Exception ex) {
                MyLogger.Exception(ex);
            }
        }
        
        /// <summary>
        ///     Called to draw the editor when the UI is enabled.
        /// </summary>
        protected virtual void OnGUI() {
            if (!HighLogic.LoadedSceneIsFlight || FlightEngineerCore.IsDisplayable == false) {
                return;
            }

            this.position = GUILayout.Window(this.GetInstanceID(), this.position, this.Window, "EDIT SECTION – " + this.ParentSection.Name.ToUpper(), this.windowStyle).ClampToScreen();
            this.ParentSection.EditorPositionX = this.position.x;
            this.ParentSection.EditorPositionY = this.position.y;
        }
        
        /// <summary>
        ///     Draws the categories list drop down UI.
        /// </summary>
        protected virtual void DrawCategories() {
            foreach (var category in ReadoutCategory.Categories) {
                var description = category.Description;
                if (description.Length > 50) {
                    description = description.Substring(0, 50 - 1) + "...";
                }

                if (GUILayout.Button("<b>" + category.Name.ToUpper() + "</b>" + (string.IsNullOrEmpty(category.Description) ? string.Empty : "\n<i>" + description + "</i>"), category == ReadoutCategory.Selected ? this.categoryButtonActiveStyle : this.categoryButtonStyle)) {
                    ReadoutCategory.Selected = category;
                    this.categoryList.enabled = false;
                }
            }
        }
        
        private Texture2D colorPickerSwatch = new Texture2D(16, 20);

        /// <summary>
        ///     Draws the options for editing custom sections.
        /// </summary>
        protected virtual void DrawCustomOptions() {
            GUILayout.Label("Drag the section to reposition, right-click-drag to resize, Alt+MMB to edit", this.windowSubtitleStyle);

            GUILayout.BeginHorizontal(GUILayout.Height(25.0f));

            this.ParentSection.Name = GUILayout.TextField(this.ParentSection.Name, this.textStyle);
            var isShowingInControlBar = !string.IsNullOrEmpty(this.ParentSection.Abbreviation);
            this.ParentSection.Abbreviation = GUILayout.TextField(this.ParentSection.Abbreviation, this.textStyle, GUILayout.Width(75.0f));

            ParentSection.IsHud = GUILayout.Toggle(this.ParentSection.IsHud, "HUD", this.readoutButtonStyle, GUILayout.Width(ParentSection.IsHud ? 46.0f : 78.0f));
            if (ParentSection.IsHud) {
                Color normalGuiColor = GUI.color;
                GUI.color = ParentSection.HudBackgroundColor;

                if (GUILayout.Button(colorPickerSwatch, colorPickerButtonStyle, GUILayout.Width(30.0f))) {
                    if (Event.current.button == 0 /* LMB */) backgroundColorPicker.Open();
                    else if (Event.current.button == 1 /* RMB */) this.ParentSection.IsHudBackground = !this.ParentSection.IsHudBackground;
                }

                if (backgroundColorPicker.enabled && Event.current.type == EventType.Repaint) {
                    backgroundColorPicker.SetPosition(GUILayoutUtility.GetLastRect().Translate(Position).Translate(new Rect(6, 0, 8, 8)), new Rect(0, 0, 180, 20));
                }
                
                GUI.color = normalGuiColor;
            }

            if (isShowingInControlBar && string.IsNullOrEmpty(this.ParentSection.Abbreviation)) {
                DisplayStack.Instance.RequestResize();
            }

            if (GUILayout.Button("DELETE SECTION", this.readoutButtonStyle, GUILayout.Width(150.0f))) {
                this.ParentSection.IsFloating = false;
                this.ParentSection.IsEditorVisible = false;
                this.ParentSection.IsDeleted = true;

                if (SectionLibrary.StockSections.Contains(this.ParentSection))
                    SectionLibrary.StockSections.Remove(this.ParentSection);
                if (SectionLibrary.CustomSections.Contains(this.ParentSection))
                    SectionLibrary.CustomSections.Remove(this.ParentSection);

                DisplayStack.Instance.RequestResize();
            }

            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        ///     Draws the presetsList selection list.
        /// </summary>
        protected virtual void DrawPresetSelector() {
            this.presetList.enabled = GUILayout.Toggle(this.presetList.enabled, "▼ PRESETS ▼", this.categoryTitleButtonStyle, GUILayout.Width(150.0f));
            if (Event.current.type == EventType.Repaint) {
                this.presetList.SetPosition(GUILayoutUtility.GetLastRect().Translate(this.position), GUILayoutUtility.GetLastRect());
            }
        }
        
        /// <summary>
        ///     Draws the editor window.
        /// </summary>
        protected void Window(int windowId) {
            try {
                this.DrawCustomOptions();
                GUILayout.BeginHorizontal();
                this.DrawCategorySelector();
                this.DrawPresetSelector();
                GUILayout.EndHorizontal();
                this.DrawAvailableReadouts();
                GUILayout.Space(5.0f);
                this.DrawInstalledReadouts();

                if (GUILayout.Button("CLOSE EDITOR", this.categoryTitleButtonStyle)) {
                    this.ParentSection.IsEditorVisible = false;
                }

                GUI.DragWindow();
            } catch (Exception ex) {
                MyLogger.Exception(ex);
            }
        }

        #endregion

        #region Methods: private
        
        private ReadoutModule editingReadout = null;

        private void SaveReadoutSettings() {
            if (editingReadout != null) ReadoutLibrary.SaveReadoutConfig(editingReadout);
        }

        /// <summary>
        ///     Draws the available readouts panel.
        /// </summary>
        private void DrawAvailableReadouts() {
            GUI.skin = HighLogic.Skin;
            this.scrollPositionAvailable = GUILayout.BeginScrollView(this.scrollPositionAvailable, false, true, GUILayout.Height(this.position.height * 0.4f));
            GUI.skin = null;

            GUILayout.Label("AVAILABLE", this.panelTitleStyle);

            foreach (var readout in ReadoutLibrary.GetCategory(ReadoutCategory.Selected)) {
                if (!this.ParentSection.ReadoutModules.Contains(readout) || readout.Cloneable) {
                    GUILayout.BeginHorizontal(GUILayout.Height(30.0f));
                    GUILayout.Label(readout.Name, this.readoutNameStyle);
                    readout.ShowHelp = GUILayout.Toggle(readout.ShowHelp, "?", this.readoutButtonStyle, GUILayout.Width(30.0f));
                    if (GUILayout.Button("INSTALL", this.readoutButtonStyle, GUILayout.Width(75.0f))) {
                        this.ParentSection.ReadoutModules.Add(readout);
                    }
                    GUILayout.EndHorizontal();

                    this.ShowHelpMessage(readout);
                }
            }

            GUILayout.EndScrollView();
        }

        /// <summary>
        ///     Draws the readoutCategories selection list.
        /// </summary>
        private void DrawCategorySelector() {
            this.categoryList.enabled = GUILayout.Toggle(this.categoryList.enabled, "▼ SELECTED CATEGORY: " + ReadoutCategory.Selected.ToString().ToUpper() + " ▼", this.categoryTitleButtonStyle);
            if (Event.current.type == EventType.Repaint) {
                this.categoryList.SetPosition(GUILayoutUtility.GetLastRect().Translate(this.position), GUILayoutUtility.GetLastRect());
            }
        }

        private Rect scrollRectInstalled = new Rect();

        /// <summary>
        ///     Draws the installed readouts panel.
        /// </summary>
        private void DrawInstalledReadouts() {
            GUI.skin = HighLogic.Skin;
            this.scrollPositionInstalled = GUILayout.BeginScrollView(this.scrollPositionInstalled, false, true);

            GUI.skin = null;

            GUILayout.Label("INSTALLED", this.panelTitleStyle);
            var removeReadout = false;
            var removeReadoutIndex = 0;

            for (var i = 0; i < this.ParentSection.ReadoutModules.Count; i++) {
                var readout = this.ParentSection.ReadoutModules[i];

                GUILayout.BeginHorizontal(GUILayout.Height(30.0f));
                
                GUILayout.Label(readout.Name, this.readoutNameStyle);


                if (GUILayout.Button("▲", this.readoutButtonStyle, GUILayout.Width(30.0f))) {
                    if (i > 0) {
                        this.ParentSection.ReadoutModules[i] = this.ParentSection.ReadoutModules[i - 1];
                        this.ParentSection.ReadoutModules[i - 1] = readout;
                    }
                }

                if (GUILayout.Button("▼", this.readoutButtonStyle, GUILayout.Width(30.0f))) {
                    if (i < this.ParentSection.ReadoutModules.Count - 1) {
                        this.ParentSection.ReadoutModules[i] = this.ParentSection.ReadoutModules[i + 1];
                        this.ParentSection.ReadoutModules[i + 1] = readout;
                    }
                }


                if (readout.Cloneable == false) {
                    if (GUILayout.Button("✎", this.readoutEditButtonStyle, GUILayout.Width(30.0f))) { //No ⚙ symbol in the in-game font :(
                        editingReadout = readout;
                        readoutSettings.Open();
                        readoutSettings.ResizeCounter = 5; //The window is too tall when first layed out and it takes multiple frames for .Window to shrink it for some reason...
                    }

                    if (editingReadout == readout && Event.current.type == EventType.Repaint && readoutSettings.enabled) {
                        readoutSettings.SetPosition(GUILayoutUtility.GetLastRect().Translate(this.position).Translate(new Rect(8, scrollRectInstalled.y - scrollPositionInstalled.y, 8, 8)), new Rect(0, 0, 200, 0));
                    }
                } else { //dont show for separators.
                    GUILayout.Label("", GUILayout.Width(26.0f));
                }


                readout.ShowHelp = GUILayout.Toggle(readout.ShowHelp, "?", this.readoutButtonStyle, GUILayout.Width(30.0f));


                if (GUILayout.Button("REMOVE", this.readoutButtonStyle, GUILayout.Width(75.0f))) {
                    removeReadout = true;
                    removeReadoutIndex = i;
                }

                GUILayout.EndHorizontal();

                this.ShowHelpMessage(readout);
            }

            GUILayout.EndScrollView();

            if (Event.current.type == EventType.Repaint) {
                scrollRectInstalled = GUILayoutUtility.GetLastRect();
            }

            if (removeReadout) {
                this.ParentSection.ReadoutModules.RemoveAt(removeReadoutIndex);
            }
        }

        private void DrawPresetButton(Preset preset) {
            if (!GUILayout.Button("<b>" + preset.Name.ToUpper() + "</b>", this.categoryButtonStyle)) {
                return;
            }

            this.ParentSection.ApplyPreset(preset);

            this.presetList.enabled = false;
        }

        private void DrawPresetSaveButton() {
            if (!GUILayout.Button("<b>SAVE PRESET</b>", this.categoryButtonStyle)) {
                return;
            }

            this.SavePreset(PresetLibrary.Presets.Find(p => String.Equals(p.Name, this.ParentSection.Name, StringComparison.CurrentCultureIgnoreCase)));
        }

        /// <summary>
        ///     Draws the preset list drop down UI.
        /// </summary>
        private void DrawPresets() {
            Preset removePreset = null;
            foreach (var preset in PresetLibrary.Presets) {
                GUILayout.BeginHorizontal();
                this.DrawPresetButton(preset);
                if (GUILayout.Button("<b>X</b>", this.categoryButtonStyle, GUILayout.Width(30.0f))) {
                    removePreset = preset;
                }
                GUILayout.EndHorizontal();
            }
            if (removePreset != null && PresetLibrary.Remove(removePreset)) {
                this.presetList.ResizeCounter = 1;
            }

            this.DrawPresetSaveButton();
        }


        /// <summary>
        ///     Initialises all the styles required for this object.
        /// </summary>
        private void InitialiseStyles() {
            this.windowStyle = new GUIStyle(HighLogic.Skin.window);
            
            this.windowSubtitleStyle = new GUIStyle(HighLogic.Skin.label) {
                normal =
                {
                    textColor = Color.white
                },
                margin = new RectOffset(0, 0, 0, 5),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                stretchWidth = true,
                stretchHeight = true,
                fixedHeight = 16
            };

            this.colorPickerButtonStyle = new GUIStyle(HighLogic.Skin.button) {
                normal = { textColor = Color.white },
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                stretchHeight = true
            };

            this.categoryButtonStyle = new GUIStyle(HighLogic.Skin.button) {
                normal =
                {
                    textColor = Color.white
                },
                margin = new RectOffset(0, 0, 2, 0),
                padding = new RectOffset(5, 5, 5, 5),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                richText = true
            };

            this.categoryButtonActiveStyle = new GUIStyle(this.categoryButtonStyle) {
                normal = this.categoryButtonStyle.onNormal,
                hover = this.categoryButtonStyle.onHover
            };

            this.panelTitleStyle = new GUIStyle(HighLogic.Skin.label) {
                normal =
                {
                    textColor = Color.white
                },
                margin = new RectOffset(),
                padding = new RectOffset(),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30.0f,
                stretchWidth = true
            };
            
            this.textStyle = new GUIStyle(HighLogic.Skin.textField) {
                margin = new RectOffset(3, 3, 3, 3),
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                stretchHeight = true
            };

            this.readoutNameStyle = new GUIStyle(HighLogic.Skin.label) {
                normal =
                {
                    textColor = Color.white
                },
                margin = new RectOffset(),
                padding = new RectOffset(10, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                stretchWidth = true,
                stretchHeight = true
            };

            this.readoutButtonStyle = new GUIStyle(HighLogic.Skin.button) {
                normal =
                {
                    textColor = Color.white
                },
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                stretchHeight = true
            };
            this.readoutEditButtonStyle = new GUIStyle(this.readoutButtonStyle);
            this.readoutEditButtonStyle.fontStyle = FontStyle.Normal;

            this.helpBoxStyle = new GUIStyle(HighLogic.Skin.box) {
                margin = new RectOffset(2, 2, 2, 10),
                padding = new RectOffset(10, 10, 10, 10)
            };

            this.helpTextStyle = new GUIStyle(HighLogic.Skin.label) {
                normal =
                {
                    textColor = Color.yellow
                },
                margin = new RectOffset(),
                padding = new RectOffset(),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                stretchWidth = true,
                richText = true
            };

            this.categoryTitleButtonStyle = new GUIStyle(this.readoutButtonStyle) {
                fixedHeight = 30.0f,
                stretchHeight = false
            };
        }

        private void SavePreset(Preset preset) {
            if (preset == null) {
                preset = new Preset();
            }

            preset.Name = this.ParentSection.Name;
            preset.Abbreviation = this.ParentSection.Abbreviation;
            preset.ReadoutNames = this.ParentSection.ReadoutModuleNames;
            preset.IsHud = this.ParentSection.IsHud;
            preset.IsHudBackground = this.ParentSection.IsHudBackground;

            PresetLibrary.Save(preset);
        }

        private void ShowHelpMessage(ReadoutModule readout) {
            if (!readout.ShowHelp) {
                return;
            }

            GUILayout.BeginVertical(this.helpBoxStyle);
            GUILayout.Label(!String.IsNullOrEmpty(readout.HelpString) ? readout.HelpString : "Sorry, no help information has been provided for this readout module.", this.helpTextStyle);
            GUILayout.EndVertical();
        }

        #endregion
    }
}