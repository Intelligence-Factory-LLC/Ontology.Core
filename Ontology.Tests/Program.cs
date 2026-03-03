using Ontology;

internal static class Program
{
	private static int Main()
	{
		Initializer.Initialize();

		var tests = new (string Name, Action Run)[]
		{
			(nameof(InvalidateCacheParents_HandlesDescendantCycles), InvalidateCacheParents_HandlesDescendantCycles),
			(nameof(InsertTypeOf_RejectsSelfEdge), InsertTypeOf_RejectsSelfEdge),
			(nameof(RemoveTypeOfAndDescendants_RemovesLastMatchWithoutIndexError), RemoveTypeOfAndDescendants_RemovesLastMatchWithoutIndexError),
			(nameof(RemoveTypeOfAndDescendants_RemovesCorrectDescendantEdges), RemoveTypeOfAndDescendants_RemovesCorrectDescendantEdges),
		};

		int failed = 0;
		foreach (var (name, run) in tests)
		{
			try
			{
				run();
				Console.WriteLine($"PASS {name}");
			}
			catch (Exception ex)
			{
				failed++;
				Console.WriteLine($"FAIL {name}: {ex.Message}");
			}
		}

		return failed == 0 ? 0 : 1;
	}

	private static void InvalidateCacheParents_HandlesDescendantCycles()
	{
		Initializer.ResetCache();

		Prototype a = Prototypes.GetOrInsertPrototype("test.a");
		Prototype b = Prototypes.GetOrInsertPrototype("test.b");
		Prototype c = Prototypes.GetOrInsertPrototype("test.c");

		b.InsertTypeOf(a);
		c.InsertTypeOf(b);

		_ = c.GetAllParents().ToList();

		a.InsertTypeOf(c);

		List<int> parents = a.GetAllParents().ToList();
		AssertTrue(parents.Contains(c.PrototypeID), "Expected c to be in a parent closure.");
		AssertTrue(parents.Contains(b.PrototypeID), "Expected b to be in a parent closure.");
	}

	private static void InsertTypeOf_RejectsSelfEdge()
	{
		Initializer.ResetCache();

		Prototype p = Prototypes.GetOrInsertPrototype("test.self");

		AssertThrows<InvalidOperationException>(() => p.InsertTypeOf(p.PrototypeID), "InsertTypeOf should reject self-edge.");
		AssertThrows<InvalidOperationException>(() => p.InsertPrimaryTypeOf(p.PrototypeID), "InsertPrimaryTypeOf should reject self-edge.");
		AssertTrue(!p.GetTypeOfs().Any(), "Self-edge insertion should not modify type-ofs.");
	}

	private static void RemoveTypeOfAndDescendants_RemovesLastMatchWithoutIndexError()
	{
		Initializer.ResetCache();

		Prototype root = Prototypes.GetOrInsertPrototype("test.root");
		Prototype parent = Prototypes.GetOrInsertPrototype("test.parent");
		Prototype child = Prototypes.GetOrInsertPrototype("test.child");

		parent.InsertTypeOf(root);
		child.InsertTypeOf(parent);

		child.RemoveTypeOfAndDescendants(root);

		AssertTrue(!child.GetTypeOfs().Any(), "Expected all matching parent links to be removed.");
		AssertTrue(!parent.GetDescendants().Any(d => d.PrototypeID == child.PrototypeID), "Expected parent descendant edge to be removed.");
	}

	private static void RemoveTypeOfAndDescendants_RemovesCorrectDescendantEdges()
	{
		Initializer.ResetCache();

		Prototype root = Prototypes.GetOrInsertPrototype("test.root");
		Prototype p1 = Prototypes.GetOrInsertPrototype("test.p1");
		Prototype p2 = Prototypes.GetOrInsertPrototype("test.p2");
		Prototype unrelated = Prototypes.GetOrInsertPrototype("test.unrelated");
		Prototype child = Prototypes.GetOrInsertPrototype("test.child");

		p1.InsertTypeOf(root);
		p2.InsertTypeOf(p1);

		child.InsertTypeOf(p1);
		child.InsertTypeOf(p2);
		child.InsertTypeOf(unrelated);

		child.RemoveTypeOfAndDescendants(root);

		List<int> remaining = child.GetTypeOfs().ToList();
		AssertTrue(remaining.Count == 1 && remaining[0] == unrelated.PrototypeID, "Expected only unrelated parent to remain.");
		AssertTrue(!p1.GetDescendants().Any(d => d.PrototypeID == child.PrototypeID), "Expected p1 descendant edge to be removed.");
		AssertTrue(!p2.GetDescendants().Any(d => d.PrototypeID == child.PrototypeID), "Expected p2 descendant edge to be removed.");
		AssertTrue(unrelated.GetDescendants().Any(d => d.PrototypeID == child.PrototypeID), "Expected unrelated descendant edge to remain.");
	}

	private static void AssertTrue(bool condition, string message)
	{
		if (!condition)
			throw new InvalidOperationException(message);
	}

	private static void AssertThrows<T>(Action action, string message) where T : Exception
	{
		try
		{
			action();
		}
		catch (T)
		{
			return;
		}

		throw new InvalidOperationException(message);
	}
}
