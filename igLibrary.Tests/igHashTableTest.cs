namespace igLibrary.Tests;

using System.Collections;
using igLibrary.Core;

public class igHashTableTest
{
	[Fact]
	public void Initialization()
	{
		igStringIntHashTable hashTable = new igStringIntHashTable();
		Assert.Empty(hashTable._keys);
		Assert.Empty(hashTable._values);
		Assert.Equal(0, hashTable._hashItemCount);

		//basic test
		hashTable.Activate(10);

		Assert.Equal(10, hashTable._keys.Length);
		Assert.Equal(10, hashTable._values.Length);

		//error
		Assert.Throws<ArgumentOutOfRangeException>(() => hashTable.Activate(-1));

		//load factor
		hashTable.ActivateWithExpectedCount(10);
		Assert.Equal(20, hashTable._keys.Length);
		Assert.Equal(20, hashTable._values.Length);

		hashTable._loadFactor = 0.1f;

		hashTable.ActivateWithExpectedCount(10);
		Assert.Equal(100, hashTable._keys.Length);
		Assert.Equal(100, hashTable._values.Length);
	}

	[Fact]
	public void ItemAdding1()
	{
		igStringStringHashTable hashTable = new igStringStringHashTable();

		hashTable.Add("]", "½");
		hashTable.Add(">", "ª");
		hashTable.Add("[", "γ");	//ignore warnings from this line
		hashTable.Add("<", "¦");

		Assert.Equal(4, hashTable._hashItemCount);
		Assert.Equal(4, hashTable._keys.Length);
		Assert.Equal(4, hashTable._values.Length);

		Assert.Equal("]", hashTable._keys[0]);
		Assert.Equal(">", hashTable._keys[1]);
		Assert.Equal("[", hashTable._keys[2]);
		Assert.Equal("<", hashTable._keys[3]);

		Assert.Equal("½", hashTable._values[0]);
		Assert.Equal("ª", hashTable._values[1]);
		Assert.Equal("γ", hashTable._values[2]);
		Assert.Equal("¦", hashTable._values[3]);
	}
	[Fact]
	public void ItemAdding2()
	{
		igStringStringHashTable hashTable = new igStringStringHashTable();

		hashTable.ActivateWithExpectedCount(10);

		hashTable.Add("`", "v");
		hashTable.Add("<", "µ");
		hashTable.Add("}", "¼");
		hashTable.Add("[", "α");
		hashTable.Add("ʘ", "÷");
		hashTable.Add("^", "¢");
		hashTable.Add("]", "β");
		hashTable.Add("{", "¬");
		hashTable.Add("~", "±");
		hashTable.Add(">", "¶");

		Assert.Equal(10, hashTable._hashItemCount);
		Assert.Equal(20, hashTable._keys.Length);
		Assert.Equal(20, hashTable._values.Length);

		Assert.Equal("<", hashTable._keys[0]);
		Assert.Equal("}", hashTable._keys[1]);
		Assert.Null(hashTable._keys[2]);
		Assert.Null(hashTable._keys[3]);
		Assert.Null(hashTable._keys[4]);
		Assert.Null(hashTable._keys[5]);
		Assert.Equal("[", hashTable._keys[6]);
		Assert.Equal("ʘ", hashTable._keys[7]);
		Assert.Null(hashTable._keys[8]);
		Assert.Equal("^", hashTable._keys[9]);
		Assert.Null(hashTable._keys[10]);
		Assert.Null(hashTable._keys[11]);
		Assert.Equal("]", hashTable._keys[12]);
		Assert.Null(hashTable._keys[13]);
		Assert.Equal("{", hashTable._keys[14]);
		Assert.Null(hashTable._keys[15]);
		Assert.Null(hashTable._keys[16]);
		Assert.Equal("~", hashTable._keys[17]);
		Assert.Equal(">", hashTable._keys[18]);
		Assert.Equal("`", hashTable._keys[19]);

		Assert.Equal("µ", hashTable._values[0]);
		Assert.Equal("¼", hashTable._values[1]);
		Assert.Null(hashTable._values[2]);
		Assert.Null(hashTable._values[3]);
		Assert.Null(hashTable._values[4]);
		Assert.Null(hashTable._values[5]);
		Assert.Equal("α", hashTable._values[6]);
		Assert.Equal("÷", hashTable._values[7]);
		Assert.Null(hashTable._values[8]);
		Assert.Equal("¢", hashTable._values[9]);
		Assert.Null(hashTable._values[10]);
		Assert.Null(hashTable._values[11]);
		Assert.Equal("β", hashTable._values[12]);
		Assert.Null(hashTable._values[13]);
		Assert.Equal("¬", hashTable._values[14]);
		Assert.Null(hashTable._values[15]);
		Assert.Null(hashTable._values[16]);
		Assert.Equal("±", hashTable._values[17]);
		Assert.Equal("¶", hashTable._values[18]);
		Assert.Equal("v", hashTable._values[19]);
	}

	[Fact]
	public void AccessingTest()
	{
		igStringStringHashTable hashTable = new igStringStringHashTable();

		hashTable.ActivateWithExpectedCount(10);

		hashTable.Add("`", "v");
		hashTable.Add("<", "µ");
		hashTable.Add("}", "¼");
		hashTable.Add("[", "α");
		hashTable.Add("ʘ", "÷");
		hashTable.Add("^", "¢");
		hashTable.Add("]", "β");
		hashTable.Add("{", "¬");
		hashTable.Add("~", "±");
		hashTable.Add(">", "¶");

		Assert.Equal("v", hashTable["`"]);
		Assert.Equal("µ", hashTable["<"]);
		Assert.Equal("¼", hashTable["}"]);
		Assert.Equal("α", hashTable["["]);
		Assert.Equal("÷", hashTable["ʘ"]);
		Assert.Equal("¢", hashTable["^"]);
		Assert.Equal("β", hashTable["]"]);
		Assert.Equal("¬", hashTable["{"]);
		Assert.Equal("±", hashTable["~"]);
		Assert.Equal("¶", hashTable[">"]);
	}
}