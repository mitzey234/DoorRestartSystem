using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
using MEC;
using UnityEngine;

namespace DoorRestartSystem
{
	class EventHandlers
	{
		private CoroutineHandle coroutine;
		private List<Door> brokenDoors = new List<Door>();
		private List<Door> doors = new List<Door>();
		private bool isRestarting = false;

		private const float delay = 15.03f;

		public void OnRoundRestart()
		{
			Timing.KillCoroutines(coroutine);
			brokenDoors.Clear();
			doors.Clear();
			isRestarting = false;
		}

		public void OnRoundStart()
		{
			coroutine = Timing.RunCoroutine(StartSystem());
			foreach (Door door in Door.List)
			{
				if (door.Type != Exiled.API.Enums.DoorType.Scp106Primary &&
					door.Type != Exiled.API.Enums.DoorType.Scp106Secondary &&
					door.Type != Exiled.API.Enums.DoorType.Scp079First &&
					door.Type != Exiled.API.Enums.DoorType.Scp079Second &&
					door.Type != Exiled.API.Enums.DoorType.Scp914Door)
				{
					doors.Add(door);
				}
			}
		}

		private IEnumerator<float> BreakDoor(Door door)
		{
			doors.Remove(door);
			brokenDoors.Add(door);
			yield return Timing.WaitForSeconds(0.7f);
			if (isRestarting)
			{
				door.Base.NetworkTargetState = !door.Base.NetworkTargetState;
				door.Base.ServerChangeLock(DoorLockReason.AdminCommand, door.Base.ActiveLocks > 0 ? false : true);
			}
			doors.Add(door);
			brokenDoors.Remove(door);
		}

		private IEnumerator<float> StartSystem()
		{
			while (Round.IsStarted)
			{
				yield return Timing.WaitForSeconds(Random.Range(480, 660));
				if (Random.Range(0, 100) < 50)
				{
					if (!Warhead.IsInProgress && !Warhead.IsDetonated)
					{
						isRestarting = true;
						Timing.CallDelayed(delay, () => isRestarting = false);
						Cassie.Message("CRITICAL ERROR . . DOOR SYSTEM MALFUNCTION IN PROGRESS . . DOOR SYSTEM SOFTWARE REPAIR COMMENCING IN 3 . 2 . 1 . . . . . . . DOOR SYSTEM REPAIR COMPLETE", true, true);
						List<Door> openDoors = new List<Door>();
						brokenDoors.Clear();
						foreach (Door door in doors) if (door.Base.IsConsideredOpen()) openDoors.Add(door);
						while (isRestarting)
						{
							Door door = doors[Random.Range(0, doors.Count)];
							Timing.RunCoroutine(BreakDoor(door));
							yield return Timing.WaitForSeconds(0.05f);
						}
						foreach (Door door in doors)
						{
							door.Base.NetworkTargetState = false;
							door.Base.ServerChangeLock(DoorLockReason.AdminCommand, true);
						}
						yield return Timing.WaitForSeconds(3f);
						foreach (Door door in doors)
						{
							if ((door.DoorLockType == Exiled.API.Enums.DoorLockType.Lockdown079 || door.DoorLockType == Exiled.API.Enums.DoorLockType.Regular079) && door.IsLocked) continue;
							door.Base.NetworkTargetState = openDoors.Contains(door);
							door.Base.ServerChangeLock(DoorLockReason.AdminCommand, false);
						}
					}
				}
			}
		}
	}
}
