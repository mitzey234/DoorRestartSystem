using Exiled.API.Features;

namespace DoorRestartSystem
{
	public class DoorRestartSystem : Plugin<Config>
	{
		private EventHandlers ev;

		public override void OnEnabled()
		{
			base.OnEnabled();

			if (!Config.IsEnabled) return;

			ev = new EventHandlers();

			Exiled.Events.Handlers.Server.RestartingRound+= ev.OnRoundRestart;
			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStart;
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			Exiled.Events.Handlers.Server.RestartingRound -= ev.OnRoundRestart;
			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStart;

			ev = null;
		}

		public override string Name => "DoorRestartSystem";
	}
}
