# Stamina v2

## Supabase deployment (required)

Run `Docs/Stamina-v2.sql` in the Supabase SQL Editor **before testing the updated client with a real account**. The database was not changed by this implementation.

The migration adds `player_stamina.additional_stamina integer not null default 0`, changes the natural stamina default to five, and caps existing natural stamina at five. Excess natural stamina is discarded, never converted to additional stamina. For example, an old balance of 100 becomes five natural apples and zero additional stamina. Existing additional stamina, if any, is left unchanged. The migration is idempotent.

Inspect the existing new-user/auth trigger or initialization RPC. If it explicitly inserts the old stamina value (10 or 100), change that literal to 5. If it relies on the column default, no trigger change is needed. Existing `last_saved_datetime`, `get_server_time()`, user IDs, and RLS policies are retained.

The new client writes `additional_stamina`. Until the SQL is applied, saves to the old schema will fail. Stop old clients from updating stamina during deployment.

## Gameplay and access

- `GameManager.Instance.staminaData.stamina`: natural apples, zero through five.
- `GameManager.Instance.staminaData.additionalStamina`: bonus stamina, initially zero.
- `GameManager.Instance.staminaData.Total`: total available stamina. GameManager has no forwarding stamina properties.
- `StaminaManager.Instance.ConsumeStamina(amount)` returns false without partial payment when the balance is insufficient. Natural apples are used first.
- `StaminaManager.Instance.AddAdditionalStamina(amount)` is the future reward/purchase entry point. No purchase or reward flow was added.

Normal entry costs one, habitat/defend-the-house entry costs two. Next-stage and restart buttons use the same costs. The existing editor test-play path remains free.

## Recovery and UI

One natural apple recovers every five minutes, capped at five. Recovery is sequential; additional stamina never recovers with time. A server UTC sample plus realtime elapsed time drives the live timer, with server resynchronization every minute and after focus/resume. Recovery applies across scenes and accounts for time spent away. The timer starts when leaving full stamina; consuming below the cap retains existing recovery progress.

CommonCanvas itself contains a StaminaBarUI component on ProductCanvas/StaminaCard, five serialized apple/fill references, a serialized grayscale material and the bonus counter. Empty apples are grayscale. Only the next apple shows clockwise color recovery within the apple silhouette. Newly recovered apples bounce around their center using unscaled time. The prefab's existing sprite, layout and nested canvases are retained.

Natural stamina consumption sends an explicit event with the spent apple range. StaminaBarUI captures only those gold apple images into a non-interactive temporary overlay. Over 0.45 seconds they ease upward by 55% of their displayed height, swell slightly, then quickly shrink and fade. Multiple spent apples animate simultaneously. The overlay belongs to the current scene and removes itself when finished. Spending additional stamina alone emits no apple effect. Spend Duration and Spend Rise Height are configurable on the prefab's StaminaBarUI.

All paid entry buttons call the shared StaminaManager.TryEnterStage method. It consumes once, waits the configured Spend Duration (0.45 seconds by default), confirms any apple effect has completed, then invokes the existing scene-transition action. Additional-only and mixed payments use the same wait time. An entry lock and transparent input blocker prevent repeated payment or other button navigation while waiting. If the initiating scene has changed externally, the pending transition is cancelled. Existing UnityEvent public button signatures are unchanged; editor test-play still bypasses payment.

Local JSON stores the UTC recharge anchor as an ISO string because JsonUtility does not serialize DateTime fields directly. Stamina saves are serialized to avoid older asynchronous requests overwriting newer balances.

Cloud load/save, local serialization and recovery calculations normalize timestamps to UTC. Postgrest can deserialize `+00:00` as a local DateTime; comparing its wall-clock value to UTC previously misclassified valid saved anchors as future dates and reset the timer on reconnect. Local timestamps are converted to UTC before comparison. Offset-free legacy server timestamps are interpreted as UTC. This fix requires no database schema or SQL changes.

## Validation

`Tools > MoePang > Validate Stamina Rules` runs 28 deterministic checks without editing player data or contacting Supabase. It covers payment priority, mixed payment, insufficient balance, invalid costs, five-minute recovery, offline accumulation, remainder preservation, full-cap timing, legacy capping without bonus transfer, independent bonus balances, migration idempotence, invalid values and local JSON/timer round trips. Reconnect regression cases use the actual Postgrest serializer and verify two minutes elapsed retains three minutes remaining, six minutes elapsed restores an apple, multiple offline intervals preserve the remainder, and local/UTC/offset-free timestamps produce consistent results.

Unity compilation and the grayscale shader were checked. All five apple and fill references were verified. Three actual prefab states were rendered in an isolated preview scene and visually inspected. A complete login/save/reload against the updated database remains to be checked after the SQL migration.

## Manual check after SQL

1. Start through TitleScene and log in. A new account shows five colored apples and +0. An old natural balance above five is capped at five with no bonus transfer.
2. Enter a normal stage: one natural apple is used. Enter habitat mode: two are used, borrowing from additional stamina if necessary.
3. Return to MainScene: the next empty apple fills clockwise, then bounces on recovery. At full stamina, wait and consume again: the next recharge starts at five minutes.
4. Restart/advance a stage and confirm the same entry cost. With insufficient total stamina, remain on the current screen with balances unchanged.
5. Restart the application and verify both balances and remaining recovery time persist.
