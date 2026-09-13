// =============================================================================
// TapTempoNode.cs
//
// タップテンポ(BPM計測)ノード。
// Tap フロー入力にキーボードボタン等のイベントノードを接続し、拍に合わせて
// タップすると、直近のタップ間隔の平均から BPM を計算して出力します。
//
// - 1回目のタップは BPM が確定しないため FirstTap へ分岐
// - 2回目以降は BPM を更新して Exit へ分岐
// - ResetThreshold 秒以上タップが途切れると、次のタップから新しい列として扱う
// - Reset フロー入力で手動リセット
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Rune.TapTempo {

    [NodeType(
        Id = "29e003b4-e647-4a70-85ca-d3789a92cf55",
        Title = "Tap Tempo",
        Category = "RUNEMODS")]
    public class TapTempoNode : Node {

        // 平均を取る直近タップ数。BPM 計算には最低 2 件必要
        private const int MinSamples = 2;

        /* DATA INPUTS */

        [DataInput]
        [Label("Max Samples")]
        [IntegerSlider(MinSamples, 32)]
        public int MaxSamples = 8; // 大きいほど安定するが反応が遅くなる

        [DataInput]
        [Label("Reset After (sec)")]
        [FloatSlider(0.5f, 10f)]
        public float ResetThreshold = 2f; // これ以上タップの間隔が空いたら、タップ列をリセットする

        [DataInput]
        [Label("Round To Integer")]
        public bool RoundToInteger = false; // ON にすると BPM を整数に丸めた値を float として出力する

        /* INTERNAL STATE */

        private readonly List<float> _tapTimes = new List<float>();
        private float _bpm;

        /* DATA OUTPUTS */

        [DataOutput]
        [Label("BPM")]
        public float BPM() => RoundToInteger ? Mathf.Round(_bpm) : _bpm;

        [DataOutput]
        [Label("Tap Count")]
        public int TapCount() => _tapTimes.Count;

        /* FLOW INPUTS */

        // このポートにキーボードボタンなど任意のイベントノードを接続してタップさせる
        [FlowInput]
        public Continuation Tap() {
            float now = Time.unscaledTime;

            // 間隔が空きすぎていたら、新しいタップ列として扱う
            if (_tapTimes.Count > 0 && now - _tapTimes[_tapTimes.Count - 1] > ResetThreshold) {
                _tapTimes.Clear();
            }

            _tapTimes.Add(now);

            // 実行中に MaxSamples を小さくされても、古いサンプルをまとめて捨てる
            int limit = Mathf.Max(MinSamples, MaxSamples);
            while (_tapTimes.Count > limit) {
                _tapTimes.RemoveAt(0);
            }

            // タップが 2 回以上たまったら、直近の平均間隔から BPM を計算
            if (_tapTimes.Count >= MinSamples) {
                float totalInterval = _tapTimes[_tapTimes.Count - 1] - _tapTimes[0];
                float averageInterval = totalInterval / (_tapTimes.Count - 1);
                _bpm = 60f / averageInterval;
                return Exit;
            }

            // 1 回目のタップは BPM を計算できないので、別の出力に流す
            return FirstTap;
        }

        // タップ列を手動でリセットしたいときに使う
        [FlowInput]
        public Continuation Reset() {
            _tapTimes.Clear();
            _bpm = 0f;
            return null;
        }

        /* FLOW OUTPUTS */

        [FlowOutput]
        public Continuation Exit; // BPM が更新されたときに発火

        [FlowOutput]
        [Label("First Tap")]
        public Continuation FirstTap; // 1 回目のタップのとき発火（まだ BPM 未確定）
    }
}
