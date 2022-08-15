using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using BeatLeader.Replays;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.ViewControllers
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerPCView.bsml")]
    internal class ReplayerPCViewController : MonoBehaviour
    {
        [Inject] private readonly DiContainer _container;
        [Inject] private readonly UI2DManager _2DManager;
        [Inject] private readonly Score _score;

        [UIValue("height")] private int Height => Mathf.CeilToInt(_2DManager.CanvasSize.y / ScaleFactor);
        [UIValue("width")] private int Width => Mathf.CeilToInt(_2DManager.CanvasSize.x / ScaleFactor);

        [UIValue("song-info")] private SongInfo _songInfo;
        [UIValue("toolbar")] private Toolbar _toolbar;
        [UIValue("layout-editor")] private LayoutEditor _layoutEditor;

        private const int ScaleFactor = 5;

        private void Start()
        {
            _layoutEditor = ReeUIComponentV2WithContainer.InstantiateInContainer<LayoutEditor>(_container, transform);
            _songInfo = ReeUIComponentV2WithContainer.InstantiateInContainer<SongInfo>(_container, transform);
            _toolbar = ReeUIComponentV2WithContainer.InstantiateInContainer<Toolbar>(_container, transform);

            gameObject.GetOrAddComponent<RectTransform>().sizeDelta = _2DManager.CanvasSize / ScaleFactor;
            BSMLParser.instance.Parse(Utilities.GetResourceContent(GetType().Assembly, 
                GetType().GetCustomAttribute<ViewDefinitionAttribute>().Definition), gameObject, this);
        }
        [UIAction("#post-parse")] private void OnInitialize()
        {
            _layoutEditor.TryAddObject("SongInfo", _songInfo.Root);
            _layoutEditor.TryAddObject("Toolbar", _toolbar.Root, true, true, true);
            _2DManager.AddObject(gameObject);
            StartCoroutine(ForceLayoutRecalculation());
        }

        private IEnumerator ForceLayoutRecalculation() { //ёбаный костыль, но работает
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.MarkLayoutForRebuild(GetComponentsInChildren<RectTransform>()[1]);
        }
    }
}
