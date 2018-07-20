


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
		private readonly string[] tabTitles = { "Log", "Status" };

		public List<string> parsedGitLog;
		
		public List<string> parsedStatusLog;
		
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

		
		[MenuItem("Window/Unity Git")]
		public static UnityGitWindow GetWindow()
		{
			var window = GetWindow<UnityGitWindow>();
			window.titleContent = new GUIContent("Untiy Git");
			window.Focus();
			window.Repaint();
			return window;
		}

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
			}
			this.Repaint();
		}

		public void Awake()
		{
			ParseGitLog();
			ParseGitStatus();
		}


		public void DisplayGitLog()
		{
            // display all of the installed packages
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical();

            GUIStyle style = CreateColoredBackground();

            if (parsedGitLog != null && parsedGitLog.Count > 0)
            {
                for (int i = 0; i < parsedGitLog.Count; i++)
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
                        EditorGUILayout.LabelField(string.Format("{0}", parsedGitLog[i]),GUILayout.Height(20));
                    }
                    EditorGUILayout.EndHorizontal();
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

		public void DisplayGitStatus()
		{
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
						EditorGUILayout.LabelField(string.Format("{0}", parsedStatusLog[i]),GUILayout.Height(20));
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

        public void ParseGitLog()
		{
			Process gitlog = new Process();
			gitlog.StartInfo.FileName = "git";
			gitlog.StartInfo.Arguments = "log --pretty=format:" + "\"" + "%h%x09%an%x09%ad%x09%s" + "\"" + "--date=short";
			gitlog.StartInfo.UseShellExecute = false;
			gitlog.StartInfo.RedirectStandardOutput = true;
			gitlog.StartInfo.RedirectStandardError = true;
			gitlog.StartInfo.CreateNoWindow = true;
			
			// http://stackoverflow.com/questions/16803748/how-to-decode-cmd-output-correctly
			// Default = 65533, ASCII = ?, Unicode = nothing works at all, UTF-8 = 65533, UTF-7 = 242 = WORKS!, UTF-32 = nothing works at all
			gitlog.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(850);
			
			gitlog.Start();
	
			string gitLogOutput = gitlog.StandardOutput.ReadToEnd();
			gitlog.WaitForExit();
			gitLogOutput = gitLogOutput.Replace("--date=short", "");
			parsedGitLog = gitLogOutput.Split(System.Environment.NewLine.ToCharArray()).ToList();
		}
		
		public void ParseGitStatus()
		{
			Process gitlog = new Process();
			gitlog.StartInfo.FileName = "git";
			gitlog.StartInfo.Arguments = "status --short";
			gitlog.StartInfo.UseShellExecute = false;
			gitlog.StartInfo.RedirectStandardOutput = true;
			gitlog.StartInfo.RedirectStandardError = true;
			gitlog.StartInfo.CreateNoWindow = true;
			
			// http://stackoverflow.com/questions/16803748/how-to-decode-cmd-output-correctly
			// Default = 65533, ASCII = ?, Unicode = nothing works at all, UTF-8 = 65533, UTF-7 = 242 = WORKS!, UTF-32 = nothing works at all
			gitlog.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(850);
			
			gitlog.Start();
	
			string gitStatusOutput = gitlog.StandardOutput.ReadToEnd();
			gitlog.WaitForExit();
			
			parsedStatusLog = gitStatusOutput.Split(System.Environment.NewLine.ToCharArray()).ToList();
		}
	}
}
