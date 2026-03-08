using System.Reflection;
using TreeDataStructures.Implementations.AVL;
using TreeDataStructures.Implementations.BST;
using TreeDataStructures.Implementations.RedBlackTree;
using TreeDataStructures.Implementations.Splay;
using TreeDataStructures.Implementations.Treap;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Tests.Base;

[TestFixture(typeof(BinarySearchTree<int, string>))]
[TestFixture(typeof(AvlTree<int, string>))]
[TestFixture(typeof(RedBlackTree<int, string>))]
[TestFixture(typeof(SplayTree<int, string>))]
[TestFixture(typeof(Treap<int, string>))]
public abstract class GenericTreeTests<TTree> where TTree : ITree<int, string>, new()
{
    protected TTree Tree;
    
    [SetUp]
    public void Setup()
    {
        Tree = new TTree();
    }
    
    #region Базовые операции (IDictionary)
    
    [Test]
    public void Test_InsertAndCount()
    {
        Tree.Add(5, "Five");
        Tree.Add(3, "Three");
        Tree.Add(7, "Seven");
        
        Assert.Multiple(() =>
        {
            Assert.That(Tree.Count, Is.EqualTo(3));
            Assert.That(Tree.ContainsKey(5), Is.True);
            Assert.That(Tree.ContainsKey(99), Is.False);
        });
    }
    
    [Test]
    public void Test_UpdateExistingKey()
    {
        Tree.Add(10, "Initial");
        Tree[10] = "Updated"; // Тест индексатора set
        
        Assert.Multiple(() =>
        {
            Assert.That(Tree.Count, Is.EqualTo(1));
            Assert.That(Tree[10], Is.EqualTo("Updated"));
        });
    }
    
    [Test]
    public void Test_TryGetValue()
    {
        Tree.Add(10, "Ten");
        
        bool found = Tree.TryGetValue(10, out var val);
        bool notFound = Tree.TryGetValue(99, out var nullVal);
        
        Assert.Multiple(() =>
        {
            Assert.That(found, Is.True);
            Assert.That(val, Is.EqualTo("Ten"));
            Assert.That(notFound, Is.False);
        });
    }
    
    [Test]
    public void Test_Indexer_Throws_OnMissingKey()
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            string _ = Tree[999];
        });
    }
    
    [Test]
    public void Test_Clear()
    {
        Tree.Add(1, "1");
        Tree.Add(2, "2");
        Tree.Clear();
        
        Assert.Multiple(() =>
        {
            Assert.That(Tree.Count, Is.EqualTo(0));
            Assert.That(Tree.InOrder(), Is.Empty);
        });
    }
    
    [Test]
    public void Test_Keys_Values_Collections()
    {
        Dictionary<int, string> data = new Dictionary<int, string> { { 5, "A" }, { 3, "B" }, { 7, "C" } };
        foreach (var kvp in data) Tree.Add(kvp.Key, kvp.Value);
        
        // Keys и Values в BST возвращаются в порядке возрастания (InOrder)
        List<int> expectedKeys = data.Keys.OrderBy(x => x).ToList();
        List<string> expectedValues = expectedKeys.Select(k => data[k]).ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(Tree.Keys, Is.EquivalentTo(expectedKeys));
            Assert.That(Tree.Values, Is.EquivalentTo(expectedValues));
        });
    }
    
    #endregion
    
    #region Удаление
    
    [Test]
    public void Test_Remove_Leaf_And_InternalNodes()
    {
        //      50
        //    /    \
        //  30      70
        //  / \    /  \
        // 20 40  60  80
        int[] keys = new[] { 50, 30, 70, 20, 40, 60, 80 };
        foreach (var k in keys) Tree.Add(k, k.ToString());
        
        // Удаление листа
        Assert.That(Tree.Remove(20), Is.True);
        Assert.That(Tree.ContainsKey(20), Is.False);
        
        // Удаление узла с одним ребенком (если бы мы удалили 30 после 20)
        Assert.That(Tree.Remove(30), Is.True);
        Assert.That(Tree.ContainsKey(30), Is.False);
        
        // Удаление корня (узла с двумя детьми)
        Assert.That(Tree.Remove(50), Is.True);
        Assert.That(Tree.ContainsKey(50), Is.False);
        
        Assert.That(Tree.Count, Is.EqualTo(4));
        
        // Проверка, что дерево осталось валидным BST
        List<int> remaining = Tree.InOrder().Select(x => x.Key).ToList();
        Assert.That(remaining, Is.Ordered);
    }
    
    #endregion
    
    
    [Test]
    public void Test_RandomData_Consistency()
    {
        Random random = new (123);
        HashSet<int> inserted = new ();
        
        for (int i = 0; i < 500; i++)
        {
            int val = random.Next(-1000, 1000);
            if (inserted.Add(val)) Tree.Add(val, "v");
        }
        
        Assert.That(Tree.Count, Is.EqualTo(inserted.Count));
        
        // Проверка сортировки
        List<int> sortedKeys = Tree.InOrder().Select(x => x.Key).ToList();
        Assert.That(sortedKeys, Is.Ordered);
        Assert.That(sortedKeys, Is.EquivalentTo(inserted));
        
        // Удаляем половину
        List<int> toRemove = inserted.Take(250).ToList();
        foreach (int k in toRemove)
        {
            Assert.That(Tree.Remove(k), Is.True, $"Failed to remove key {k}");
            inserted.Remove(k);
        }
        
        Assert.That(Tree.Count, Is.EqualTo(inserted.Count));
        Assert.That(Tree.InOrder().Select(x => x.Key), Is.Ordered);
    }
    
    #region Splay Tests
    
    private void AssertSplayProperty(int expectedKey)
    {
        if (!Tree.GetType().Name.StartsWith("SplayTree"))
        {
            return;
        }
        
        Type bstType = Tree.GetType();
        
        FieldInfo? rootField = null;
        Type? currentType = bstType;
        while (currentType != null && rootField == null)
        {
            rootField = currentType.GetField("Root", BindingFlags.NonPublic | BindingFlags.Instance);
            currentType = currentType.BaseType;
        }
        
        if (rootField == null)
        {
            Assert.Fail("Could not find protected field 'Root' via reflection.");
        }
        
        object? rootNode = rootField?.GetValue(Tree);
        
        Assert.That(rootNode, Is.Not.Null, "Root should not be null after operation");
        
        PropertyInfo? keyProperty = rootNode.GetType().GetProperty("Key");
        int actualKey = (int)keyProperty?.GetValue(rootNode)!;
        
        Assert.That(actualKey, Is.EqualTo(expectedKey),
            $"Splay violation: after accessing key {expectedKey}, it must become the Root.");
    }
    
    [Test]
    public void Test_SplayTree_RootMovement()
    {
        Tree.Add(10, "Ten");
        Tree.Add(20, "Twenty");
        Tree.Add(5, "Five");
        AssertSplayProperty(5);
        
        _ = Tree.ContainsKey(20);
        AssertSplayProperty(20);
        
        _ = Tree[10];
        AssertSplayProperty(10);
    }
    
    #endregion
}