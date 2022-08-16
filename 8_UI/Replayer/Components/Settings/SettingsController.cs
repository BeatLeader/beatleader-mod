using System;
using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatLeader.Replayer.Movement;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using UnityEngine.XR;
using UnityEngine;
using Zenject;
using HMUI;
using BeatLeader.Replayer.Managers;
using UnityEngine.UI;
using System.Reflection;

namespace BeatLeader.Components.Settings
{
    internal class SettingsController : ReeUIComponentV2WithContainer
    {
        [Inject] private readonly InputManager _inputManager;

        [UIObject("container")] private GameObject _container;
        [UIObject("settings-view")] private GameObject _settingsView;
        [UIObject("settings-container")] private GameObject _settingsContainer;
        [UIObject("sub-menu-container")] private GameObject _subMenuContainer;
        [UIObject("sub-menu-view")] private GameObject _subMenuView;

        private List<Setting> _settings;
        private SubMenu _currentMenu;
        private ModalView _modal;
        private bool _chillinInSubMenu;

        protected override void OnInitialize()
        {
            _modal = _container.GetComponentInParentHierarchy<ModalView>();
            _modal.blockerClickedEvent += NotifySettingsModalClosed;
            _subMenuContainer.SetActive(false);
            _settings = Assembly.GetExecutingAssembly().ScanAndActivateTypes<Setting>();
            _settings.ForEach(x => HandleSetting(x));
        }

        private void PresentMenu(SubMenu menu)
        {
            menu.gameObject.SetActive(true);
            _subMenuContainer.SetActive(true);
            _settingsContainer.SetActive(false);

            _currentMenu = menu;
            _chillinInSubMenu = true;
        }
        private void DismissMenu()
        {
            if (!_chillinInSubMenu || _currentMenu is null) return;

            _currentMenu.gameObject.SetActive(false);
            _subMenuContainer.SetActive(false);
            _settingsContainer.SetActive(true);

            _currentMenu = null;
            _chillinInSubMenu = false;
        }

        private void HandleSetting(Setting setting)
        {
            Container.Inject(setting);
            if (!setting.Handle()) return;

            setting.OnSettingsClosingRequested += NotifySettingsModalClosingRequested;
            var content = setting.ContentTransform;

            if (setting.IsSubMenu)
            {
                var menu = CreateSubMenu(setting);
                var button = CreateSubMenuButton(menu);
                button.OnClick += PresentMenu;
                menu.transform.SetParent(_subMenuView.transform, false);
                menu.gameObject.SetActive(false);
                content = button.transform;
            }

            content.gameObject.AddComponent<InputDependentObject>().Init(_inputManager, setting.AvailableInputs);
            content.SetParent(_settingsView.transform, false);
            if (setting.SettingIndex != -1) content.SetSiblingIndex(setting.SettingIndex);
        }
        private SubMenu CreateSubMenu(Setting setting)
        {
            var menu = new GameObject(setting.GetType().Name).AddComponent<SubMenu>();
            menu.Init(setting);
            return menu;
        }
        private SubMenuButton CreateSubMenuButton(SubMenu menu)
        {
            var button = new GameObject($"{menu.Name}SubMenuButton").AddComponent<SubMenuButton>();
            button.Init(menu);
            return button;
        }

        [UIAction("dismiss-button-clicked")] private void DismissButtonClicked() => DismissMenu();
        private void NotifySettingsModalClosingRequested()
        {
            _modal.Hide(true);
        }
        private void NotifySettingsModalClosed()
        {
            _settings.ForEach(x => x.OnSettingsClosed());
            DismissMenu();
        }
    }
}