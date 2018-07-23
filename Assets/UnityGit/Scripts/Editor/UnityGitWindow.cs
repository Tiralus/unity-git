

namespace UnityGit
{
	using System.Linq;
	using System.Text;
	using System.Diagnostics;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	public class UnityGitWindow : EditorWindow
	{
		/// <summary>
		/// The titles of the tabs in the window.
		/// </summary>
		private readonly string[] tabTitles = { "Log", "Status", "Commit" };

		private List<LogItem> parsedGitLog = new List<LogItem>();
		
		private List<StatusItem> parsedStatusLog = new List<StatusItem>();

		private string commitMessage = string.Empty;
		
		/// <summary>
		/// The current position of the scroll bar in the GUI.
		/// </summary>
		private Vector2 scrollPosition;

		private TreeViewState gitLogTreeViewState;
		
		/// <summary>
		/// The currently selected tab in the window.
		/// </summary>
		private int currentTab;

		private MultiColumnHeaderState multiColumnHeaderState;

		/// <summary>
		/// Creates the window
		/// </summary>
		/// <returns>The GUI style with the appropriate background color set.</returns>
		[MenuItem("Window/Unity Git")]
		public static UnityGitWindow GetWindow()
		{
			var window = GetWindow<UnityGitWindow>();
			window.titleContent = new GUIContent("Untiy Git");
			window.Focus();
			window.Repaint();
			return window;
		}

		/// <summary>
		/// Updates the window
		/// </summary>
		public void OnGUI()
		{
			currentTab = GUILayout.Toolbar(currentTab, tabTitles);

			switch (currentTab)
			{
				case 0:
					DisplayGitLog();
					break;
				case 1:
					DisplayGitStatus();
					break;
				case 2:
					DisplayCommit();
					break;
			}
		}

		/// <summary>
		/// Call the parse to initialize everything.
		/// </summary>
		public void Awake()
		{
			ParseGitLog();
			ParseGitStatus();
		}

#region Display Methods
		/// <summary>
		/// Display the git log
		/// </summary>
		public void DisplayGitLog()
		{
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical();

            GUIStyle style = CreateColoredBackground();

            if (parsedGitLog != null && parsedGitLog.Count > 0)
            {
                for (int i = 0; i < parsedGitLog.Count; i++)
                {
                    // alternate the background color for each entry
                    if (i % 2 == 0)
                        EditorGUILayout.BeginVertical();
                    else
                        EditorGUILayout.BeginVertical(style);

					foreach (string displayString in parsedGitLog[i].parsedLogItem)
					{
						DisplayLabel(displayString);
					}
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
	            DisplayLabel("There is not a repository!");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
		
		/// <summary>
		/// Dispaly the status tab
		/// </summary>
		public void DisplayGitStatus()
		{
			DisplayStatusHeader();
			// display all of the installed packages
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			EditorGUILayout.BeginVertical();

			GUIStyle style = CreateColoredBackground();

			if (parsedStatusLog != null && parsedStatusLog.Count > 0)
			{
				for (int i = 0; i < parsedStatusLog.Count; i++)
				{
					// alternate the background color for each package
					if (i % 2 == 0)
						EditorGUILayout.BeginVertical();
					else
						EditorGUILayout.BeginVertical(style);

					EditorGUILayout.BeginHorizontal();
					{
						DisplayLabel(parsedStatusLog[i].status);
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
				}
			}
			else
			{
				DisplayLabel("There are no files modified!");
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}
		
		/// <summary>
		/// Display the head for the status tab
		/// </summary>
		public void DisplayStatusHeader()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Add All"))
				{
					CallProcess("git", "add -A");
					ParseGitStatus();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		
		/// <summary>
		/// Display the commit tab
		/// </summary>
		public void DisplayCommit()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorStyles.label.fontStyle = FontStyle.Bold;
			EditorStyles.label.fontSize = 14;
			commitMessage = EditorGUILayout.TextField("Message: ", commitMessage, GUILayout.Height(20));
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Commit"))
			{
				CallProcess("git", "commit -m" + "\"" + commitMessage + "\"");
			}
			if (GUILayout.Button("Push"))
			{
				CallProcess("git", "push");
			}
			EditorGUILayout.EndVertical();
		}
		
		/// <summary>
		/// Generic method to display a label
		/// </summary>
		public void DisplayLabel(string label)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorStyles.label.fontStyle = FontStyle.Bold;
				EditorStyles.label.fontSize = 14;
				EditorGUILayout.LabelField(string.Format("{0}", label), GUILayout.Height(20));
			}
			EditorGUILayout.EndHorizontal();
		}
#endregion
		
        /// <summary>
        /// Creates the alternating background color based upon if the Unity Editor is the free (light) skin or the Pro (dark) skin.
        /// </summary>
        /// <returns>The GUI style with the appropriate background color set.</returns>
        private GUIStyle CreateColoredBackground()
        {
            GUIStyle style = new GUIStyle();
            if (Application.HasProLicense())
            {
                style.normal.background = MakeTex(20, 20, new Color(0.3f, 0.3f, 0.3f));
            }
            else
            {
                style.normal.background = MakeTex(20, 20, new Color(0.6f, 0.6f, 0.6f));
            }

            return style;
        }

        /// <summary>
        /// From here: http://forum.unity3d.com/threads/changing-the-background-color-for-beginhorizontal.66015/
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

#region Parsing methods
		/// <summary>
		/// Call git and get the output from git log, split it to be easier to display.
		/// </summary>
        public void ParseGitLog()
		{
			string gitLogOutput = CallProcess("git", "log --pretty=format:" + "\"" + "%h%x09%an%x09%ad%x09%s" + "\"" + "--date=short");

			gitLogOutput = gitLogOutput.Replace("--date=short", "");
			List<string> parsedOutput = gitLogOutput.Split(System.Environment.NewLine.ToCharArray()).ToList();
			
			parsedGitLog.Clear();

			foreach (string parsed in parsedOutput)
			{
				List<string> splitParsed = parsed.Split('\t').ToList();
				
				// Log format:
				// hash username date comment
				LogItem item = new LogItem();

				item.parsedLogItem = splitParsed;
				
				parsedGitLog.Add(item);
			}
		}

		
		/// <summary>
		/// Call git and get the output from git status
		/// </summary>
		public void ParseGitStatus()
		{
			string gitOutput = CallProcess("git", "status --short");
			
			// Split on newlines
			List<string> splitOutput =  gitOutput.Split(System.Environment.NewLine.ToCharArray()).ToList();
			
			// Prune empty strings
			splitOutput = splitOutput.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
			
			parsedStatusLog.Clear();
			
			foreach (string stringOutput in splitOutput)
			{
				StatusItem item = new StatusItem();
				item.status = stringOutput;
				item.add = false;
				
				parsedStatusLog.Add(item);
			}
		}
#endregion
		
		/// <summary>
		/// Generic method to call git.
		/// </summary>
		/// <returns>The output from git.</returns>
		/// <param name="fileName"></param>
		/// <param name="arguments"></param>
		public string CallProcess(string fileName, string arguments)
		{
			Process gitProcess = new Process();
			gitProcess.StartInfo.FileName = fileName;
			gitProcess.StartInfo.Arguments = arguments;
			gitProcess.StartInfo.UseShellExecute = false;
			gitProcess.StartInfo.RedirectStandardOutput = true;
			gitProcess.StartInfo.RedirectStandardError = true;
			gitProcess.StartInfo.CreateNoWindow = true;
			
			// http://stackoverflow.com/questions/16803748/how-to-decode-cmd-output-correctly
			// Default = 65533, ASCII = ?, Unicode = nothing works at all, UTF-8 = 65533, UTF-7 = 242 = WORKS!, UTF-32 = nothing works at all
			gitProcess.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(850);
			
			gitProcess.Start();
			
			string gitStatusOutput = gitProcess.StandardOutput.ReadToEnd();
			gitProcess.WaitForExit();

			return gitStatusOutput;
		}
	}
}
