namespace igCauldron3.Transactions
{
	public abstract class Transaction
	{
		/// <summary>
		/// Carry out the action
		/// </summary>
		public abstract void Commit();


		/// <summary>
		/// Revert the action
		/// </summary>
		public abstract void Revert();
	}
}