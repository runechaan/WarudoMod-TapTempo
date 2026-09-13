// =============================================================================
// TapTempoPlugin.cs
//
// 配布用プラグイン MOD（.warudo）のエントリポイント。
// ブループリント上でタップテンポ(BPM計測)を行う TapTempoNode を提供します。
//
// Warudo SDK（Unity）でのビルド時にのみ使用します。
// Playground 版の TapTempoNode.cs と同時に置かないでください（ノード型が二重登録になります）。
// =============================================================================

using Warudo.Core.Attributes;
using Warudo.Core.Plugins;

namespace Rune.TapTempo {

    [PluginType(
        Id = "rune.taptempo",
        Name = "Tap Tempo",
        Description = "Adds a Tap Tempo node that measures BPM from tap events in the blueprint.",
        Version = "1.0.0",
        Author = "RUNE.",
        NodeTypes = new[] {
            typeof(TapTempoNode)
        })]
    public class TapTempoPlugin : Plugin {
    }
}
