using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.Replay;

namespace BeatLeader.Utils {
    internal static class ReplayUtils {
        //forked from https://github.com/BeatLeader/beatleader-server/blob/master/Utils/ReplayUtils.cs

        #region Models

        private enum ControllerEnum {
            unknown = 0,
            oculustouch = 1,
            oculustouch2 = 16,
            quest2 = 256,
            vive = 2,

            vivePro = 4,
            wmr = 8,
            odyssey = 9,
            hpMotion = 10,

            picoNeo3 = 33,
            picoNeo2 = 34,
            vivePro2 = 35,
            miramar = 37,
            disco = 44,
            questPro = 61,
            viveTracker = 62,
            viveTracker2 = 63,
            knuckles = 64,
            nolo = 65,
            picophoenix = 66,
            hands = 67,
            viveTracker3 = 68,
            pimax = 69,
            huawei = 70,
            polaris = 71,
            tundra = 72,
            cry = 73,
            e4 = 74,
            gamepad = 75,
            joycon = 76,
            steamdeck = 77,
            viveCosmos = 128,
        }

        private enum HMD {
            unknown = 0,
            rift = 1,
            riftS = 16,
            quest = 32,
            quest2 = 256,
            quest3 = 512,
            quest3s = 513,
            vive = 2,
            vivePro = 4,
            viveCosmos = 128,
            wmr = 8,

            picoNeo3 = 33,
            picoNeo2 = 34,
            vivePro2 = 35,
            viveElite = 36,
            miramar = 37,
            pimax8k = 38,
            pimax5k = 39,
            pimaxArtisan = 40,
            hpReverb = 41,
            samsungWmr = 42,
            qiyuDream = 43,
            disco = 44,
            lenovoExplorer = 45,
            acerWmr = 46,
            viveFocus = 47,
            arpara = 48,
            dellVisor = 49,
            e3 = 50,
            viveDvt = 51,
            glasses20 = 52,
            hedy = 53,
            vaporeon = 54,
            huaweivr = 55,
            asusWmr = 56,
            cloudxr = 57,
            vridge = 58,
            medion = 59,
            picoNeo4 = 60,
            questPro = 61,
            pimaxCrystal = 62,
            e4 = 63,
            index = 64,
            controllable = 65,
            bigscreenbeyond = 66,
            nolosonic = 67,
            hypereal = 68,
            varjoaero = 69,
            psvr2 = 70,
            megane1 = 71
        }

        #endregion

        #region Calculations

        public static Score ComputeScore(Replay replay) {
            var score = new Score();
            var info = replay.info;

            score.id = -1;
            score.baseScore = info.score < 0 ? info.score * -1 : info.score;
            score.modifiers = info.modifiers;
            
            var hmd = HMDFromName(info.hmd);
            score.hmd = (int)hmd;
            score.headsetName = HMDToName(hmd);
            
            score.controller = (int)ControllerFromName(info.controller);
            score.timeSet = info.timestamp;
            score.platform = info.platform + "," + info.gameVersion + "," + info.version;

            score.modifiedScore = score.baseScore;
            score.pp = -1;
            score.wallsHit = replay.walls.Count;

            var firstNoteTime = replay.notes.FirstOrDefault()?.eventTime ?? 0;
            var lastNoteTime = replay.notes.LastOrDefault()?.eventTime ?? 0;
            score.pauses = replay.pauses.Count(p => p.time >= firstNoteTime && p.time <= lastNoteTime);

            foreach (var item in replay.notes) {
                switch (item.eventType) {
                    case NoteEventType.bad:
                        score.badCuts++;
                        break;
                    case NoteEventType.miss:
                        score.missedNotes++;
                        break;
                    case NoteEventType.bomb:
                        score.bombCuts++;
                        break;
                }
            }

            score.fullCombo = score is { bombCuts: 0, missedNotes: 0, wallsHit: 0, badCuts: 0 };
            return score;
        }

        private static string? HMDToName(HMD hmd) {
            return hmd switch {
                HMD.rift => "Oculus Rift",
                HMD.riftS => "Oculus Rift S",
                HMD.quest => "Meta Quest",
                HMD.quest2 => "Meta Quest 2",
                HMD.quest3 => "Meta Quest 3",
                HMD.quest3s => "Meta Quest 3S",
                HMD.questPro => "Meta Quest Pro",
                HMD.vive => "HTC Vive",
                HMD.vivePro => "HTC Vive Pro",
                HMD.vivePro2 => "HTC Vive Pro 2",
                HMD.viveCosmos => "HTC Vive Cosmos",
                HMD.viveElite => "HTC Vive Elite",
                HMD.viveFocus => "HTC Vive Focus",
                HMD.viveDvt => "HTC Vive DVT",
                HMD.wmr => "WMR",
                HMD.hpReverb => "HP Reverb",
                HMD.samsungWmr => "Samsung Odyssey",
                HMD.lenovoExplorer => "Lenovo Explorer",
                HMD.acerWmr => "Acer WMR",
                HMD.dellVisor => "Dell Visor",
                HMD.asusWmr => "ASUS WMR",
                HMD.picoNeo2 => "Pico Neo 2",
                HMD.picoNeo3 => "Pico Neo 3",
                HMD.picoNeo4 => "Pico Neo 4",
                HMD.pimax8k => "Pimax 8K",
                HMD.pimax5k => "Pimax 5K",
                HMD.pimaxArtisan => "Pimax Artisan",
                HMD.pimaxCrystal => "Pimax Crystal",
                HMD.index => "Valve Index",
                HMD.psvr2 => "PlayStation VR2",
                HMD.varjoaero => "Varjo Aero",
                HMD.bigscreenbeyond => "Bigscreen Beyond",
                HMD.controllable => "Controllable",
                _ => null
            };
        }

        private static HMD HMDFromName(string hmdName) {
            var lowerHmd = hmdName.ToLower();

            if (lowerHmd.Contains("pico") && lowerHmd.Contains("4")) return HMD.picoNeo4;
            if (lowerHmd.Contains("pico neo") && lowerHmd.Contains("3")) return HMD.picoNeo3;
            if (lowerHmd.Contains("pico neo") && lowerHmd.Contains("2")) return HMD.picoNeo2;
            if (lowerHmd.Contains("pico")) return HMD.picoNeo4;
            if (lowerHmd.Contains("vive pro 2")) return HMD.vivePro2;
            if (lowerHmd.Contains("vive elite")) return HMD.viveElite;
            if (lowerHmd.Contains("focus3")) return HMD.viveFocus;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("8k")) return HMD.pimax8k;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("5k")) return HMD.pimax5k;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("artisan")) return HMD.pimaxArtisan;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("crystal")) return HMD.pimaxCrystal;

            if (lowerHmd.Contains("controllable")) return HMD.controllable;

            if (lowerHmd.Contains("beyond")) return HMD.bigscreenbeyond;
            if (lowerHmd.Contains("nolo") && lowerHmd.Contains("sonic")) return HMD.nolosonic;
            if (lowerHmd.Contains("hypereal")) return HMD.hypereal;
            if (lowerHmd.Contains("varjo") && lowerHmd.Contains("aero")) return HMD.varjoaero;

            if (lowerHmd.Contains("hp reverb")) return HMD.hpReverb;
            if (lowerHmd.Contains("samsung windows")) return HMD.samsungWmr;
            if (lowerHmd.Contains("qiyu dream")) return HMD.qiyuDream;
            if (lowerHmd.Contains("disco")) return HMD.disco;
            if (lowerHmd.Contains("lenovo explorer")) return HMD.lenovoExplorer;
            if (lowerHmd.Contains("acer")) return HMD.acerWmr;
            if (lowerHmd.Contains("arpara")) return HMD.arpara;
            if (lowerHmd.Contains("dell visor")) return HMD.dellVisor;
            if (lowerHmd.Contains("meganex") && lowerHmd.Contains("1")) return HMD.megane1;

            if (lowerHmd.Contains("e3")) return HMD.e3;
            if (lowerHmd.Contains("e4")) return HMD.e4;

            if (lowerHmd.Contains("vive dvt")) return HMD.viveDvt;
            if (lowerHmd.Contains("3glasses s20")) return HMD.glasses20;
            if (lowerHmd.Contains("hedy")) return HMD.hedy;
            if (lowerHmd.Contains("vaporeon")) return HMD.vaporeon;
            if (lowerHmd.Contains("huaweivr")) return HMD.huaweivr;
            if (lowerHmd.Contains("asus mr0")) return HMD.asusWmr;
            if (lowerHmd.Contains("cloudxr")) return HMD.cloudxr;
            if (lowerHmd.Contains("vridge")) return HMD.vridge;
            if (lowerHmd.Contains("medion mixed reality")) return HMD.medion;

            if (lowerHmd.Contains("quest") && lowerHmd.Contains("3s")) return HMD.quest3s;
            if (lowerHmd.Contains("ventura")) return HMD.quest3s;
            if (lowerHmd.Contains("unknown_panther")) return HMD.quest3s;
            if (lowerHmd.Contains("quest") && lowerHmd.Contains("3")) return HMD.quest3;
            if (lowerHmd.Contains("quest") && lowerHmd.Contains("2")) return HMD.quest2;
            if (lowerHmd.Contains("miramar")) return HMD.quest2;
            if (lowerHmd.Contains("quest") && lowerHmd.Contains("pro")) return HMD.questPro;

            if (lowerHmd.Contains("vive cosmos")) return HMD.viveCosmos;
            if (lowerHmd.Contains("vive_cosmos")) return HMD.viveCosmos;
            if (lowerHmd.Contains("index")) return HMD.index;
            if (lowerHmd.Contains("quest")) return HMD.quest;
            if (lowerHmd.Contains("rift s")) return HMD.riftS;
            if (lowerHmd.Contains("rift_s")) return HMD.riftS;
            if (lowerHmd.Contains("windows")) return HMD.wmr;
            if (lowerHmd.Contains("vive pro")) return HMD.vivePro;
            if (lowerHmd.Contains("vive_pro")) return HMD.vivePro;
            if (lowerHmd.Contains("vive")) return HMD.vive;
            if (lowerHmd.Contains("rift")) return HMD.rift;
            
            if (lowerHmd.Contains("playstation_vr2") || lowerHmd.Contains("ps vr2")) return HMD.psvr2;

            return HMD.unknown;
        }

        private static ControllerEnum ControllerFromName(string controllerName) {
            var lowerController = controllerName.ToLower();

            if (lowerController.Contains("vive tracker") && lowerController.Contains("3")) return ControllerEnum.viveTracker3;
            if (lowerController.Contains("vive tracker") && lowerController.Contains("pro")) return ControllerEnum.viveTracker2;
            if (lowerController.Contains("vive tracker")) return ControllerEnum.viveTracker;

            if (lowerController.Contains("vive") && lowerController.Contains("cosmos")) return ControllerEnum.viveCosmos;
            if (lowerController.Contains("vive") && lowerController.Contains("pro") && lowerController.Contains("2")) return ControllerEnum.vivePro2;
            if (lowerController.Contains("vive") && lowerController.Contains("pro")) return ControllerEnum.vivePro;
            if (lowerController.Contains("vive")) return ControllerEnum.vive;

            if (lowerController.Contains("pico") && lowerController.Contains("phoenix")) return ControllerEnum.picophoenix;
            if (lowerController.Contains("pico neo") && lowerController.Contains("3")) return ControllerEnum.picoNeo3;
            if (lowerController.Contains("pico neo") && lowerController.Contains("2")) return ControllerEnum.picoNeo2;
            if (lowerController.Contains("knuckles")) return ControllerEnum.knuckles;
            if (lowerController.Contains("miramar")) return ControllerEnum.miramar;

            if (lowerController.Contains("gamepad")) return ControllerEnum.gamepad;
            if (lowerController.Contains("joy-con")) return ControllerEnum.joycon;
            if (lowerController.Contains("steam deck")) return ControllerEnum.steamdeck;

            if (lowerController.Contains("quest pro")) return ControllerEnum.questPro;
            if (lowerController.Contains("quest2")) return ControllerEnum.quest2;
            if (lowerController.Contains("oculus touch") || lowerController.Contains("rift cv1")) return ControllerEnum.oculustouch;
            if (lowerController.Contains("rift s") || lowerController.Contains("quest")) return ControllerEnum.oculustouch2;

            if (lowerController.Contains("066a")) return ControllerEnum.hpMotion;
            if (lowerController.Contains("065d")) return ControllerEnum.odyssey;
            if (lowerController.Contains("windows")) return ControllerEnum.wmr;

            if (lowerController.Contains("nolo")) return ControllerEnum.nolo;
            if (lowerController.Contains("disco")) return ControllerEnum.disco;
            if (lowerController.Contains("hands")) return ControllerEnum.hands;

            if (lowerController.Contains("pimax")) return ControllerEnum.pimax;
            if (lowerController.Contains("huawei")) return ControllerEnum.huawei;
            if (lowerController.Contains("polaris")) return ControllerEnum.polaris;
            if (lowerController.Contains("tundra")) return ControllerEnum.tundra;
            if (lowerController.Contains("cry")) return ControllerEnum.cry;
            if (lowerController.Contains("e4")) return ControllerEnum.e4;

            return ControllerEnum.unknown;
        }

        private static float GetPositiveMultiplier(this ModifiersMap modifiersObject, string modifiers) {
            var modifiersMap = modifiersObject.ToDictionary<float>();
            return 1 + modifiersMap.Keys.Where(
                modifier => modifiers.Contains(modifier)
                    && modifiersMap[modifier] > 0
            ).Sum(modifier => modifiersMap[modifier]);
        }

        private static float GetNegativeMultiplier(this ModifiersMap modifiersObject, string modifiers) {
            var modifiersMap = modifiersObject.ToDictionary<float>();
            return 1 + modifiersMap.Keys.Where(
                modifier => modifiers.Contains(modifier)
                    && modifiersMap[modifier] < 0
            ).Sum(modifier => modifiersMap[modifier]);
        }

        #endregion

        #region Extensions

        private static IDictionary<string, T> ToDictionary<T>(this object? source) {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();
            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source)) {
                AddPropertyToDictionary<T>(property, source!, dictionary);
            }
            return dictionary;
        }

        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary) {
            var value = property.GetValue(source);
            if (value is T value1) dictionary.Add(property.Name, value1);
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull() {
            throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
        }

        #endregion
    }
}