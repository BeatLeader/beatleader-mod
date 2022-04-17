using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleSwingRatingCounter : ISaberSwingRatingCounter
    {
        protected readonly LazyCopyHashSet<ISaberSwingRatingCounterDidChangeReceiver> _didChangeReceivers = new LazyCopyHashSet<ISaberSwingRatingCounterDidChangeReceiver>();
        protected readonly LazyCopyHashSet<ISaberSwingRatingCounterDidFinishReceiver> _didFinishReceivers = new LazyCopyHashSet<ISaberSwingRatingCounterDidFinishReceiver>();

        protected float _beforeCutRating;
        protected float _afterCutRating;
        protected bool _finished;

        public float beforeCutRating => _beforeCutRating;
        public float afterCutRating => _afterCutRating;
        public bool finished => _finished;

        public void Finish()
        {
            List<ISaberSwingRatingCounterDidFinishReceiver> items = _didFinishReceivers.items;
            for (int num = items.Count - 1; num >= 0; num--)
            {
                items[num].HandleSaberSwingRatingCounterDidFinish(this);
            }
            _finished = true;
        }
        public void Change()
        {
            List<ISaberSwingRatingCounterDidChangeReceiver> items = _didChangeReceivers.items;
            for (int num2 = items.Count - 1; num2 >= 0; num2--)
            {
                items[num2].HandleSaberSwingRatingCounterDidChange(this, _afterCutRating);
            }
        }
        public void Refresh()
        {
            Change();
            Finish();
        }
        public void Init(float beforeCutRating, float afterCutRating)
        {
            _finished = false;
            _beforeCutRating = beforeCutRating;
            _afterCutRating = afterCutRating;
        }
        public void RegisterDidChangeReceiver(ISaberSwingRatingCounterDidChangeReceiver receiver) 
        {
            _didChangeReceivers.Add(receiver);
        }
        public void RegisterDidFinishReceiver(ISaberSwingRatingCounterDidFinishReceiver receiver) 
        { 
            _didFinishReceivers.Add(receiver);
        }
        public void UnregisterDidChangeReceiver(ISaberSwingRatingCounterDidChangeReceiver receiver) 
        { 
            _didChangeReceivers.Remove(receiver);
        }
        public void UnregisterDidFinishReceiver(ISaberSwingRatingCounterDidFinishReceiver receiver) 
        {
            _didFinishReceivers.Remove(receiver);
        }
    }
}
