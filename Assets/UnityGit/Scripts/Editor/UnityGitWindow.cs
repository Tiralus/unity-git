

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

		public List<LogItem> parsedGitLog = new List<LogItem>();
		
		public List<StatusItem> parsedStatusLog = new List<StatusItem>();

		public string commitTitle = string.Empty;

		public string commitMessage = string.Empty;

		private bool addAll = false;

		private bool showCommitWindow = false;
		private bool commit = false;

		private bool push = false;
		
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
		/// Creates the alternating background color based upon if the Unity Editor is the free (light) skin or the Pro (dark) skin.
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
		/// Creates the alternating background color based upon if the Unity Editor is the free (light) skin or the Pro (dark) skin.
		/// </summary>
		public void Awake()
		{
			ParseGitLog();
			ParseGitStatus();
		}

		/// <summary>
		/// Creates the alternating background color based upon if the Unity Editor is the free (light) skin or the Pro (dark) skin.
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
                EditorStyles.label.fontStyle = FontStyle.Bold;
                EditorStyles.label.fontSize = 14;
	            EditorGUILayout.LabelField("There is not a repository!", GUILayout.Height(20));
                EditorStyles.label.fontSize = 10;
                EditorStyles.label.fontStyle = FontStyle.Normal;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
		
		/// <summary>
		/// Creates the alternating background color based upon if the Unity Editor is the free (light) skin or the Pro (dark) skin.
		/// </summary>
		public void DisplayGitStatus()
		{
			RunGitCommands();
			
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
						EditorStyles.label.fontStyle = FontStyle.Bold;
						EditorStyles.label.fontSize = 14;
						EditorGUILayout.LabelField(string.Format("{0}", parsedStatusLog[i].status), GUILayout.Height(20));
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
				}
			}
			else
			{
				EditorStyles.label.fontStyle = FontStyle.Bold;
				EditorStyles.label.fontSize = 14;
				EditorGUILayout.LabelField("There are no files modified!", GUILayout.Height(20));
				EditorStyles.label.fontSize = 10;
				EditorStyles.label.fontStyle = FontStyle.Normal;
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}

		public void DisplayStatusHeader()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			{
				addAll = GUILayout.Button("Add All");
				push = GUILayout.Button("Push");
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		
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

		public void RunGitCommands()
		{
			if (addAll)
			{
				CallGitProcess("git", "add -A");
				ParseGitStatus();
				addAll = false;
			}
		}
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

		/// <summary>
		/// Call git and get the output from git log, split it to be easier to display.
		/// </summary>
        public void ParseGitLog()
		{
			string gitLogOutput = CallGitProcess("git", "log --pretty=format:" + "\"" + "%h%x09%an%x09%ad%x09%s" + "\"" + "--date=short");

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
			string gitOutput = CallGitProcess("git", "status --short");
			
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

		public void DisplayCommit()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			commitTitle = EditorGUILayout.TextField("Title: ", commitTitle);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			commitMessage = EditorGUILayout.TextField("Message: ", commitMessage);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			commit = GUILayout.Button("Commit");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			if (commit)
			{
				CallGitProcess("git", "commit -m" + "\"" + commitTitle + "\"" + "-m" + "\"" + commitMessage + "\"");
			}
		}

		/// <summary>
		/// Generic method to call git.
		/// </summary>
		/// <returns>The output from git.</returns>
		/// <param name="fileName"></param>
		/// <param name="arguments"></param>
		public string CallGitProcess(string fileName, string arguments)
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
