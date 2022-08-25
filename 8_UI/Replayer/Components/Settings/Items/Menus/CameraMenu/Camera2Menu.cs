using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.Camera2Menu.bsml")]
    internal class Camera2Menu : MenuWithContainer //not implemented yet
    {
        private class Cam2CellInfo
        {
            public Cam2CellInfo(Camera2Tool.Cam2Info info)
            {
                Info = info;
            }

            public Camera2Tool.Cam2Info Info { get; private set; }

            [UIValue("name-text")] private string name => Info.name;
            [UIValue("fpv-text")] private string isFPV => Info.isFPV ? "FPV" : "Positionable";
        }

        //[Inject] 
        private readonly Camera2Tool _cam2Tool;

        [UIComponent("cameras-list")] private readonly CustomCellListTableData _camerasList;
        [UIObject("scrollbar-container")] private readonly GameObject _scrollbarContainer;

        protected override void OnAfterParse()
        {
            //_cam2Tool.LoadedCameras.ForEach(x => _camerasList.data.Add(new Cam2CellInfo(x)));
            _camerasList.tableView.ReloadData();
            AdjustScrollbar();
        }
        private void AdjustScrollbar()
        {
            var scrollbar = _camerasList.transform.Find("ScrollBar");
            if (scrollbar == null) return;

            var layoutElement = scrollbar.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 4;

            var scrollIndicator = scrollbar.Find("VerticalScrollIndicator");
            if (scrollIndicator == null) return;

            var scrollIndicatorElement = scrollIndicator.gameObject.AddComponent<LayoutElement>();
            scrollIndicatorElement.preferredWidth = 1.6f;

            foreach (var item in scrollbar.GetComponentsInChildren<Button>())
            {
                item.navigation = new Navigation() { mode = Navigation.Mode.None };

                var rect = item.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(4, 0);

                item.transform.localPosition = new Vector2(-2, item.transform.localPosition.y);
            }

            scrollbar.SetParent(_scrollbarContainer.transform);
        }

        [UIAction("cell-selected")] private void OnCellSelected(TableView view, object info)
        {

        }
    }
}
