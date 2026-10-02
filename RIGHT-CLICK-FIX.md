# Right-click responsiveness fix

The low-level mouse hook now uses a dedicated Windows message-loop thread, so WPF layout and settings work do not delay mouse input. Ordinary clicks skip process lookup and gesture recognition. Unrecognized movement and gestures without an enabled, non-None action restore a right click instead of silently consuming it. Delayed actions capture the resolved configuration for their own gesture.

Regression tests cover stationary clicks, movement between the 8-pixel feedback threshold and the 48-pixel recognition threshold, unmatched gestures, configured gestures, disabled/None bindings, and hook start/stop/restart on Windows.

Manual validation: run the repaired app, right-click the desktop repeatedly, then repeat with a small movement while holding right click and while Settings is open. Verify configured gestures still work. Existing right-button capture continues to replay unmatched clicks on release; native right-button drag behavior is outside this change.

Performance follow-up: unrelated mouse notifications bypass payload marshaling; movement below all profile thresholds skips process lookup; no-profile configurations skip process lookup; right-click buffers are reused; initial trail points are queued once. Configuration publication uses a volatile snapshot. The 60 ms gesture release delay is retained. Validation: 57 tests passed; desktop latency has not been benchmarked.
