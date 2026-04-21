using Ontology;
using Ontology.BaseTypes;
using Ontology.Simulation;
using System.Reflection;
using System.Threading;

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
			(nameof(ResetCache_ClearsPrototypeSingletonSlots), ResetCache_ClearsPrototypeSingletonSlots),
			(nameof(Audit_NoRawAsyncLocalPrototypeFields_InOntologyAssembly), Audit_NoRawAsyncLocalPrototypeFields_InOntologyAssembly),
			(nameof(IntWrapperCtor_AcceptsIntWrapperPrototype), IntWrapperCtor_AcceptsIntWrapperPrototype),
			(nameof(BoolWrapperCtor_AcceptsBoolWrapperPrototype), BoolWrapperCtor_AcceptsBoolWrapperPrototype),
			(nameof(DoubleWrapperCtor_AcceptsDoubleWrapperPrototype), DoubleWrapperCtor_AcceptsDoubleWrapperPrototype),
			(nameof(NestedParent_TracksDirectDottedParent), NestedParent_TracksDirectDottedParent),
			(nameof(NestedParent_TracksDirectDottedParent_WhenParentCreatedAfterChild), NestedParent_TracksDirectDottedParent_WhenParentCreatedAfterChild),
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

	private static void ResetCache_ClearsPrototypeSingletonSlots()
	{
		Initializer.ResetCache();

		Prototype stringBefore = System_String.Prototype;
		stringBefore.Value = 42;

		Initializer.ResetCache();

		Prototype stringAfter = System_String.Prototype;

		AssertTrue(!ReferenceEquals(stringBefore, stringAfter), "Expected System_String prototype slot to be reset.");
		AssertTrue(stringAfter.Value == 1, "Expected System_String prototype to be recreated after reset.");
	}

	private static void Audit_NoRawAsyncLocalPrototypeFields_InOntologyAssembly()
	{
		Type bannedType = typeof(AsyncLocal<Prototype>);
		Assembly[] assemblies = new[] { typeof(Prototype).Assembly };
		var offenders = new List<string>();

		foreach (Assembly assembly in assemblies.Distinct())
		{
			foreach (Type type in assembly.GetTypes())
			{
				foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (field.FieldType == bannedType)
					{
						offenders.Add(type.FullName + "." + field.Name);
					}
				}
			}
		}

		AssertTrue(offenders.Count == 0, "Found raw AsyncLocal<Prototype> fields: " + string.Join(", ", offenders));
	}

	private static void IntWrapperCtor_AcceptsIntWrapperPrototype()
	{
		Initializer.ResetCache();

		IntWrapper original = new IntWrapper(123);
		IntWrapper clone = new IntWrapper((Prototype)original);

		AssertTrue(clone.GetIntValue() == 123, "Expected IntWrapper(Prototype) to accept IntWrapper input and preserve value.");
	}

	private static void BoolWrapperCtor_AcceptsBoolWrapperPrototype()
	{
		Initializer.ResetCache();

		BoolWrapper original = new BoolWrapper(true);
		BoolWrapper clone = new BoolWrapper((Prototype)original);

		AssertTrue(clone.GetBoolValue(), "Expected BoolWrapper(Prototype) to accept BoolWrapper input and preserve value.");
	}

	private static void DoubleWrapperCtor_AcceptsDoubleWrapperPrototype()
	{
		Initializer.ResetCache();

		DoubleWrapper original = new DoubleWrapper(3.5);
		DoubleWrapper clone = new DoubleWrapper((Prototype)original);

		AssertTrue(clone.GetDoubleValue() == 3.5d, "Expected DoubleWrapper(Prototype) to accept DoubleWrapper input and preserve value.");
	}

	private static void NestedParent_TracksDirectDottedParent()
	{
		Initializer.ResetCache();

		Prototype parent = Prototypes.GetOrInsertPrototype("test.nested.parent");
		Prototype child = Prototypes.GetOrInsertPrototype("test.nested.parent.child");

		Prototype? nestedParent = GetNestedParent(child);
		AssertTrue(nestedParent != null, "Expected child prototype to expose a direct NestedParent.");
		AssertTrue(nestedParent.PrototypeID == parent.PrototypeID, "Expected NestedParent to resolve the direct dotted parent prototype.");
		AssertTrue(GetNestedChildren(parent).Any(x => x.PrototypeID == child.PrototypeID), "Expected parent prototype to expose the child through GetNestedChildren().");
	}

	private static void NestedParent_TracksDirectDottedParent_WhenParentCreatedAfterChild()
	{
		Initializer.ResetCache();

		Prototype child = Prototypes.GetOrInsertPrototype("test.outoforder.parent.child");
		Prototype parent = Prototypes.GetOrInsertPrototype("test.outoforder.parent");

		Prototype? nestedParent = GetNestedParent(child);
		AssertTrue(nestedParent != null, "Expected child prototype to pick up NestedParent after the direct dotted parent is created.");
		AssertTrue(nestedParent.PrototypeID == parent.PrototypeID, "Expected NestedParent to resolve the later-created direct dotted parent prototype.");
		AssertTrue(GetNestedChildren(parent).Any(x => x.PrototypeID == child.PrototypeID), "Expected later-created parent prototype to expose the pre-existing child through GetNestedChildren().");
	}

	private static Prototype? GetNestedParent(Prototype prototype)
	{
		PropertyInfo? property = typeof(Prototype).GetProperty("NestedParent", BindingFlags.Instance | BindingFlags.Public);
		AssertTrue(property != null, "Expected Prototype to expose a public NestedParent property.");
		return property.GetValue(prototype) as Prototype;
	}

	private static List<Prototype> GetNestedChildren(Prototype prototype)
	{
		MethodInfo? method = typeof(Prototype).GetMethod("GetNestedChildren", BindingFlags.Instance | BindingFlags.Public);
		AssertTrue(method != null, "Expected Prototype to expose a public GetNestedChildren() method.");
		object? value = method.Invoke(prototype, Array.Empty<object>());
		AssertTrue(value is IEnumerable<Prototype>, "Expected GetNestedChildren() to return IEnumerable<Prototype>.");
		return ((IEnumerable<Prototype>)value).ToList();
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
