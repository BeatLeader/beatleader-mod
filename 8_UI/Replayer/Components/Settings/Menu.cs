using System;
using BeatLeader.Utils;
using UnityEngine;
using BeatSaberMarkupLanguage;
using System.ComponentModel;

namespace BeatLeader.Components.Settings
{
    internal abstract class Menu : INotifyPropertyChanged
    {
        public virtual Transform ContentTransform { get; protected set; }
        public virtual Transform OriginalParent { get; protected set; }
        public virtual bool ParseOnConstructor => true;
        public bool IsParsed { get; protected set; }

        public event Action<bool> SettingsCloseRequestedEvent;
        public event Action<Menu> MenuPresentRequestedEvent;
        public event Func<bool> MenuDismissRequestedEvent;
        public event Action MenuDismissToRootRequestedEvent;

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
        public virtual void OnMenuPresent() { }
        public virtual void OnMenuDismiss() { }
        protected virtual void OnInstantiate() => Handle();
        protected virtual void OnBeforeParse() { }
        protected virtual void OnAfterParse() { }

        #endregion

        #region Menus

        protected void PresentMenu(Menu menu) => MenuPresentRequestedEvent?.Invoke(menu);
        protected void DismissMenu() => MenuDismissRequestedEvent?.Invoke();
        protected void DismissToRootMenu() => MenuDismissToRootRequestedEvent?.Invoke();
        protected void CloseSettings(bool animated = false) => SettingsCloseRequestedEvent?.Invoke(animated);

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
