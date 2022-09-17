using System;
using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;

namespace BeatLeader.Components.Settings
{
    internal class SettingsController : ReeUIComponentV2WithContainer
    {
        [Inject] private readonly DiContainer _container;

        [UIObject("dismiss-menu-button")] private GameObject _dismissMenuButton;
        [UIObject("menu-view-container")] private GameObject _menuViewContainer;

        public Menu RootMenu
        {
            get => _rootMenu;
            set
            {
                HandleIfNeeded(value);
                if (_rootMenu != null) UnbindEvents(_rootMenu);
                _rootMenu = value;
                SetMenuEnabled(RootMenu);
                BindEvents(value);
            }
        }

        public event Action<bool> OnSettingsCloseRequested;

        private Stack<Menu> _menusHierarchyStack = new();
        private Menu _rootMenu;

        protected override void OnInitialize()
        {
            _dismissMenuButton.SetActive(false);
        }

        public void NotifySettingsClosed()
        {
            _menusHierarchyStack.ToList().ForEach(x => x.OnSettingsClosed());
        }
        private void PresentMenu(Menu menu)
        {
            PresentMenu(menu, true);
        }
        private void PresentMenu(Menu menu, bool keepHierarchy)
        {
            if (!keepHierarchy) _menusHierarchyStack.Clear();

            BindEvents(menu);
            HandleIfNeeded(menu);

            if (RootMenu.ContentTransform.gameObject.activeSelf)
            {
                SetMenuEnabled(RootMenu, false);
                _dismissMenuButton.SetActive(true);
            }

            if (_menusHierarchyStack.TryPeek(out var currentMenu))
                SetMenuEnabled(currentMenu, false);

            menu.OnMenuPresent();
            SetMenuEnabled(menu);
            _menusHierarchyStack.Push(menu);
        }
        private bool DismissMenu()
        {
            bool flag = false;
            if (_menusHierarchyStack.TryPop(out var menu))
            {
                SetMenuEnabled(menu, false);
                UnbindEvents(menu);
                menu.OnMenuDismiss();
                if (_menusHierarchyStack.TryPeek(out var menu2))
                    SetMenuEnabled(menu2);
                flag = true;
            }
            if (_menusHierarchyStack.Count == 0 && !RootMenu.ContentTransform.gameObject.activeSelf)
            {
                _dismissMenuButton.SetActive(false);
                SetMenuEnabled(RootMenu);
                flag = true;
            }
            return flag;
        }
        private void DismissToRootMenu()
        {
            while (_menusHierarchyStack.Count > 0) DismissMenu();
        }

        private void HandleIfNeeded(Menu menu)
        {
            if (menu.IsParsed) return;
            _container.Inject(menu);
            menu.Handle();
        }
        private void BindEvents(Menu menu)
        {
            menu.OnMenuPresentRequested += PresentMenu;
            menu.OnMenuDismissRequested += DismissMenu;
            menu.OnMenuDismissToRootRequested += DismissToRootMenu;
            menu.OnSettingsCloseRequested += NotifySettingsCloseRequested;
        }
        private void UnbindEvents(Menu menu)
        {
            menu.OnMenuPresentRequested -= PresentMenu;
            menu.OnMenuDismissRequested -= DismissMenu;
            menu.OnMenuDismissToRootRequested -= DismissToRootMenu;
            menu.OnSettingsCloseRequested -= NotifySettingsCloseRequested;
        }
        private void SetMenuEnabled(Menu menu, bool enabled = true)
        {
            if (menu == null) return;
            menu.ContentTransform.gameObject.SetActive(enabled);
            menu.ContentTransform.SetParent(enabled ? _menuViewContainer.transform : menu.OriginalParent, false);
        }

        [UIAction("dismiss-button-clicked")] private void DismissButtonClicked() => DismissMenu();
        private void NotifySettingsCloseRequested(bool animated)
        {
            OnSettingsCloseRequested?.Invoke(animated);
        }
    }
}