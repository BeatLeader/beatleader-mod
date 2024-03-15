using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LayoutEditorComponentsList : ReeListComponentBase<LayoutEditorComponentsList, ILayoutComponent, LayoutEditorComponentsList.Cell> {
        #region Cell
        
        public class Cell : ReeTableCell<Cell, ILayoutComponent> {
            #region UI Components

            [UIComponent("background-image"), UsedImplicitly]
            private AdvancedImage _backgroundImage = null!;
            
            [UIComponent("component-state-button"), UsedImplicitly]
            private ImageButton _componentStateButton = null!;
            
            [UIComponent("component-name"), UsedImplicitly]
            private TMP_Text _componentNameText = null!;
            
            [UIComponent("component-layer"), UsedImplicitly]
            private TMP_Text _componentLayerText = null!;

            #endregion

            #region Setup
            
            protected override string Markup { get; } = BSMLUtility.ReadMarkupOrFallback(
                "LayoutEditorComponentsListCell", Assembly.GetExecutingAssembly()
            );
            
            private ILayoutComponent _layoutComponent = null!;

            protected override void Init(ILayoutComponent component) {
                _layoutComponent = component;
                _componentNameText.text = component.ComponentName;
                _componentLayerText.text = component.ComponentController.ComponentLayer.ToString();
                _componentStateButton.Click(component.ComponentController.ComponentActive);
            }

            #endregion

            #region Colors

            private static readonly Color baseColor = new(0.3f, 0.3f, 0.3f);
            private static readonly Color selectedColor = Color.cyan;
            
            private static readonly Color baseTextColor = Color.white;
            private static readonly Color selectedTextColor = Color.black;

            private void RefreshColors(bool selected) {
                _backgroundImage.Color = selected ? selectedColor : baseColor;
                var textColor = selected ? selectedTextColor : baseTextColor;
                _componentNameText.color = textColor;
                _componentLayerText.color = textColor;
            }

            #endregion

            #region Callbacks

            public override void OnStateChange(bool selected, bool highlighted) {
                RefreshColors(selected);
            }

            [UIAction("visibility-button-click"), UsedImplicitly]
            private void HandleVisibilityButtonClicked(bool state) {
                _layoutComponent.ComponentController.ComponentActive = state;
            }

            #endregion
        }

        #endregion

        #region Sorting

        private class ComponentComparator : IComparer<ILayoutComponent> {
            public int Compare(ILayoutComponent x, ILayoutComponent y) => Comparer<int>.Default
                .Compare(y.ComponentController.ComponentLayer, x.ComponentController.ComponentLayer);
        }

        private readonly ComponentComparator _componentComparator = new();

        private void RefreshSorting() {
            items.Sort(_componentComparator);
        }
        
        #endregion

        #region Setup

        protected override float CellSize => 9.5f;
        
        protected override void OnEarlyRefresh() {
            RefreshSorting();
        }

        #endregion
    }
}