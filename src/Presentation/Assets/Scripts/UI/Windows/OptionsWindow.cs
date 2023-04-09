using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;

public class OptionsWindow : MonoBehaviour
{

	public CanvasGroup uiCanvasGroup;

	public static bool isMenu; // активно меню или нет
	public static float soundValue;	// настройка звука
	public static float musicValue;

	// для элементов UI
	public Scrollbar music;
	public Scrollbar sound;

	public void OpenMenu()
	{
		gameObject.SetActive(true);
		DarkestDungeonManager.GamePaused = true;
		uiCanvasGroup.blocksRaycasts = false;
	}

	// Графика
	public void Graphics()
	{
		
	}

	// Аудио
	public void Audio()
	{
		AudioListener.volume = 0.1f; // не пашет
		soundValue = 0.5f;
		musicValue = 0.5f;
	}

	// Другие
	public void Other()
	{
		Time.timeScale = 8.5f; // изменение скорости игры пашет
	}
}
