using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
using MEC;

namespace DoorRestartSystem
{
	class EventHandlers
	{
		private CoroutineHandle coroutine;
		private List<DoorVariant> brokenDoors = new List<DoorVariant>();
		private List<DoorVariant> doors = new List<DoorVariant>();
		private bool isRestarting = false;
		private bool isRoundStarted = false;

		private const float delay = 15.03f;

		public void OnRoundRestart()
		{
			Timing.KillCoroutines(new CoroutineHandle[] { coroutine });
			brokenDoors.Clear();
			doors.Clear();
			isRestarting = false;
			isRoundStarted = false;
		}

		public void OnRoundStart()
		{
			isRoundStarted = true;
			coroutine = Timing.RunCoroutine(StartSystem());
		}

		public void OnRoundEnd(RoundEndedEventArgs ev) => isRoundStarted = false;

		private IEnumerator<float> BreakDoor(DoorVariant door)
		{
			doors.Remove(door);
			brokenDoors.Add(door);
			yield return Timing.WaitForSeconds(0.7f);
			if (isRestarting)
			{
				door.NetworkTargetState = !door.NetworkTargetState;
				door.ServerChangeLock(DoorLockReason.AdminCommand, door.ActiveLocks > 0 ? false : true);
			}
			doors.Add(door);
			brokenDoors.Remove(door);
		}

		private IEnumerator<float> StartSystem()
		{
			while (isRoundStarted)
			{
				yield return Timing.WaitForSeconds(UnityEngine.Random.Range(480, 660));
				if (UnityEngine.Random.Range(0, 100) < 50)
				{
					foreach (DoorVariant door in Map.Doors) doors.Add(door);
					if (!Warhead.IsInProgress && !Warhead.IsDetonated)
					{
						isRestarting = true;
						Timing.CallDelayed(delay, () => isRestarting = false);
						Cassie.Message("CRITICAL ERROR . . DOOR SYSTEM MALFUNCTION IN PROGRESS . . DOOR SYSTEM SOFTWARE REPAIR COMMENCING IN 3 . 2 . 1 . . . . . . . DOOR SYSTEM REPAIR COMPLETE", true, true);
						List<DoorVariant> openDoors = new List<DoorVariant>();
						foreach (DoorVariant door in Map.Doors) if (door.IsConsideredOpen()) openDoors.Add(door);
						while (isRestarting)
						{
							DoorVariant door = doors[UnityEngine.Random.Range(0, doors.Count)];
							Timing.RunCoroutine(BreakDoor(door));
							yield return Timing.WaitForSeconds(0.05f);
						}
						foreach (DoorVariant door in Map.Doors)
						{
							door.NetworkTargetState = false;
							door.ServerChangeLock(DoorLockReason.AdminCommand, true);
						}
						yield return Timing.WaitForSeconds(3f);
						foreach (DoorVariant door in Map.Doors)
						{
							door.NetworkTargetState = openDoors.Contains(door);
							door.ServerChangeLock(DoorLockReason.AdminCommand, false);
						}
						brokenDoors.Clear();
					}
				}
			}
		}
	}
}
