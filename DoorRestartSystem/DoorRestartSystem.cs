using Exiled.API.Features;

namespace DoorRestartSystem
{
	public class DoorRestartSystem : Plugin<Config>
	{
		private EventHandlers ev;

		public override void OnEnabled()
		{
			ev = new EventHandlers();

			Exiled.Events.Handlers.Server.RestartingRound+= ev.OnRoundRestart;
			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded += ev.OnRoundEnd;
		}

		public override void OnDisabled()
		{
			Exiled.Events.Handlers.Server.RestartingRound -= ev.OnRoundRestart;
			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= ev.OnRoundEnd;

			ev = null;
		}

		public override string Name => "DoorRestartSystem";
	}
}
