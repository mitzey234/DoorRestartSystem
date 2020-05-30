using EXILED;

namespace DoorRestartSystem
{
	public class Plugin : EXILED.Plugin
	{
		private EventHandlers ev;

		public override void OnEnable()
		{
			ev = new EventHandlers();
			Events.RoundRestartEvent += ev.OnRoundRestart;
			Events.RoundStartEvent += ev.OnRoundStart;
			Events.RoundEndEvent += ev.OnRoundEnd;
		}

		public override void OnDisable() { }

		public override void OnReload() { }

		public override string getName { get; } = "DoorRestartSystem";
	}
}
