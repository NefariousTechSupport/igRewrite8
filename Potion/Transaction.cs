namespace Potion
{
	public abstract class Transaction
	{
		protected TransactionType _type;

		public abstract void Revert(BlobManager blobs);
	}
}