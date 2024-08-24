namespace igCauldron3.Transactions
{
	/// <summary>
	/// Manages actions for Undo/Redo
	/// </summary>
	public static class TransactionManager
	{
		private static List<Transaction> _transactions = new List<Transaction>();
		private static int _index;


		/// <summary>
		/// Submit an action to the list, clearing previously undone actions 
		/// </summary>
		public static void DoAction(Transaction transaction)
		{
			_index++;

			if(_index < _transactions.Count)
			{
				_transactions.RemoveRange(_index, _transactions.Count - _index);
			}

			_transactions.Add(transaction);

			transaction.Commit();
		}


		/// <summary>
		/// Undo an action
		/// </summary>
		public static void Undo()
		{
			if(_index == 0) return;

			_index--;
			_transactions[_index].Revert();
		}


		/// <summary>
		/// Redo an action
		/// </summary>
		public static void Redo()
		{
			if(_index == _transactions.Count - 1) return;

			_index++;
			_transactions[_index].Commit();
		}
	} 
}