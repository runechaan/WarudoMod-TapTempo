# WarudoMod-TapTempo

Warudo のブループリントにタップテンポ(BPM 計測)ノードを追加するプラグイン MOD です。

A Warudo plugin mod that adds a **Tap Tempo** node for measuring BPM from tap events in the blueprint.

---

## 日本語

### 概要

キーボードボタンなどのイベントノードを `Tap` に接続し、拍に合わせてタップすると、直近のタップ間隔の平均から BPM を計算して出力します。

### ノード仕様

ノードパレットのカテゴリ「RUNE.MODS」に「Tap Tempo」として表示されます。

| 種別 | 名前 | 説明 |
|---|---|---|
| Flow In | `Tap` | タップ入力。2 回目以降は `Exit`、1 回目は `First Tap` へ分岐 |
| Flow In | `Reset` | タップ列と BPM を手動でリセット |
| Data In | `Max Samples` (2–32, 既定 8) | 平均に使う直近タップ数。大きいほど安定するが反応が遅い |
| Data In | `Reset After (sec)` (0.5–10, 既定 2) | この秒数以上タップが途切れたら、次のタップから新しい列として扱う |
| Data In | `Round To Integer` | ON で BPM を整数に丸めて出力(型は float のまま) |
| Data Out | `BPM` (float) | 計算された BPM |
| Data Out | `Tap Count` (int) | 現在保持しているタップ数 |
| Flow Out | `Exit` | BPM が更新されたときに発火 |
| Flow Out | `First Tap` | 1 回目のタップ(BPM 未確定)のときに発火 |

### ファイル構成

- `TapTempoPlugin.cs` — プラグインのエントリポイント
- `TapTempoNode.cs` — ノード本体

---

## English

### Overview

Connect any event node (e.g. a keyboard button) to `Tap` and tap along with the beat. The node averages the most recent tap intervals and outputs the BPM.

### Node reference

The node appears as **Tap Tempo** under the **RUNE.MODS** category.

| Kind | Name | Description |
|---|---|---|
| Flow In | `Tap` | Tap input. Continues to `Exit` from the 2nd tap on, or `First Tap` on the 1st |
| Flow In | `Reset` | Manually clears the tap history and BPM |
| Data In | `Max Samples` (2–32, default 8) | Number of recent taps to average. Higher is steadier but slower to react |
| Data In | `Reset After (sec)` (0.5–10, default 2) | If taps stop for longer than this, the next tap starts a new sequence |
| Data In | `Round To Integer` | Output BPM rounded to an integer (still a float) |
| Data Out | `BPM` (float) | Computed BPM |
| Data Out | `Tap Count` (int) | Taps currently held |
| Flow Out | `Exit` | Fires when BPM is updated |
| Flow Out | `First Tap` | Fires on the first tap (BPM not yet available) |

### Files

- `TapTempoPlugin.cs` — plugin entry point
- `TapTempoNode.cs` — the node itself

## License

MIT License. See [LICENSE](LICENSE).
