using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }

        if (Comparer.Compare(root.Key, key) <= 0)
        {
            var (rl, rr) = Split(root.Right, key);
            root.Right = rl;
            if (rl != null) rl.Parent = root;

            if (rr != null) rr.Parent = null;

            root.Parent = null;
            return (root, rr);
        }
        else
        {
            var (ll, lr) = Split(root.Left, key);
            root.Left = lr;
            if (lr != null) lr.Parent = root;

            if (ll != null) ll.Parent = null;

            root.Parent = null;
            return (ll, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Priority > right.Priority)
        {
            var newRight = Merge(left.Right, right);
            left.Right = newRight;
            if (newRight != null) newRight.Parent = left;
            return left;
        }
        else
        {
            var newLeft = Merge(left, right.Left);
            right.Left = newLeft;
            if (newLeft != null) newLeft.Parent = right;
            return right;
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? existing = FindNode(key);
        if (existing != null)
        {
            existing.Value = value;
            return;
        }

        TreapNode<TKey, TValue> newNode = CreateNode(key, value);
        var (left, right) = Split(Root, key);

        var temp = Merge(left, newNode);
        Root = Merge(temp, right);
        if (Root != null) Root.Parent = null;

        Count++;
        OnNodeAdded(newNode);
    }

    public override bool Remove(TKey key)
    {
        TreapNode<TKey, TValue>? node = FindNode(key);
        if (node == null) return false;

        var parent = node.Parent;
        // Merge children to create a replacement tree
        TreapNode<TKey, TValue>? merged = Merge(node.Left, node.Right);

        // Replace node with merged
        Transplant(node, merged);

        Count--;
        OnNodeRemoved(parent, merged);
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
    }
    
}