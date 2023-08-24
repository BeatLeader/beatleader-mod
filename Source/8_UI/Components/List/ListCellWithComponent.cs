using HMUI;
using UnityEngine;

namespace BeatLeader.Components {
    /// <summary>
    /// TableCell wrapper for ReeUIComponentV3
    /// </summary>
    /// <typeparam name="TData">Initialization data type</typeparam>
    /// <typeparam name="TComponent">ReeUIComponentV3 to wrap</typeparam>
    internal abstract class ListCellWithComponent<TData, TComponent> : TableCell
        where TComponent : ReeUIComponentV3<TComponent>, ListCellWithComponent<TData, TComponent>.IComponent {

        public interface IComponent {
            void Init(TData data);
        }

        public static readonly string CellName = $"{typeof(TComponent).Name}Cell";
        private TComponent _component = null!;

        public void Init(TData data) {
            _component.Init(data);
        }

        private void Awake() {
            _component = ReeUIComponentV3<TComponent>.Instantiate(transform);
        }

        public static T InstantiateCell<T>(
            Transform? parent = null
        ) where T : ListCellWithComponent<TData, TComponent> {
            var go = new GameObject(CellName);
            go.transform.SetParent(parent);
            var cell = go.AddComponent<T>();
            cell.reuseIdentifier = CellName;
            return cell;
        }
        
        public static T InstantiateCell<T>(
            TData data,
            Transform? parent = null
        ) where T : ListCellWithComponent<TData, TComponent> {
            var cell = InstantiateCell<T>(parent);
            cell.Init(data);
            return cell;
        }
    }
}