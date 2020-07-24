using Exiled.API.Interfaces;

namespace DoorRestartSystem
{
	public class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;
	}
}
