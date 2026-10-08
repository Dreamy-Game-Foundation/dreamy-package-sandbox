# Feedback Economy

Prerequisites: **Basic Feedback** sample, Dreamy Economy, Dreamy UI, Dreamy Audio and their dependencies. Open `Generated/FeedbackEconomyDemo.unity` and press Play.

Grant commits 100 coins before particle/text/icon feedback and the sample tone. Retry repeats the transaction ID without another balance change or reward feedback. Back/Close closes the standalone panel; reopen the scene to restart its in-memory demo session.

FeedbackDemoWallet and FeedbackEconomyDemo are demonstration composition with no player-save persistence. Real hosts call `FeedbackEconomyPanel.Configure(wallet, balanceSource, audioService)`; Foundation also supplies its shared FeedbackHost. FeedbackRewardPresenter observes confirmed changes and disposes its subscription on close. Icons never mutate the wallet.

Foundation uses the persistent Datasave wallet, shared feedback owner and existing audio. Coin rewards from other features also reach that owner. Reopening binds one observer without duplicating effects.

`Dreamy/Feedback/Build Economy Demo` creates another copy in a new folder after Basic Feedback. Tests verify retry, disposed subscriptions and prefab close/reopen.

The shipped EventSystem uses StandaloneInputModule. Set Active Input Handling to Input Manager (Old) or Both. For new-input-only projects, replace that component with InputSystemUIInputModule after importing the Input System package.

With the sandbox DOTween Git revision, retain the Unity Physics2D built-in module: its Physics2D extension code requires that assembly even when this sample only tweens UI.

Before running either sample in a fresh project, import **Window > TextMeshPro > Import TMP Essential Resources** once. UGUI 2.0 TMP still needs its project-wide TMP Settings/default style resources in players. The sample uses its own static font and shaders, but does not ship a second `Resources/TMP Settings` asset that could conflict with your project.
