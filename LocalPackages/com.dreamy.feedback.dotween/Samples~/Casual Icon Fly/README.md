# Casual / Puzzle Icon Fly

Import **Basic Feedback** and TMP Essential Resources. Install this adapter, add DotweenFeedbackProvider to GameFeedbackRig and assign it to Host → Animation Provider before initialization. Use the preview scene to compare Coin/Star/Energy/Gem presets and five trajectory buttons. No rebuild is required.

| Style | Typical use | Tuning |
| --- | --- | --- |
| Straight | Fast HUD collection | InCubic ease, .4–.6 s, low stagger |
| Arc | Coin/chest reward | ArcHeight 100–200 canvas units, .6–.9 s |
| ScatterMagnet | Match-3 clear, resource shower | Spread 60–120, .7–1 s; 30% scatter then accelerate to target |
| Fountain | Level win/star burst | ArcHeight 160–260, Spread 100, .9 s |
| Spiral | Booster/energy collection | Spread 50–100, two turns, .8–1.1 s |

All styles expose Count, Stagger, Duration, Size, Spread, ArcHeight, Spin and easing in authored node options. Target anchors follow live UI positions. Scale/pop/spin/fade use a shared trajectory evaluator so the fallback backend preserves layout and endpoint behavior. The DOTween implementation owns a sequence per leased icon; scatter uses distinct scatter and collect tweens.

Resource presets play icon collection followed by destination punch. Compose extra VFX/text/haptic nodes in your own definition when needed. Complete fast-forwards the request; Stop returns its icons to the pool. Play requests concurrently to compare independent leases. Neither animation completion nor icon arrival grants wallet resources.
