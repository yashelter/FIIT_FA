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
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) SplitAndKeyGoLeft(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
            return (null, null);

        int cmp = Comparer.Compare(root.Key, key);

        if (cmp <= 0)
        {
            var (leftPart, rightPart) = SplitAndKeyGoLeft(root.Right, key);
            root.Right = leftPart;

            if (leftPart != null) leftPart.Parent = root;
            if (rightPart != null) rightPart.Parent = null;

            return (root, rightPart);
        }
        else
        {
            var (leftPart, rightPart) = SplitAndKeyGoLeft(root.Left, key);
            root.Left = rightPart;

            if (rightPart != null) rightPart.Parent = root;
            if (leftPart != null) leftPart.Parent = null;

            return (leftPart, root);
        }
    }

    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) SplitAndKeyGoRight(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
            return (null, null);

        int cmp = Comparer.Compare(root.Key, key);

        if (cmp < 0)
        {
            var (leftPart, rightPart) = SplitAndKeyGoRight(root.Right, key);
            root.Right = leftPart;

            if (leftPart != null) leftPart.Parent = root;
            if (rightPart != null) rightPart.Parent = null;

            return (root, rightPart);
        }
        else
        {
            var (leftPart, rightPart) = SplitAndKeyGoRight(root.Left, key);
            root.Left = rightPart;

            if (rightPart != null) rightPart.Parent = root;
            if (leftPart != null) leftPart.Parent = null;

            return (leftPart, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null || right == null)
        {
            if (left != null)
            {
                if (right != null) right.Parent = null;
                return left;
            }
            else
            {
                if (left != null) left.Parent = null;
                return right;
            }
        }
        if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if (left.Right != null) left.Right.Parent = left;
            left.Parent = null;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            if (right.Left != null) right.Left.Parent = right;
            right.Parent = null;
            return right;
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        var existing = FindNode(key);
        if (existing != null)
        {
            existing.Value = value;
            return;
        }

        TreapNode<TKey, TValue> newNode = CreateNode(key, value);

        var (left, right) = SplitAndKeyGoLeft(Root, key);

        var newLeft = Merge(left, newNode);

        Root = Merge(newLeft, right);

        if (Root != null) Root.Parent = null;

        Count++;
    }

    public override bool Remove(TKey key)
    {
        var (left, right) = SplitAndKeyGoRight(Root, key);

        var (middle, rest) = SplitAndKeyGoLeft(right, key);

        if (middle != null)
        {
            Root = Merge(left, rest);
            if (Root != null) Root.Parent = null;
            Count--;
            return true;
        }

        Root = Merge(left, right);
        if (Root != null) Root.Parent = null;
        return false;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode) { }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child) { }
    
}