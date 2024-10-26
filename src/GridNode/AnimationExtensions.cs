namespace Vertex.GridNode;

using System;
using Godot;

public static class AnimationExtensions {
  public static void TrackSetFirstKeyValue(this Animation animation, int trackIndex, Variant value) =>
    animation.TrackSetKeyValue(trackIndex, 0, value);

  public static void TrackSetLastKeyValue(this Animation animation, int trackIndex, Variant value) {
    var lastKey = animation.TrackGetKeyCount(trackIndex) - 1;
    animation.TrackSetKeyValue(trackIndex, lastKey, value);
  }

  public static int GetAnimationTrackIndex(this Animation animation, string trackPath) {
    var trackIndex = animation.FindTrack(trackPath, Animation.TrackType.Value);
    return trackIndex == -1
      ? throw new InvalidOperationException($"{trackPath} track not found.")
      : trackIndex;
  }
}
