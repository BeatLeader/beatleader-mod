using BeatLeader.Replays.Managers;
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

namespace BeatLeader.Components.Settings
{
    internal abstract class Setting : INotifyPropertyChanged
    {
        public Setting() { }

        public virtual bool IsSubMenu => false;
        public virtual string SubMenuName => GetType().Name.Replace("Setting", "");
        public virtual int SettingIndex => -1;
        public virtual InputManager.InputType AvailableInputs => InputManager.InputType.FPFC | InputManager.InputType.VR;
        public Transform ContentTransform { get; protected set; }

        public event Action OnSettingsClosingRequested;

        #region Events

        public virtual void OnSettingsClosed() { }
        protected virtual void OnAfterHandling() { }
        protected virtual void OnBeforeHandling() { }

        #endregion

        public virtual bool Handle()
        {
            OnBeforeHandling();
            bool flag = Parse();
            if (flag) OnAfterHandling();
            return flag;
        }
        protected bool TryReadBSMLContent(out string content)
        {
            Type type = GetType();
            content = string.Empty;
            ViewDefinitionAttribute viewDefinitionAttribute;

            if ((viewDefinitionAttribute = type.GetCustomAttribute(typeof(ViewDefinitionAttribute)) as ViewDefinitionAttribute) != null)
            {
                content = Utilities.GetResourceContent(type.Assembly, viewDefinitionAttribute.Definition);
                return true;
            }
            return false;
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
                Plugin.Log.Warn($"Unable to load {GetType().Name} setting! {ex}");
                return false;
            }

            ContentTransform = container.transform.GetChild(0);
            ContentTransform.SetParent(null);
            GameObject.Destroy(container);

            return true;
        }
        protected void CloseSettings() => OnSettingsClosingRequested?.Invoke();

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
