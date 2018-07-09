

namespace UnityGit
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;

	public class UnityGitWindow : EditorWindow
	{
		[MenuItem("Window/Unity Git")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(UnityGitWindow));
		}

		public void OnGUI()
		{
			
		}
	}
}
