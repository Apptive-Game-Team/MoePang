-- Run in Supabase SQL Editor before running the updated client.
-- Existing natural stamina is capped at five. Excess is not converted to bonus stamina.
begin;
alter table public.player_stamina
    add column if not exists additional_stamina integer not null default 0;

update public.player_stamina
set stamina = greatest(0, least(5, stamina)),
    last_saved_datetime = coalesce(last_saved_datetime, now());

alter table public.player_stamina alter column stamina set default 5;

-- Replace an old upper-bound check, if one exists, only after reviewing its name.
-- This migration leaves existing constraints and RLS policies intact.
notify pgrst, 'reload schema';
commit;

-- Verify:
select column_name, data_type, column_default
from information_schema.columns
where table_schema = 'public' and table_name = 'player_stamina';

-- Also check your auth/new-player trigger: insert stamina=5, additional_stamina=0.
-- If it explicitly inserts the old 10/100 value, change that literal to 5.
