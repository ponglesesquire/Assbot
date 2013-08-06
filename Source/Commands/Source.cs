namespace Assbot.Commands
{
	public class Source : Command
	{
		public override string Prefix
		{
			get
			{
				return "source";
			}
		}

		public Source(Bot parent)
			: base(parent)
		{

		}

		public override void Execute()
		{
			Parent.SendChannelMessage("https://github.com/Rixlet/Assbot");
		}
	}
}
