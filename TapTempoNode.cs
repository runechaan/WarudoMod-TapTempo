// =============================================================================
// TapTempoNode.cs
//
// タップテンポ(BPM計測)ノード。
// Tap フロー入力にキーボードボタン等のイベントノードを接続し、拍に合わせて
// タップすると、直近のタップ時刻から BPM を計算して出力します。
//
// - 1回目のタップは BPM が確定しないため FirstTap へ分岐
// - 2回目以降は BPM を更新して Exit へ分岐
// - ResetThreshold 秒以上タップが途切れると、次のタップから新しい列として扱う
// - Reset フロー入力で手動リセット
//
// BPM の計算:
//   直近のタップ時刻に最小二乗法で直線を当てはめ、その傾きを 1 拍の長さとする。
//   最初と最後の 2 点だけを使う方法より、途中のタップも使う分だけ揺れに強い。
//
// 整数出力のヒステリシス:
//   Round To Integer が ON のとき、表示中の整数から RoundHysteresis + 0.5 以上
//   離れたときだけ値を切り替える。x.5 付近で 119 と 120 を行き来するのを防ぐ。
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Rune.TapTempo {

    [NodeType(
        Id = "29e003b4-e647-4a70-85ca-d3789a92cf55",
        Title = "Tap Tempo",
        Category = "RUNE.MODS")]
    public class TapTempoNode : Node {

        // BPM 計算には最低 2 件のタップが必要
        private const int MinSamples = 2;

        // 整数出力を切り替えるまでの余裕。表示中の整数から 0.5 + この値だけ離れたら切り替える
        private const float RoundHysteresis = 0.3f;

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

        // 長時間起動しても精度が落ちないよう、タップ時刻は double で保持する
        private readonly List<double> _tapTimes = new List<double>();
        private float _bpm;
        private float _roundedBpm;
        private bool _hasRoundedBpm;

        /* DATA OUTPUTS */

        [DataOutput]
        [Label("BPM")]
        public float BPM() => RoundToInteger ? _roundedBpm : _bpm;

        [DataOutput]
        [Label("Tap Count")]
        public int TapCount() => _tapTimes.Count;

        /* FLOW INPUTS */

        // このポートにキーボードボタンなど任意のイベントノードを接続してタップさせる
        [FlowInput]
        public Continuation Tap() {
            double now = Time.unscaledTimeAsDouble;

            // 間隔が空きすぎていたら、新しいタップ列として扱う
            if (_tapTimes.Count > 0 && now - _tapTimes[_tapTimes.Count - 1] > ResetThreshold) {
                _tapTimes.Clear();
                _hasRoundedBpm = false; // 新しい列では整数出力もすぐ追従させる
            }

            _tapTimes.Add(now);

            // 実行中に MaxSamples を小さくされても、古いサンプルをまとめて捨てる
            int limit = Mathf.Max(MinSamples, MaxSamples);
            while (_tapTimes.Count > limit) {
                _tapTimes.RemoveAt(0);
            }

            // 1 回目のタップは BPM を計算できないので、別の出力に流す
            if (_tapTimes.Count < MinSamples) {
                return FirstTap;
            }

            double beat = EstimateBeatLength();
            if (beat > 0.0) {
                _bpm = (float) (60.0 / beat);
                UpdateRoundedBpm();
            }
            return Exit;
        }

        // タップ列を手動でリセットしたいときに使う
        [FlowInput]
        public Continuation Reset() {
            _tapTimes.Clear();
            _bpm = 0f;
            _roundedBpm = 0f;
            _hasRoundedBpm = false;
            return null;
        }

        /* FLOW OUTPUTS */

        [FlowOutput]
        public Continuation Exit; // BPM が更新されたときに発火

        [FlowOutput]
        [Label("First Tap")]
        public Continuation FirstTap; // 1 回目のタップのとき発火（まだ BPM 未確定）

        /* CALCULATION */

        // タップ番号 i と時刻 t に最小二乗法で直線 t = a + b*i を当てはめ、傾き b（1 拍の秒数）を返す。
        // タップが 2 件のときは、単純な間隔と同じ値になる。
        private double EstimateBeatLength() {
            int n = _tapTimes.Count;
            double meanIndex = (n - 1) * 0.5;

            double meanTime = 0.0;
            for (int i = 0; i < n; i++) {
                meanTime += _tapTimes[i];
            }
            meanTime /= n;

            double covariance = 0.0;
            double variance = 0.0;
            for (int i = 0; i < n; i++) {
                double dx = i - meanIndex;
                covariance += dx * (_tapTimes[i] - meanTime);
                variance += dx * dx;
            }
            return covariance / variance;
        }

        // 整数出力を更新する。表示中の値から十分離れたときだけ切り替える
        private void UpdateRoundedBpm() {
            if (!_hasRoundedBpm || Mathf.Abs(_bpm - _roundedBpm) > 0.5f + RoundHysteresis) {
                _roundedBpm = Mathf.Round(_bpm);
                _hasRoundedBpm = true;
            }
        }
    }
}
