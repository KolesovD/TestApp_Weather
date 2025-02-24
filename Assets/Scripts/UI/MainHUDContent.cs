﻿using System;
using TestApp.UI.Elements;
using TestApp.UI.Pages;
using UnityEngine;
using UnityEngine.Assertions;

namespace TestApp.UI
{
    public class MainHUDContent : MonoBehaviour
    {
        [Serializable]
        private struct PageButtonsIterrelation
        {
            public SimpleButton Button;
            public AbstractMenuPage Page;
        }

        [Header("Pages")]
        [SerializeField] private PageButtonsIterrelation[] _pageButtons;

        public void Init()
        {
            Assert.AreNotEqual(0, _pageButtons.Length);

            for (int i = 0; i < _pageButtons.Length; i++)
            {
                int index = i;

                _pageButtons[index].Page.Init();
                _pageButtons[index].Page.gameObject.SetActive(false);
                _pageButtons[index].Button.SetOnClick(() => ShowPage(index));
            }

            ShowPage(0);
        }

        private void ShowPage(int index)
        {
            for (int i = 0; i < _pageButtons.Length; i++)
            {
                if (i == index)
                    _pageButtons[i].Page.Show();
                else
                    _pageButtons[i].Page.Hide();
            }
        }
    }
}
