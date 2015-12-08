﻿using UnityEngine;
using UnityEngine.UI;

using InputKeys = System.Collections.Generic.Dictionary<string, System.Func<bool>>;

namespace Assets.MVC.View.Android
{   
    public class GameInitializerAndroidScript : MonoBehaviour
    {
        public AudioSource BackgroundMusic;
        public Toggle MuteToggle;

        private PlayerController _player;

        private readonly InputKeys _inputKeys = 
        new InputKeys
        {
            { "Left", TouchControls.LeftShift },
            { "Right", TouchControls.RightShift },
            { "Down", TouchControls.DownwardShift },
            { "Drop", TouchControls.DoubleTouch },
            { "RotateCW", TouchControls.UpwardShipt },
            { "RotateCCW", () => false }
        };

        void Start()
        {
            if (BackgroundMusic != null)
            {
                BackgroundMusic = Instantiate(BackgroundMusic);
                BackgroundMusic.Play();
            }
            MuteToggle.onValueChanged.AddListener(isMute => BackgroundMusic.mute = isMute);

            _player = new PlayerController(_inputKeys, "MainMenu_android");
            _player.GameView.NewGame(Vector3.zero);
        }

        void Update()
        {
            _player.HandleEvents();
        }

        void OnGUI()
        {
            //TODO: View Score and Level as child of Players boards
            GUI.Label(new Rect(5, 2, 100, 50), "Score:\n  " + _player.GameModel.Score);
            GUI.Label(new Rect(105, 2, 100, 50), "Level:\n  " + _player.GameModel.Level);
        }
    }

}
