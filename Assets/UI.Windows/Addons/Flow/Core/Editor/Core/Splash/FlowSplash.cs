﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using ME;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowSplash {

		public enum State : byte {

			ProjectSelector,
			NewProject,

		};

		public State state = State.ProjectSelector;

		private Vector2 dataSelectionScroll;
		private Texture splash;
		public FlowData cachedData;
		private FlowData[] scannedData;

		private GUISkin skin;

		private FlowSystemEditorWindow editor;

		public FlowSplash(FlowSystemEditorWindow editor) {

			this.editor = editor;

			if (this.skin == null) this.skin = Resources.Load<GUISkin>("UI.Windows/Flow/Styles/" + (EditorGUIUtility.isProSkin == true ? "SkinDark" : "SkinLight"));
			if (this.splash == null) this.splash = Resources.Load<Texture>("UI.Windows/Flow/Splash");

		}

		public bool Draw() {

			var result = true;
			var hasData = FlowSystem.HasData();

			if (FlowSystemEditorWindow.loaded == false) {

				this.DrawLoader();
				result = false;

			} else if (hasData == false) {
				
				if (this.state == State.ProjectSelector) {

					this.DrawSplash(this.DrawProjectSelector);

				} else if (this.state == State.NewProject) {
					
					this.DrawSplash(this.DrawNewProject);

				}

				result = false;

			}

			return result;

		}

		public void DrawLoader() {
			
			this.editor.DrawBackground();
			
			var darkLabel = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "DarkLabel", (styleName) => {
				
				var _darkLabel = FlowSystemEditorWindow.defaultSkin.FindStyle(styleName);
				_darkLabel.alignment = TextAnchor.MiddleCenter;
				_darkLabel.stretchWidth = true;
				_darkLabel.stretchHeight = true;
				_darkLabel.fixedWidth = 0f;
				_darkLabel.fixedHeight = 0f;
				
				return _darkLabel;
				
			});
			
			var rect = FlowSystemEditor.GetCenterRect(this.editor.position, this.splash.width, this.splash.height);
			
			var boxStyle = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "boxStyle", (styleName) => {
				
				var _boxStyle = new GUIStyle(GUI.skin.box);
				_boxStyle.margin = new RectOffset(0, 0, 0, 0);
				_boxStyle.padding = new RectOffset(0, 0, 0, 0);
				_boxStyle.normal.background = null;
				
				return _boxStyle;
				
			});
			
			GUI.Box(rect, this.splash, boxStyle);
			
			var width = 730f;
			var height = 456f;
			var rectOffset = FlowSystemEditor.GetCenterRect(this.editor.position, width, height);
			
			var marginLeft = 240f;
			var margin = 20f;
			
			var padding = 20f;
			
			GUILayout.BeginArea(rectOffset);
			{
				
				var borderWidth = width - marginLeft - margin;
				var borderHeight = height - margin * 2f;
				
				var labelStyle = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "sv_iconselector_labelselection");
				
				GUILayout.BeginArea(new Rect(marginLeft, margin, borderWidth, borderHeight), labelStyle);
				{
					
					GUILayout.BeginArea(new Rect(padding, padding, borderWidth - padding * 2f, borderHeight - padding * 2f));
					{
						
						GUILayout.Label("Loading...", darkLabel);
						
					}
					GUILayout.EndArea();
					
				}
				GUILayout.EndArea();
				
			}
			GUILayout.EndArea();
			
		}

		private void DrawSplash(System.Action drawer) {

			this.editor.BeginWindows();

			this.editor.DrawBackground();
			
			if (this.splash == null) this.splash = Resources.Load<Texture>("UI.Windows/Flow/Splash");

			var rect = FlowSystemEditor.GetCenterRect(this.editor.position, this.splash.width, this.splash.height);
			
			var boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.margin = new RectOffset(0, 0, 0, 0);
			boxStyle.padding = new RectOffset(0, 0, 0, 0);
			boxStyle.normal.background = null;
			GUI.Box(rect, this.splash, boxStyle);
			
			var width = 730f;
			var height = 456f;
			var rectOffset = FlowSystemEditor.GetCenterRect(this.editor.position, width, height);
			
			var marginLeft = 240f;
			var margin = 20f;
			
			var padding = 20f;
			
			GUILayout.BeginArea(rectOffset);
			{
				
				var borderWidth = width - marginLeft - margin;
				var borderHeight = height - margin * 2f;
				
				var labelStyle = ME.Utilities.CacheStyle("FlowEditor.DataSelection.Styles", "sv_iconselector_labelselection");
				
				GUILayout.BeginArea(new Rect(marginLeft, margin, borderWidth, borderHeight), labelStyle);
				{
					
					GUILayout.BeginArea(new Rect(padding, padding, borderWidth - padding * 2f, borderHeight - padding * 2f));
					{

						drawer.Invoke();
						
					}
					GUILayout.EndArea();
					
				}
				GUILayout.EndArea();
				
			}
			GUILayout.EndArea();
			
			this.editor.EndWindows();

		}

		private string projectFolder = "Assets/";
		private string projectNamespace = string.Empty;
		private bool projectNamespaceAuto = true;
		public void DrawNewProject() {

			var hasErrors = false;

			var darkLabel = ME.Utilities.CacheStyle("FlowEditor.DataSelection.Styles", "DarkLabel", (name) => {
				
				var style = new GUIStyle(FlowSystemEditorWindow.defaultSkin.FindStyle(name));
				style.alignment = TextAnchor.MiddleLeft;
				return style;
				
			});
			
			GUILayout.Label("1. Select the project folder:", darkLabel);
			GUILayout.BeginHorizontal(GUILayout.Height(30f));
			{

				this.projectFolder = EditorGUILayout.TextField(this.projectFolder, this.skin.textField).UppercaseWords();
				if (GUILayout.Button("...", this.skin.button, GUILayout.Width(35f)) == true) {

					var path = EditorUtility.SaveFolderPanel("Select Empty Project Folder", "Assets/", "").Trim();
					if (string.IsNullOrEmpty(path) == false) {

						var splitted = path.Split(new string[] { Application.dataPath }, StringSplitOptions.None);
						if (splitted.Length > 1) {

							path = "Assets" + splitted[1];

						}

						this.projectFolder = path;

					}

				}

			}
			GUILayout.EndHorizontal();

			// Test it
			if (Directory.Exists(this.projectFolder) == true &&
			    (Directory.GetFiles(this.projectFolder).Count() > 0 ||
			    Directory.GetDirectories(this.projectFolder).Count() > 0)) {

				EditorGUILayout.HelpBox("Directory should not exists or must be empty.", MessageType.Error);
				hasErrors = true;

			}

			if (this.projectNamespaceAuto == true || string.IsNullOrEmpty(this.projectNamespace) == true) {

				this.projectNamespace = this.projectFolder.Split('/').Last().Trim() + ".UI";

			}

			GUILayout.BeginHorizontal();
			{
				
				GUILayout.Label("2. Project namespace:", darkLabel);
				GUILayout.FlexibleSpace();
				this.projectNamespaceAuto = GUILayout.Toggle(this.projectNamespaceAuto, "Automatic");

			}
			GUILayout.EndHorizontal();
			
			var oldState = GUI.enabled;

			GUILayout.BeginHorizontal(GUILayout.Height(30f));
			{

				GUI.enabled = oldState && !this.projectNamespaceAuto;
				this.projectNamespace = EditorGUILayout.TextField(this.projectNamespace, this.skin.textField);
				GUI.enabled = oldState;

			}
			GUILayout.EndHorizontal();

			// Test it
			//var type = Type.GetType("Assembly-CSharp", throwOnError: true, ignoreCase: true);
			string assemblyName = "Assembly-CSharp";

			var asm = AppDomain.CurrentDomain.GetAssemblies().Where((item) => {
				
				var name = item.FullName.ToLower().Trim();
				return name.StartsWith(assemblyName.ToLower() + ",");
				
			}).FirstOrDefault();
			
			if (asm != null) {
				
				var contains = asm.GetTypes().Any((type) => {

					return type.Namespace != null && type.Namespace.ToLower().Trim() == this.projectNamespace.ToLower().Trim();
					
				});
				
				if (contains == true) {
					
					EditorGUILayout.HelpBox("Namespace already exists.", MessageType.Error);
					hasErrors = true;

				}
				
			} else {
				
				EditorGUILayout.HelpBox("Assembly was not found. Try to re-open Unity.", MessageType.Error);
				hasErrors = true;

			}

			var pattern = @"^([A-Za-z]+[\.a-zA-Z0-9_]*)$";
			var ns = this.projectNamespace;
			var rgx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			var isMatch = rgx.IsMatch(ns);

			if (isMatch == false) {
				
				EditorGUILayout.HelpBox("Namespace can contains `.`, `_`, A-Z and 0-9. First symbol must be a char.", MessageType.Error);
				hasErrors = true;

			}

			GUILayout.FlexibleSpace();
			
			CustomGUI.Splitter();
			
			GUILayout.BeginHorizontal();
			{
				
				if (GUILayout.Button("Cancel", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(40f)) == true) {
					
					this.state = State.ProjectSelector;
					
				}
				
				GUILayout.FlexibleSpace();

				oldState = GUI.enabled;
				GUI.enabled = oldState && !hasErrors;
				if (GUILayout.Button("Create & Open", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(150f), GUILayout.Height(40f)) == true) {
					
					this.state = State.ProjectSelector;
					var data = this.CreateProject(this.projectFolder, this.projectNamespace);
					this.editor.OpenFlowData(data);

					EditorUtilities.ResetCache<FlowData>();

				}
				GUI.enabled = oldState;

			}
			GUILayout.EndHorizontal();

		}

		public FlowData CreateProject(string projectFolder, string projectNamespace) {

			if (Directory.Exists(projectFolder) == false) {

				Directory.CreateDirectory(projectFolder);

			}

			var filepath = projectFolder + "/" + projectNamespace;
			var data = ME.EditorUtilities.CreateAsset<FlowData>(filepath);
			data.flowWindowWithLayout = true;
			data.namespaceName = projectNamespace;
			data.version = VersionInfo.BUNDLE_VERSION;

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Selection.activeObject = data;

			return data;

		}

		public void DrawProjectSelector() {
			
			var darkLabel = ME.Utilities.CacheStyle("FlowEditor.DataSelection.Styles", "DarkLabel", (name) => {
				
				var style = new GUIStyle(FlowSystemEditorWindow.defaultSkin.FindStyle(name));
				style.alignment = TextAnchor.MiddleLeft;
				return style;
				
			});

			var headerStyle = new GUIStyle("LODLevelNotifyText");
			headerStyle.normal.textColor = EditorGUIUtility.isProSkin == true ? Color.white : Color.black;
			headerStyle.fontSize = 18;
			headerStyle.alignment = TextAnchor.MiddleCenter;
			
			GUILayoutExt.LabelWithShadow("UI.Windows Flow Extension v" + VersionInfo.BUNDLE_VERSION, headerStyle);
			
			GUILayout.Space(10f);
			
			GUILayout.Label("Open one of your projects:", darkLabel);
			
			var backStyle = new GUIStyle("sv_iconselector_labelselection");
			
			var skin = GUI.skin;
			GUI.skin = FlowSystemEditorWindow.defaultSkin;
			this.dataSelectionScroll = GUILayout.BeginScrollView(this.dataSelectionScroll, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, backStyle);
			{
				
				GUI.skin = skin;

				this.scannedData = EditorUtilities.GetAssetsOfType<FlowData>(useCache: false);

				if (this.scannedData.Length == 0) {
					
					var center = new GUIStyle(darkLabel);
					center.fixedWidth = 0f;
					center.fixedHeight = 0f;
					center.stretchWidth = true;
					center.stretchHeight = true;
					center.alignment = TextAnchor.MiddleCenter;
					center.wordWrap = true;
					
					GUILayout.Label("No projects was found. Create a new one by Right-Click on any folder in Project View and choosing Create->UI.Windows->Flow->Graph option.", center);
					
				} else {
					
					var buttonStyle = new GUIStyle("U2D.createRect");
					buttonStyle.padding = new RectOffset(15, 15, 15, 15);
					buttonStyle.margin = new RectOffset(2, 2, 2, 2);
					buttonStyle.fixedWidth = 0f;
					buttonStyle.fixedHeight = 0f;
					buttonStyle.stretchWidth = true;
					buttonStyle.stretchHeight = false;
					buttonStyle.normal.textColor = Color.black;
					buttonStyle.fontSize = 12;
					buttonStyle.richText = true;
					
					var buttonStyleSelected = new GUIStyle(buttonStyle);
					
					buttonStyle.normal.background = null;
					
					this.scannedData = this.scannedData.OrderByDescending((data) => (data != null ? data.lastModifiedUnix : 0)).ToArray();
					
					foreach (var data in this.scannedData) {
						
						if (data == null) continue;
						
						var title = data.name + "\n";
						title += "<color=#777><size=10>Modified: " + data.lastModified + "</size></color>\n";
						title += "<color=#888><size=10>Version: " + data.version + "</size></color>";
						
						if (GUILayout.Button(title, this.cachedData == data ? buttonStyleSelected : buttonStyle) == true) {
							
							this.cachedData = data;
							
						}
						
					}
					
				}
				
				GUILayout.FlexibleSpace();
				
			}
			GUILayout.EndScrollView();
			
			GUILayout.Space(10f);
			
			GUILayout.Label("Or select the project file:", darkLabel);
			
			this.cachedData = GUILayoutExt.ObjectField<FlowData>(this.cachedData, false, FlowSystemEditorWindow.defaultSkin.FindStyle("ObjectField"));
			
			CustomGUI.Splitter();
			
			GUILayout.BeginHorizontal();
			{

				if (GUILayout.Button("Create Project...", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(150f), GUILayout.Height(40f)) == true) {

					this.state = State.NewProject;

				}

				GUILayout.FlexibleSpace();
				
				var oldState = GUI.enabled;
				GUI.enabled = oldState && this.cachedData != null;

				if (this.cachedData != null && this.cachedData.version < VersionInfo.BUNDLE_VERSION) {
					
					// Need to upgrade
					
					if (GUILayout.Button("Upgrade to " + VersionInfo.BUNDLE_VERSION, FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(150f), GUILayout.Height(40f)) == true) {
						
						UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", this.cachedData.version, VersionInfo.BUNDLE_VERSION), 0f);
						var type = this.cachedData.GetType();
						
						while (this.cachedData.version < VersionInfo.BUNDLE_VERSION) {
							
							var nextVersion = this.cachedData.version + 1;

							try {

								// Try to find upgrade method
								var methodName = "UpgradeTo" + nextVersion.ToSmallWithoutTypeString();
								var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
								if (methodInfo != null) {
									
									methodInfo.Invoke(this.cachedData, null);

									Debug.Log("[UPGRADE] Invoked: `" + methodName + "`, version " + nextVersion);

								} else {
									
									Debug.Log("[UPGRADE] Method `" + methodName + "` was not found: version " + nextVersion + " skipped");
									
								}

								UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", this.cachedData.version, nextVersion), 0.5f);

							} catch (UnityException) {
							} finally {

								UnityEditor.EditorUtility.ClearProgressBar();

							}

							this.cachedData.version = nextVersion;
							UnityEditor.EditorUtility.SetDirty(this.cachedData);
							
						}
						
						UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", this.cachedData.version, VersionInfo.BUNDLE_VERSION), 1f);
						UnityEditor.EditorUtility.ClearProgressBar();
						
					}
					
				} else if (this.cachedData != null && this.cachedData.version > VersionInfo.BUNDLE_VERSION) {

					var info = string.Format(
						"Selected Project has `{0}` version while UI.Windows System has `{1}` version number. Click here to download a new version.",
						this.cachedData.version,
						VersionInfo.BUNDLE_VERSION
					);

					if (GUILayout.Button(new GUIContent(info, new GUIStyle("CN EntryWarn").normal.background), EditorStyles.helpBox, GUILayout.Height(40f)) == true) {
						
						Application.OpenURL(VersionInfo.DOWNLOAD_LINK);
						
					}

				} else {

					if (GUILayout.Button("Open", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(40f)) == true) {
						
						FlowSystem.SetData(this.cachedData);
						
					}
						
				}

				GUI.enabled = oldState;
				
			}
			GUILayout.EndHorizontal();

		}

	}

}