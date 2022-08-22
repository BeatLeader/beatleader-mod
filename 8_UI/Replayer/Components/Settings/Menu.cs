using BeatLeader.Replayer.Managers;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using UnityEngine;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.ComponentModel;
using Zenject;

namespace BeatLeader.Components.Settings
{
    internal abstract class Menu : INotifyPropertyChanged
    {
        public virtual Transform ContentTransform { get; protected set; }
        public virtual Transform OriginalParent { get; protected set; }
        public virtual bool ParseOnConstructor => true;
        public bool IsParsed { get; protected set; }

        public event Action<bool> OnSettingsCloseRequested;
        public event Action<Menu> OnMenuPresentRequested;
        public event Func<bool> OnMenuDismissRequested;
        public event Action OnMenuDismissToRootRequested;

        public static SubMenuButton CreateButtonForMenu(Menu parent, Menu child, string text)
        {
            var button = ReeUIComponentV2.Instantiate<SubMenuButton>(null);
            button.Init(child, text);
            button.OnClick += parent.PresentMenu;
            return button;
        }
        public static Menu Instantiate(Type type)
        {
            var menu = (Menu)Activator.CreateInstance(type);
            menu.OnInstantiate();
            return menu;
        }
        public static T Instantiate<T>() where T : Menu
        {
            return (T)Instantiate(typeof(T));
        }

        #region Parsing

        public virtual bool Handle()
        {
            OnBeforeParse();
            bool flag = Parse();
            if (flag)
            {
                OnAfterParse();
                IsParsed = true;
            }
            return flag;
        }
        protected bool TryReadBSMLContent(out string content)
        {
            return (content = GetType().ReadViewDefinition()) != String.Empty;
        }
        protected bool Parse()
        {
            if (!TryReadBSMLContent(out var content)) return false;
            var container = new GameObject($"{GetType().Name}Container");
            try
            {
                BSMLParser.instance.Parse(content, container, this);
            }
            catch (Exception ex)
            {
                GameObject.Destroy(container);
                Plugin.Log.Warn($"Unable to load {GetType().Name} setting!");
                Plugin.Log.Critical(ex);
                return false;
            }

            ContentTransform = container.transform.GetChild(0);
            ContentTransform.SetParent(null);
            ContentTransform.gameObject.SetActive(false);
            GameObject.Destroy(container);

            return true;
        }

        #endregion

        #region Events

        public virtual void OnSettingsClosed() => DismissToRootMenu();
        protected virtual void OnInstantiate() => Handle();
        protected virtual void OnBeforeParse() { }
        protected virtual void OnAfterParse() { }

        #endregion

        #region Menus

        protected void PresentMenu(Menu menu) => OnMenuPresentRequested?.Invoke(menu);
        protected void DismissMenu() => OnMenuDismissRequested?.Invoke();
        protected void DismissToRootMenu() => OnMenuDismissToRootRequested?.Invoke();
        protected void CloseSettings(bool animated = false) => OnSettingsCloseRequested?.Invoke(animated);

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
