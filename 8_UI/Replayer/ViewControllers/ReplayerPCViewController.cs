using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using BeatLeader.Replayer;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.ViewControllers
{
    internal class ReplayerPCViewController : MonoBehaviour
    {
        [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerPCView.bsml")]
        private class View : ReeUIComponentV2WithContainer
        {
            [UIValue("song-info")] private HorizontalSongInfo _songInfo;
            [UIValue("player-info")] private HorizontalPlayerInfo _playerInfo;
            [UIValue("toolbar")] private Toolbar _toolbar;
            [UIValue("layout-editor")] private LayoutEditor _layoutEditor;

            protected override void OnInstantiate()
            {
                _layoutEditor = InstantiateInContainer<LayoutEditor>(Container, transform);
                _songInfo = InstantiateInContainer<HorizontalSongInfo>(Container, transform);
                _playerInfo = InstantiateInContainer<HorizontalPlayerInfo>(Container, transform);
                _toolbar = InstantiateInContainer<Toolbar>(Container, transform);
            }
            protected override void OnInitialize()
            {
                _layoutEditor.TryAddObject(_songInfo);
                _layoutEditor.TryAddObject(_toolbar);
            }
        }

        private const string Content = "<horizontal vertical-fit=\"Unconstrained\"><vertical horizontal-fit=\"Unconstrained\"><macro.as-host host=\"content-view\"><macro.reparent transform=\"ui-component\"/></macro.as-host></vertical></horizontal>";

        [Inject] private readonly UI2DManager _2DManager;
        [Inject] private readonly DiContainer _container;

        [UIValue("content-view")] private View _view;

        private void Start()
        {
            _view = ReeUIComponentV2WithContainer.InstantiateInContainer<View>(_container, transform);
            this.ParseInObjectHierarchy(Content);
            _2DManager.AddObject(transform.GetChild(0).gameObject);
        }
    }
}
