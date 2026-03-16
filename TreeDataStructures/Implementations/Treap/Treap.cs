using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    protected virtual (TreapNode<TKey, TValue>?, TreapNode<TKey, TValue>?)
        Split(TreapNode<TKey, TValue>? node, TKey key)
    {
        if (node is null) return (null, null);

        var cmp = Comparer.Compare(node.Key, key);

        if (cmp <= 0)
        {
            var (left, right) = Split(node.Right, key);
            
            node.Right = left;
            left?.Parent = node;
            node.Parent = null;
            right?.Parent = null;
            
            return (node, right);
        } 
        else 
        {
            var (left, right) = Split(node.Left, key);
            
            node.Left = right;
            right?.Parent = node;
            node.Parent = null;
            left?.Parent = null;
            
            return (left, node);
        }
    }

    protected virtual TreapNode<TKey, TValue>? Merge(
        TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left is null)
        {
            right?.Parent = null;
            
            return right;
        }

        if (right is null)
        {
            left.Parent = null;
            
            return left;
        }

        if (left.Priority >= right.Priority)
        {
            left.Right = Merge(left.Right, right);
            
            if (left.Right is { } leftRight) leftRight.Parent = left;
            
            left.Parent = null;
            
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);

            if (right.Left is { } rightLeft) rightLeft.Parent = right;

            right.Parent = null;

            return right;
        }
    }

    public override void Add(TKey key, TValue value)
    {
        if (FindNode(key) is { } existing)
        {
            existing.Value = value;
            
            return;
        }

        var node = CreateNode(key, value);
        
        var (left, right) = Split(Root, key);
        
        Root = Merge(Merge(left, node), right);
        Count++;
        
        OnNodeAdded(node);
    }

    public override bool Remove(TKey key)
    {
        TreapNode<TKey, TValue>? removed = null;
        TreapNode<TKey, TValue>? rebalanceParent = null;
        TreapNode<TKey, TValue>? replacement = null;

        TreapNode<TKey, TValue>? Erase(TreapNode<TKey, TValue>? node)
        {
            if (node is null) return null;

            var cmp = Comparer.Compare(key, node.Key);

            switch (cmp)
            {
                case < 0:
                {
                    node.Left = Erase(node.Left);
                    if (node.Left is { } left) left.Parent = node;
                    node.Parent = null;

                    return node;
                }
                case > 0:
                {
                    node.Right = Erase(node.Right);
                    if (node.Right is { } right) right.Parent = node;
                    node.Parent = null;

                    return node;
                }
            }

            removed = node;
            rebalanceParent = node.Parent;
            replacement = Merge(node.Left, node.Right);

            node.Left = null;
            node.Right = null;
            node.Parent = null;

            return replacement;
        }

        Root = Erase(Root);

        if (removed is null) return false;

        Count--;
        
        OnNodeRemoved(rebalanceParent, replacement);
        
        return true;
    }
}