﻿using System.Collections.Generic;
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
		private bool isRoundStarted = false;

		private const float delay = 15.03f;

		public void OnRoundRestart()
		{
			Timing.KillCoroutines(coroutine);
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
			while (isRoundStarted)
			{
				yield return Timing.WaitForSeconds(UnityEngine.Random.Range(480, 660));
				if (Random.Range(0, 100) < 50)
				{
					DoorVariant scp106door = DoorNametagExtension.NamedDoors["106_PRIMARY"].TargetDoor;
					DoorVariant scp106door2 = DoorNametagExtension.NamedDoors["106_SECONDARY"].TargetDoor;
					Room room914 = Room.Get(Exiled.API.Enums.RoomType.Lcz914);
					foreach (Door door in Door.List)
					{
						if (door.Base.transform.position != scp106door.transform.position &&
						door.Base.transform.position != scp106door2.transform.position &&
						door.Type != Exiled.API.Enums.DoorType.Scp079First &&
						door.Type != Exiled.API.Enums.DoorType.Scp079Second &&
						door.Type != Exiled.API.Enums.DoorType.Scp914 &&
						Vector3.Distance(door.Position, room914.Position) >= 11f) // Specific offset for how far 914 doors are from the room center
						{
							doors.Add(door);
						}
					}

					if (!Warhead.IsInProgress && !Warhead.IsDetonated)
					{
						isRestarting = true;
						Timing.CallDelayed(delay, () => isRestarting = false);
						Cassie.Message("CRITICAL ERROR . . DOOR SYSTEM MALFUNCTION IN PROGRESS . . DOOR SYSTEM SOFTWARE REPAIR COMMENCING IN 3 . 2 . 1 . . . . . . . DOOR SYSTEM REPAIR COMPLETE", true, true);
						List<Door> openDoors = new List<Door>();
						foreach (Door door in Door.List) if (door.Base.IsConsideredOpen()) openDoors.Add(door);
						while (isRestarting)
						{
							Door door = doors[UnityEngine.Random.Range(0, doors.Count)];
							Timing.RunCoroutine(BreakDoor(door));
							yield return Timing.WaitForSeconds(0.05f);
						}
						foreach (Door door in Door.List.Where(d => d.Type != Exiled.API.Enums.DoorType.Scp079First && d.Type != Exiled.API.Enums.DoorType.Scp079Second))
						{
							door.Base.NetworkTargetState = false;
							door.Base.ServerChangeLock(DoorLockReason.AdminCommand, true);
						}
						yield return Timing.WaitForSeconds(3f);
						foreach (Door door in Door.List.Where(d => d.Type != Exiled.API.Enums.DoorType.Scp079First && d.Type != Exiled.API.Enums.DoorType.Scp079Second))
						{
							if ((door.DoorLockType == Exiled.API.Enums.DoorLockType.Lockdown079 || door.DoorLockType == Exiled.API.Enums.DoorLockType.Regular079) && door.IsLocked) continue;
							door.Base.NetworkTargetState = openDoors.Contains(door);
							door.Base.ServerChangeLock(DoorLockReason.AdminCommand, false);
						}
						brokenDoors.Clear();
					}
				}
			}
		}
	}
}
