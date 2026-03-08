using TreeDataStructures.Implementations.AVL;
using TreeDataStructures.Implementations.BST;
using TreeDataStructures.Implementations.RedBlackTree;
using TreeDataStructures.Implementations.Splay;
using TreeDataStructures.Implementations.Treap;
using TreeDataStructures.Tests.Base;

namespace TreeDataStructures.Tests;

[TestFixture, Category("BST")]
public class BinarySearchTreeTests : GenericTreeTests<BinarySearchTree<int, string>>
{
    #region Обходы (Traversals)
    /// <summary>
    /// Тест проверяет классические порядки обхода.
    /// Для BST:
    /// Root=10, Left=5, Right=15
    /// InOrder: 5, 10, 15
    /// PreOrder: 10, 5, 15
    /// PostOrder: 5, 15, 10
    /// </summary>
    [Test]
    public void Test_Traversals_Order()
    {
        Tree.Add(10, "Root");
        Tree.Add(5, "Left");
        Tree.Add(15, "Right");
        
        int[] inOrder = Tree.InOrder().Select(x => x.Key).ToArray();
        int[] preOrder = Tree.PreOrder().Select(x => x.Key).ToArray();
        int[] postOrder = Tree.PostOrder().Select(x => x.Key).ToArray();
        
        Assert.Multiple(() =>
        {
            Assert.That(inOrder, Is.EqualTo(new[] { 5, 10, 15 }), "InOrder failed");
            Assert.That(preOrder, Is.EqualTo(new[] { 10, 5, 15 }), "PreOrder failed");
            Assert.That(postOrder, Is.EqualTo(new[] { 5, 15, 10 }), "PostOrder failed");
        });
    }
    
    [Test]
    public void Test_Reverse_Traversals()
    {
        Tree.Add(10, "Root");
        Tree.Add(5, "Left");
        Tree.Add(15, "Right");
        
        int[] inOrderRev = Tree.InOrderReverse().Select(x => x.Key).ToArray();
        int[] preOrderRev = Tree.PreOrderReverse().Select(x => x.Key).ToArray();
        int[] postOrderRev = Tree.PostOrderReverse().Select(x => x.Key).ToArray();
        
        Assert.Multiple(() =>
        {
            Assert.That(inOrderRev, Is.EqualTo(new[] { 15, 10, 5 }), "InOrderReverse failed");
            Assert.That(preOrderRev, Is.EqualTo(new[] { 15, 5, 10 }), "PreOrderReverse failed");
            Assert.That(postOrderRev, Is.EqualTo(new[] { 10, 15, 5 }), "PostOrderReverse failed");
        });
    }
    #endregion
}

[TestFixture, Category("AVL")]
public class AvlTests : GenericTreeTests<AvlTree<int, string>> { }

[TestFixture, Category("RB")]
public class RedBlackTests : GenericTreeTests<RedBlackTree<int, string>> { }

[TestFixture, Category("Splay")]
public class SplayTests : GenericTreeTests<SplayTree<int, string>> { }

[TestFixture, Category("Treap")]
public class TreapTests : GenericTreeTests<Treap<int, string>> { }