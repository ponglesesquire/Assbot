namespace Assbot
{
	public abstract class Command
	{
		public virtual string Prefix
		{
			get
			{
				return "";
			}
		}

		protected Bot Parent;

		protected Command(Bot parent)
		{
			Parent = parent;
		}

		public virtual void Execute()
		{

		}
	}
}
