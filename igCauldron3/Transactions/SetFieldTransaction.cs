using System.Reflection;
using igLibrary.Core;

namespace igCauldron3.Transactions
{
	/// <summary>
	/// Field for updating the value of a field
	/// </summary>
	public class SetFieldAction : Transaction
	{
		private igObject _target;
		private FieldInfo _field;
		private object? _oldValue;
		private object? _newValue;


		/// <summary>
		/// Factory constructor for set field action
		/// </summary>
		/// <param name="target">The object to apply the change to</param>
		/// <param name="field">The field to update</param>
		/// <param name="value">The value to set the field to</param>
		public static void Create(igObject target, FieldInfo field, object? value)
		{
			SetFieldAction action = new SetFieldAction(target, field, value);

			TransactionManager.DoAction(action);
		}


		/// <summary>
		/// Private constructor for set field action
		/// </summary>
		/// <param name="target">The object to apply the change to</param>
		/// <param name="field">The field to update</param>
		/// <param name="value">The value to set the field to</param>
		private SetFieldAction(igObject target, FieldInfo field, object? value)
		{
			if(target == null) throw new ArgumentNullException("target object is null");
			if(field == null) throw new ArgumentNullException("field FieldInfo is null");

			_target = target;
			_field = field;
			_newValue = value;

			// Grab the old value for the sake of undoing changes
			_oldValue = _field.GetValue(target);
		}


		/// <summary>
		/// Set the field on the object
		/// </summary>
		public override void Commit()
		{
			_field.SetValue(_target, _newValue);
		}


		/// <summary>
		/// Unset the field on the object
		/// </summary>
		public override void Revert()
		{
			_field.SetValue(_target, _oldValue);
		}
	}
}