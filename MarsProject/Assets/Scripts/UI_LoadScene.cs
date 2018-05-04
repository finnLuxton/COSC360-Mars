using UnityEngine;
using UnityEngine.SceneManagement;
 
public class UI_LoadScene : MonoBehaviour
{
    // Remembers the last selected character in the bio section,
    // in case the screen changes
    public static int lastBioLoaded = 0;

    // Load another scene (duh!)
    public void LoadAnotherScene(string sceneName)
    {
        if(sceneName.Equals("UI_SplashScreen_Intro")){
            UI_Music.PlayIntroMusic();
        }
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNewGame() {
        GameMaster.IN_GAME = true;
        GameMaster.Init(true);
    }

    public void PlayGameMusic(){
        UI_Music.PlayGameMusic();
    }

    public void LoadStopMusic(){
        UI_Music.StopMusic();
    }
    public void easy (){
		GameMaster.difficulty = 25;
        Debug.Log("easy worked");

	}
	public void medium(){
		GameMaster.difficulty = 30;
        Debug.Log("Medium Worked");
 
	}

	public void hard(){
		GameMaster.difficulty = 35;
        Debug.Log("Hard Worked");

	}

}
