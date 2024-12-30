namespace Potion
{
	public abstract class Mod
	{
		protected List<Transaction> _transactions = new List<Transaction>();

		public virtual void Apply(ModManager manager)
		{
			for (int i = 0; i < _transactions.Count; i++)
			{
				_transactions[i].Apply(manager.Blobs);
			}
		}

		public virtual void Revert(ModManager manager)
		{
			for (int i = _transactions.Count; i >= 0; i--)
			{
				_transactions[i].Revert(manager.Blobs);
			}
		}
	}
}